using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.Drawing.Printing;
using System.Diagnostics;
using System.IO;
using TerrTools.Properties;
using System.Reflection;

namespace TerrTools
{
    [Transaction(TransactionMode.Manual)]
    class PdfPrinting : IExternalCommand
    {
        UIDocument uidoc;
        Document doc { get => uidoc.Document; }
        string printerName { get; set; }
        PrintManager printManager;



        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            printManager = doc.PrintManager;

            string[] printers = PDFPrinterNamesIterator().ToArray();
            if (printers.Length == 0)
            {
                TaskDialog.Show("Ошибка", "Не найдены виртуальные принтеры для печати PDF. Операция отменена");
                return Result.Failed;
            }
            ViewSheetSet[] viewSheetSets = new FilteredElementCollector(doc).OfClass(typeof(ViewSheetSet)).Cast<ViewSheetSet>().ToArray();
            if (viewSheetSets.Length == 0)
            {
                TaskDialog.Show("Ошибка", "Не найден ни один набор листов в проекте. Операция отменена");
                return Result.Failed;
            }

            var ui = new UI.PdfPrintingForm(printers, viewSheetSets);
            if (ui.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                View[] views = null;
                switch (ui.SelectedOption)
                {
                    case UI.PdfPrintingForm.ExportViewOption.Current:
                        views = new View[] { doc.ActiveView };
                        break;

                    case UI.PdfPrintingForm.ExportViewOption.Selection:
                        try
                        {
                            views = uidoc.Selection.GetElementIds().Select(x => doc.GetElement(x)).Cast<View>().ToArray();
                        }
                        catch (InvalidCastException)
                        {
                        }
                        break;

                    case UI.PdfPrintingForm.ExportViewOption.Set:
                        List<View> list = new List<View>();
                        foreach (View view in ui.Set.Views)
                        {
                            list.Add(view);
                        }
                        views = list.ToArray();
                        break;
                }
                if (views == null)
                {
                    TaskDialog.Show("Ошибка", $"Выделите листы в диспетчере проекта перед экспортом. Операция отменена");
                    return Result.Cancelled;
                }

                using (TransactionGroup transGroup = new TransactionGroup(doc))
                {
                    transGroup.Start("Настройка принтера");

                    ViewSheetSet viewSheetSet = ui.Set;
                    printerName = ui.Printer;
                    printManager.SelectNewPrintDriver(printerName);
                    printManager = doc.PrintManager;

                    foreach (View view in views)
                    {
                        ViewSheet viewSheet = view as ViewSheet;
                        if (viewSheet != null)
                        {
                            PrintTitleBlock(viewSheet);
                        }
                        else
                        {
                            TaskDialog.Show("Ошибка", $"Вид {view.Name}: Поддерживаются только листы. Вид будет пропущен");
                        }
                    }
                    transGroup.Assimilate();
                }
                return Result.Succeeded;
            }
            else
            {
                return Result.Cancelled;
            }
        }


        private IEnumerable<string> PDFPrinterNamesIterator()
        {
            foreach (string installedPrinter in PrinterSettings.InstalledPrinters)
            {
                printManager.SelectNewPrintDriver(installedPrinter);
                if (printManager.IsVirtual == VirtualPrinterType.AdobePDF)
                {
                    yield return installedPrinter;
                }
            }
        }

        private void SetDefaultPrintManagerOptions(ViewSheet viewSheet)
        {
            printManager.PrintRange = Autodesk.Revit.DB.PrintRange.Current;
            printManager.PrintToFile = true;
            string folder = Path.GetDirectoryName(doc.PathName);
            string filename = $"Лист {viewSheet.SheetNumber.PadLeft(5, '0')} - {viewSheet.Name}.pdf";
            string filepath = Path.Combine(folder, filename);
            printManager.PrintToFileName = filepath;
        }

