using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.Attributes;
using Excel = Microsoft.Office.Interop.Excel;
using WF = System.Windows.Forms;
using System.Collections;
using System.Text.RegularExpressions;
using System.IO;

namespace TerrTools
{
    [Transaction(TransactionMode.Manual)]
    class ScheduleExporter : IExternalCommand
    {
        private Document doc;
        private ScheduleExportOptions opt { get; set; }
        private string GetSaveFolder()
        {
            WF.FolderBrowserDialog dialog = new WF.FolderBrowserDialog();
            dialog.ShowNewFolderButton = true;
            dialog.RootFolder = Environment.SpecialFolder.Desktop;
            if (dialog.ShowDialog() == WF.DialogResult.OK)
            {
                return dialog.SelectedPath;
            }
            else return null;
        }

        private string GetSaveFile()
        {
            WF.SaveFileDialog dialog = new WF.SaveFileDialog();
            dialog.Filter = "Excel|*.xlsx";
            if (dialog.ShowDialog() == WF.DialogResult.OK)
            {
                return dialog.FileName;
            }
            else return null;
        }


        private void FillRange(Excel.Worksheet sheet, int startR, int startC, RevitDataSection section)
        {
            Excel.Range range = sheet.Range[sheet.Cells[startR, startC],
                sheet.Cells[startR + section.Height - 1, startC + section.Width - 1]];
            range.Value = section.ValueMatrix;
        }

        private string SafeName(string name)
        {
            Regex pattern = new Regex(@"[\\/\?\*:\[\] ]");
            name = pattern.Replace(name, "");
            int length = name.Length < 31 ? name.Length : 30;
            name = name.Substring(0, length);
            return name;
        }

        private void SafeSave(Excel.Workbook wb, string path)
        {
            try
            {
                wb.SaveAs(path);
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                WF.MessageBox.Show(string.Format("Ошибка при сохранения файла {0}. " +
                    "Возможно, этот документ уже открыт или у вас нет прав доступа к папке", path),
                    "Ошибка", WF.MessageBoxButtons.OK, WF.MessageBoxIcon.Error);
            }
            finally
            {
                wb.Close();
            }
        }

        private void FillHeader(Excel.Worksheet ws, RevitDataTable table)
        {
            var headerRow = table.Header;
            if (headerRow != null && headerRow.Width > 0)
            {
                Excel.Range range = ws.Range[ws.Cells[1, 1],
                ws.Cells[1, headerRow.Width]];
                range.Value = headerRow.RowValues;
                range.Font.Bold = true;
                range.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightGray);
            }
        }

