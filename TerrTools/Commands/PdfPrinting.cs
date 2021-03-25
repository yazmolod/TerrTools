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
        string printerName
        {
            get { return printManager.PrinterName; }
            set
            {
                printManager.SelectNewPrintDriver(value);
                printManager.Apply();
            }
        }
        PrintManager printManager { get => doc.PrintManager; }



        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;

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

                printerName = ui.Printer;
                ViewSheetSet viewSheetSet = ui.Set;
                using (TransactionGroup transGroup = new TransactionGroup(doc))
                {
                    transGroup.Start("Настройка принтера");
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
                printerName = installedPrinter;
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
            string filename = viewSheet.Name + ".pdf";
            string filepath = Path.Combine(folder, filename);
            printManager.PrintToFileName = filepath;
        }

        private void PrintTitleBlock(ViewSheet viewSheet)
        {            
            Tuple<int, int> sheetSize = GetViewTitleBlocksSizes(viewSheet);
            if (sheetSize != null)
            {
                TitleBlockPrintSize[] blockPrintSizes = GetSystemPaperSize(sheetSize).ToArray();

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
                    PrintSetting setting = CreateSetting(blockPrintSizes[0]);
                    SetupAndPrint(viewSheet, setting);
                }
                catch
                {
                    TaskDialog td = new TaskDialog($"Ошибка");
                    td.MainIcon = TaskDialogIcon.TaskDialogIconWarning;
                    td.MainInstruction = $"Лист {viewSheet.Name}: не удалось подобрать настройки или создать настройки для размера листа {sheetSize.Item1}x{sheetSize.Item2}";
                    td.Show();
                }                
            }
        }

        private void SetupAndPrint(ViewSheet viewSheet, PrintSetting setting)
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
            PrintParameters pp = setting.PrintParameters;

            try
            {
                flags.Add(pp.PaperPlacement == PaperPlacementType.Margins);
                flags.Add(pp.MarginType == MarginType.NoMargin);
                flags.Add(pp.ZoomType == ZoomType.Zoom);

                flags.Add(pp.Zoom == 100);

                flags.Add(pp.PaperSize.Name == blockPrintSize.Name);
                flags.Add(pp.PageOrientation == blockPrintSize.PageOrientation);

                return flags.All(x => x == true);
            }
            catch(Autodesk.Revit.Exceptions.InvalidOperationException)
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

        private IEnumerable<TitleBlockPrintSize> GetSystemPaperSize(Tuple<int,int> viewSize) 
        {

            PrinterSettings pd = new PrinterSettings();
            pd.PrinterName = printerName;
            foreach (System.Drawing.Printing.PaperSize existed_size in pd.PaperSizes) {

                int paperWidth_mm = (int) Math.Round(existed_size.Width * 0.254);
                int paperHeight_mm = (int) Math.Round(existed_size.Height * 0.254);
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

        private Tuple<int,int> GetViewTitleBlocksSizes(ViewSheet viewSheet)
        {
            Element[] title_blocks = new FilteredElementCollector(doc, viewSheet.Id).OfCategory(BuiltInCategory.OST_TitleBlocks).ToArray();
            if (title_blocks.Length == 1)
            {
                Element title_block = title_blocks[0];
                BoundingBoxXYZ bbox = title_block.get_BoundingBox(viewSheet);
                XYZ diff = bbox.Max - bbox.Min;
                int width = (int)Math.Round(UnitUtils.ConvertFromInternalUnits(diff.X, DisplayUnitType.DUT_MILLIMETERS));
                int height = (int)Math.Round(UnitUtils.ConvertFromInternalUnits(diff.Y, DisplayUnitType.DUT_MILLIMETERS));
                Tuple<int, int> size = new Tuple<int, int>(width, height);
                return size;
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

        private PrintSetting CreateSetting(TitleBlockPrintSize size)
        {
            // По неведомым причинам, идентичный код настройки новых параметров принтера  
            // работает в Python, но не работает в C#. Поэтому после двух безуспешных дней работы
            // я сдался и сделал этот ужас
            var ps = size.ConvertToRevitPaperSize(printManager);
            object[] input = new object[] { doc, ps, size.Rotated};
            string settingName = PythonExecuter.RunPythonScriptFromResource("TerrTools.Resources.CreatePrintSetting.py", input);
            PrintSetting setting = FindSettingByName(settingName); 
            return setting;
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