        private void PrintTitleBlock(ViewSheet viewSheet)
        {
            TitleBlockPrintSize[] blockPrintSizes;
            Tuple<int, int> sheetSize = GetViewTitleBlocksSizes(viewSheet);
            if (sheetSize != null)
            {
                blockPrintSizes = GetSystemPaperSize(sheetSize).ToArray();                
            }
            else
            {
                var size = AskSheetSize(viewSheet, printerName);                
                blockPrintSizes = size != null ? new TitleBlockPrintSize[] { size } : new TitleBlockPrintSize[] { };
            }
                // пытаемся найти уже существующую настройку с подходящими параметрами 
                foreach (TitleBlockPrintSize blockPrintSize in blockPrintSizes)
                {
                    foreach (PrintSetting setting in PrintSettingsIterator())
                    {
                        if (SettingIsOk(setting, blockPrintSize))
                        {
                            SetupAndPrint(viewSheet, setting);
                            return;
                        }
                    }
                }
            // ничего не нашли - делаем сами
            try
            {
                IPrintSetting new_setting = CreateSetting(blockPrintSizes[0]);
                SetupAndPrint(viewSheet, new_setting);
            }
            catch
            {
                TaskDialog td = new TaskDialog($"Ошибка");
                td.MainIcon = TaskDialogIcon.TaskDialogIconWarning;
                td.MainInstruction = $"Лист {viewSheet.Name}: не удалось подобрать настройки или создать настройки для листа";
                td.Show();
            }

        }

        private TitleBlockPrintSize AskSheetSize(ViewSheet viewSheet, string printerName)
        {
            var ui = new UI.AskPaperFormatForm(viewSheet.Name, printerName);
            if (ui.ShowDialog() == System.Windows.Forms.DialogResult.OK) return new TitleBlockPrintSize(ui.PaperSize, ui.IsRotated);
            else return null;
        }

        private void SetupAndPrint(ViewSheet viewSheet, IPrintSetting setting)
        {
            using (Transaction tr = new Transaction(doc, "Обновление принтера"))
            {
                tr.Start();
                SetDefaultPrintManagerOptions(viewSheet);
                printManager.PrintSetup.CurrentPrintSetting = setting;
                printManager.Apply();
                tr.Commit();
            }
            printManager.SubmitPrint(viewSheet);
        }


        private bool SettingIsOk(PrintSetting setting, TitleBlockPrintSize blockPrintSize)
        {
            List<bool> flags = new List<bool>();
            try
            {
                PrintParameters pp = setting.PrintParameters;            
                flags.Add(pp.PaperPlacement == PaperPlacementType.Margins);
                flags.Add(pp.MarginType == MarginType.NoMargin);
                flags.Add(pp.ZoomType == ZoomType.Zoom);

                flags.Add(pp.Zoom == 100);

                flags.Add(pp.PaperSize.Name == blockPrintSize.Name);
                flags.Add(pp.PageOrientation == blockPrintSize.PageOrientation);

                return flags.All(x => x == true);
            }
            catch (Autodesk.Revit.Exceptions.InvalidOperationException)
            {
                return false;
            }
        }

        private IEnumerable<PrintSetting> PrintSettingsIterator()
        {
            var settingIds = doc.GetPrintSettingIds();
            foreach (ElementId id in settingIds)
            {
                PrintSetting setting = doc.GetElement(id) as PrintSetting;
                yield return setting;
            }
        }

        private IEnumerable<TitleBlockPrintSize> GetSystemPaperSize(Tuple<int, int> viewSize)
        {

            PrinterSettings pd = new PrinterSettings();
            pd.PrinterName = printerName;
            foreach (System.Drawing.Printing.PaperSize existed_size in pd.PaperSizes) {

                int paperWidth_mm = (int)Math.Round(existed_size.Width * 0.254);
                int paperHeight_mm = (int)Math.Round(existed_size.Height * 0.254);
                if (paperWidth_mm == viewSize.Item1 && paperHeight_mm == viewSize.Item2)
                {
                    yield return new TitleBlockPrintSize(existed_size, false);
                }
                else if (paperWidth_mm == viewSize.Item2 && paperHeight_mm == viewSize.Item1)
                {
                    yield return new TitleBlockPrintSize(existed_size, true);
                }
            }
        }

        private bool PrinterHasSuchSize(Tuple<int, int> viewSize)
        {
            return GetSystemPaperSize(viewSize).Count() > 0;
        }