        private void WriteTable(Excel.Workbook wb, RevitDataTable table)
        {
            int margin = 1; // количество пустых строк между секциями
            int currentRow = 1;
            Excel.Worksheet ws;
            switch (opt.SplittingData)
            {
                case SplitDataOptions.NoSplit:
                    ws = wb.Sheets.Add();
                    ws.Name = SafeName(table.Name);
                    if (opt.ShowHeaders) FillHeader(ws, table);
                    currentRow = opt.ShowHeaders ? currentRow + 1 : currentRow;
                    foreach (var sect in table.Sections)
                    {
                        FillRange(ws, currentRow, 1, sect);
                        currentRow += sect.Height;
                    }
                    break;

                case SplitDataOptions.OneSheet:
                    ws = wb.Sheets.Add();
                    ws.Name = SafeName(table.Name);
                    if (opt.ShowHeaders) FillHeader(ws, table);
                    currentRow = opt.ShowHeaders ? currentRow + 1 : currentRow;
                    foreach (var sect in table.Sections)
                    {
                        Excel.Range range = ws.Range[ws.Cells[currentRow, 1], ws.Cells[currentRow, sect.Width]];
                        range.Merge();
                        range.Value = sect.Name;
                        range.Font.Bold = true;
                        range.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightSlateGray);                        

                        FillRange(ws, currentRow + 1, 1, sect);
                        currentRow += sect.Height + 1 + margin;
                    }
                    break;

                case SplitDataOptions.MultipleSheet:
                    currentRow = opt.ShowHeaders ? currentRow + 1 : currentRow;
                    foreach (var sect in table.Sections)
                    {
                        ws = wb.Sheets.Add();
                        ws.Name = SafeName(table.Name + "_" + sect.Name);
                        if (opt.ShowHeaders) FillHeader(ws, table);                        
                        FillRange(ws, currentRow, 1, sect);
                    }
                    break;
            }
        }

        private void ExcelExport(string path, List<RevitDataTable> tables)
        {
            Excel.Application excelApp = new Excel.Application();
            excelApp.DisplayAlerts = false;
            Excel.Workbook wb;
            string savepath;
            switch (opt.SplittingFile)
            {
                case SplitFileOptions.MultipleFiles:
                    foreach (var table in tables)
                    {
                        wb = excelApp.Workbooks.Add();
                        WriteTable(wb, table);
                        savepath = Path.Combine(path, table.Name + ".xlsx");
                        SafeSave(wb, savepath);
                    }
                    break;

                case SplitFileOptions.OneFileMultiSheet:
                    wb = excelApp.Workbooks.Add();
                    foreach (var table in tables)
                    {
                        WriteTable(wb, table);
                    }
                    SafeSave(wb, path);
                    break;

                case SplitFileOptions.OneFileOneSheet:
                    wb = excelApp.Workbooks.Add();
                    RevitDataTable unitedTable;
                    if (opt.MergeSections) unitedTable = RevitDataTable.Merge(tables);                    
                    else unitedTable = RevitDataTable.Join(tables);
                    WriteTable(wb, unitedTable);
                    SafeSave(wb, path);
                    break;
            }
            excelApp.Quit();
        }


        private RevitDataTable GetRevitDataTable(ViewSchedule schedule)
        {
            ScheduleDefinition scheduleDefinition = schedule.Definition;
            TableSectionData bodyData = schedule.GetTableData().GetSectionData(SectionType.Body);
            TableSectionData headerData = schedule.GetTableData().GetSectionData(SectionType.Header);

            RevitDataTable dataTable = new RevitDataTable(schedule.Name);
            RevitDataSection dataSection = new RevitDataSection("Без названия");
            dataTable.Sections.Add(dataSection);
            // заголовки
            int start_i;
            if (scheduleDefinition.ShowHeaders)
            {                
                RevitDataRow header = new RevitDataRow(0);
                for (int col = 0; col < bodyData.NumberOfColumns; col++)
                {
                    RevitDataCell dataCell = new RevitDataCell(
                        schedule.GetCellText(SectionType.Body, 0, col),
                        bodyData.GetCellType(0, col),
                        bodyData.GetCellParamId(col));
                    header.Cells.Add(dataCell);
                }
                start_i = 1;
                dataTable.Header = header;
            }
            else
            {
                start_i = 0;
            }
            //ищем секции
            for (int row = start_i; row < bodyData.NumberOfRows; row++)
            {
                if (bodyData.GetCellType(row, 0) == CellType.Text
                    && bodyData.GetMergedCell(row, 0).Right == bodyData.LastColumnNumber)
                {
                    string header = bodyData.GetCellText(row, 0);
                    header = string.IsNullOrEmpty(header) ? "Без названия" : header;
                    dataSection = new RevitDataSection(header);
                    dataTable.Sections.Add(dataSection);
                    continue;
                }

                RevitDataRow dataRow = new RevitDataRow(row);
                for (int col = 0; col < bodyData.NumberOfColumns; col++)
                {
                    RevitDataCell dataCell = new RevitDataCell(
                        schedule.GetCellText(SectionType.Body, row, col),
                        bodyData.GetCellType(row, col),
                        bodyData.GetCellParamId(col));
                    dataRow.Cells.Add(dataCell);
                }
                dataSection.Rows.Add(dataRow);
            }
            if (dataTable["Без названия"].Rows.Count == 0) dataTable.Sections.Remove(dataTable["Без названия"]);
            return dataTable;
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            doc = commandData.Application.ActiveUIDocument.Document;
            var viewSchedules = new FilteredElementCollector(doc).OfClass(typeof(ViewSchedule))
                                .WhereElementIsNotElementType()
                                .Where(x => !x.Name.Contains("Ведомость изменений"))
                                .ToList();
            var form = new UI.ExportSchedulesForm(viewSchedules);
            if (form.DialogResult != WF.DialogResult.OK || form.Result.Count() == 0) return Result.Cancelled;
            opt = form.ExportOptions;

            // если много файлов - выбираем папку, если нет - файл
            string path;
            if (opt.SplittingFile == SplitFileOptions.MultipleFiles) path = GetSaveFolder();
            else path = GetSaveFile();
            if (path == null) return Result.Cancelled;


            List<RevitDataTable> tables = new List<RevitDataTable>();
            foreach (ViewSchedule schedule in form.Result)
            {
                var table = GetRevitDataTable(schedule);
                tables.Add(table);
            }
            ExcelExport(path, tables);

            TaskDialog.Show("Успешно", "Экспорт завершен");
            return Result.Succeeded;
        }
    }

    public enum SplitFileOptions { MultipleFiles, OneFileMultiSheet, OneFileOneSheet }
    public enum SplitDataOptions { NoSplit, OneSheet, MultipleSheet }
    public class ScheduleExportOptions
    {
        public SplitFileOptions SplittingFile { get; set; }
        public SplitDataOptions SplittingData { get; set; }
        public bool MergeSections { get; set; }
        public bool ShowHeaders { get; set; }
        public ScheduleExportOptions(SplitFileOptions fileSplit, SplitDataOptions dataSplit, 
                                    bool merge, bool showheaders)
        {
            SplittingFile = fileSplit;
            SplittingData = dataSplit;
            MergeSections = merge;
            ShowHeaders = showheaders;
        }
    }

    class RevitDataCell
    {
        public CellType Type { get; }
        public string Value { get; }
        public ElementId ParameterId { get; }
        public RevitDataCell(string value, CellType cellType, ElementId parameterId)
        {
            Value = value;
            Type = cellType;
            ParameterId = parameterId;
        }
        public RevitDataCell()
        {
            Value = "";
            Type = CellType.Text;
            ParameterId = ElementId.InvalidElementId;
        }
    }

    class RevitDataRow
    {
        public List<RevitDataCell> Cells { get; set; }
        public int Width { get => Cells.Count; }
        public int Index { get; set; }
        public RevitDataCell this[int c] { get => Cells.Count() > c ? Cells[c] : new RevitDataCell(); }
        public string[] RowValues { get => Cells.Select(x => x.Value).ToArray(); }
        public RevitDataRow(int index)
        {
            Cells = new List<RevitDataCell>();
            Index = index;
        }
    }

    class RevitDataSection
    {
        public List<RevitDataRow> Rows { get; set; }
        /// <summary>
        /// Количество строчек в секции
        /// </summary>
        public int Height { get => Rows.Count; }

        /// <summary>
        /// Максимальная длина строчек в секции
        /// </summary>
        public int Width { get => Rows.Max(x => x.Width); }
        public RevitDataCell this[int r, int c] { get => Rows[r][c]; }

        public string[,] ValueMatrix
        {
            get
            {
                string[,] values = new string[Height, Width];
                for (int r = 0; r < Height; r++)
                {
                    for (int c = 0; c < Width; c++)
                    {
                        values[r, c] = Rows[r][c].Value;
                    }
                }
                return values;
            }
        }
        public string Name { get; set; }
        public RevitDataSection(string name)
        {
            Rows = new List<RevitDataRow>();
            Name = name;
        }
        public static RevitDataSection operator+ (RevitDataSection a, RevitDataSection b)
        {
            if (a.Name == b.Name)
            {
                RevitDataSection result = new RevitDataSection(a.Name);
                result.Rows.AddRange(a.Rows);
                result.Rows.AddRange(b.Rows);
                return result;
            }
            else throw new ArgumentException("Имена секций отличаются");
        }
    }

    class RevitDataTable : IEnumerable
    {
        public List<RevitDataSection> Sections { get; set; }
        public RevitDataRow Header { get; set; }      
        public HashSet<string> SectionNames
        {
            get
            {
                return new HashSet<string>(Sections.Select(x => x.Name));
            }
        }
        public string Name { get; set; }
        public IEnumerator GetEnumerator()
        {
            return Sections.GetEnumerator();
        }
        public RevitDataTable(string name)
        {
            Sections = new List<RevitDataSection>();
            Name = name;
        }
        public RevitDataSection this[string sectionName]
        {
            get
            {
                var sects = Sections.Where(x => x.Name == sectionName);
                return sects.FirstOrDefault();
            }
        }
        public void Merge(RevitDataTable anotherTable)
        {
            Name = "Merged";
            foreach (RevitDataSection s in anotherTable)
            {
                var rightSection = this[s.Name];
                if (rightSection != null) rightSection.Rows.AddRange(s.Rows);
                else Sections.Add(s);
            }
        }

        static public RevitDataTable Merge(List<RevitDataTable> tables)
        {
            RevitDataTable merged = new RevitDataTable("Merged");
            merged.Header = tables[0].Header;
            HashSet<string> sectNames = new HashSet<string>();
            foreach (RevitDataTable table in tables) sectNames.UnionWith(table.SectionNames);
            foreach (string sectName in sectNames)
            {
                RevitDataSection mergedSection = new RevitDataSection(sectName);
                foreach (RevitDataTable table in tables)
                {
                    var s = table[sectName];
                    mergedSection = s != null ? mergedSection + s : mergedSection;
                }
                merged.Sections.Add(mergedSection);
            }
            return merged;
        }

        static public RevitDataTable Join(List<RevitDataTable> tables)
        {
            RevitDataTable joined = new RevitDataTable("Joined");
            joined.Header = tables[0].Header;
            foreach(RevitDataTable table in tables)
            {
                foreach(RevitDataSection sect in table)
                {
                    sect.Name = table.Name + "_" + sect.Name;
                    joined.Sections.Add(sect);
                }
            }
            return joined;
        }

    }
}