        private Tuple<int, int> GetViewTitleBlocksSizes(ViewSheet viewSheet)
        {
            FamilyInstance[] title_blocks = new FilteredElementCollector(doc, viewSheet.Id).OfCategory(BuiltInCategory.OST_TitleBlocks).Cast<FamilyInstance>().ToArray();
            if (title_blocks.Length == 1)
            {
                FamilyInstance title_block = title_blocks[0];
                double instance_width_d = title_block.get_Parameter(BuiltInParameter.SHEET_WIDTH).AsDouble();
                double instance_height_d = title_block.get_Parameter(BuiltInParameter.SHEET_HEIGHT).AsDouble();
                int instance_width = (int)Math.Round(UnitUtils.ConvertFromInternalUnits(instance_width_d, DisplayUnitType.DUT_MILLIMETERS));
                int instance_height = (int)Math.Round(UnitUtils.ConvertFromInternalUnits(instance_height_d, DisplayUnitType.DUT_MILLIMETERS));
                Tuple<int, int> instance_size = new Tuple<int, int>(instance_width, instance_height);
                if (PrinterHasSuchSize(instance_size))
                {
                    return instance_size;
                }

                FamilySymbol title_block_symbol = title_block.Symbol;
                Parameter w_parameter = title_block_symbol.LookupParameter("Ширина");
                Parameter h_parameter = title_block_symbol.LookupParameter("Высота");
                if (w_parameter != null && h_parameter != null)
                {
                    double type_width_d = w_parameter.AsDouble();
                    double type_height_d = h_parameter.AsDouble();
                    int type_width = (int)Math.Round(UnitUtils.ConvertFromInternalUnits(type_width_d, DisplayUnitType.DUT_MILLIMETERS));
                    int type_height = (int)Math.Round(UnitUtils.ConvertFromInternalUnits(type_height_d, DisplayUnitType.DUT_MILLIMETERS));
                    Tuple<int, int> type_size = new Tuple<int, int>(type_width, type_height);
                    if (PrinterHasSuchSize(type_size))
                    {
                        return type_size;
                    }
                }
                return null;
            }
            else
            {
                TaskDialog td = new TaskDialog($"Ошибка");
                td.MainIcon = TaskDialogIcon.TaskDialogIconWarning;
                td.MainInstruction = title_blocks.Length == 0 ?
                    $"{viewSheet.Name}: На листе не найден ни один штамп и будет пропущен" :
                    $"{viewSheet.Name}: На листе обнаружено несколько штампов. Корректная печать невозможна";
                td.Show();
                return null;
            }
        }

        private PrintSetting FindSettingByName(string name)
        {
            return doc.GetPrintSettingIds().Select(x => doc.GetElement(x)).Where(x => x.Name == name).Cast<PrintSetting>().FirstOrDefault();
        }


        private IPrintSetting CreateSetting(TitleBlockPrintSize size)
        {
            var s = printManager.PrintSetup.CurrentPrintSetting;
            s.PrintParameters.PaperSize = size.ConvertToRevitPaperSize(printManager);
            s.PrintParameters.ZoomType = ZoomType.Zoom;
            s.PrintParameters.Zoom = 100;
            s.PrintParameters.PaperPlacement = PaperPlacementType.Margins;
            s.PrintParameters.MarginType = MarginType.NoMargin;
            s.PrintParameters.PageOrientation = size.PageOrientation;            

            using (Transaction tr = new Transaction(doc, "Добавление настроек принтера"))
            {
                tr.Start();
                printManager.PrintSetup.CurrentPrintSetting = s;
                string suffix = size.Rotated ? "А" : "К";
                string settingName = size.Name + suffix;
                try
                {
                    printManager.PrintSetup.SaveAs(settingName);
                }
                catch
                {
                    settingName += '_';
                    settingName += DateTime.Now.ToString("s").Replace(':', '-');
                    printManager.PrintSetup.SaveAs(settingName);
                }
                tr.Commit();
            }
            return s;
        }
    } 

    class TitleBlockPrintSize
    {
        public System.Drawing.Printing.PaperSize SystemPaperSize { get; }
        public bool Rotated { get; }
        public PageOrientationType PageOrientation { get => Rotated ? PageOrientationType.Landscape : PageOrientationType.Portrait; }
        public int Width { get => (int)Math.Round(SystemPaperSize.Width * 0.254); }
        public int Height { get => (int)Math.Round(SystemPaperSize.Height * 0.254); } 
        public string Name { get => SystemPaperSize.PaperName; }
        public TitleBlockPrintSize(System.Drawing.Printing.PaperSize size, bool rotated)
        {
            SystemPaperSize = size;
            Rotated = rotated;
        }
        public Autodesk.Revit.DB.PaperSize ConvertToRevitPaperSize(PrintManager manager)
        {
            foreach (Autodesk.Revit.DB.PaperSize ps in manager.PaperSizes)
            {
                if (SystemPaperSize.PaperName == ps.Name)
                {
                    return ps;
                }
            }
            return null;
        }
    }
}
