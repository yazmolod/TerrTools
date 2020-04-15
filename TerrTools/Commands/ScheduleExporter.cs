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
using Forms = System.Windows.Forms;
using System.Collections;

namespace TerrTools
{
    [Transaction(TransactionMode.Manual)]
    class ScheduleExporter : IExternalCommand
    {
        private Document doc;
        private string GetSaveFile()
        {
            Forms.SaveFileDialog dialog = new Forms.SaveFileDialog();
            dialog.Filter = "Файл Excel (*.xlsx)|*.xlsx";
            dialog.RestoreDirectory = true;
            if (dialog.ShowDialog() == Forms.DialogResult.OK)
            {
                return dialog.FileName;
            }
            else return null;
        }


        private void FillExcelWorksheet(Excel.Worksheet sheet, RevitDataSection section)
        {
            try { sheet.Name = section.Name; }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                TaskDialog.Show("Ошибка", string.Format(
                    "\"{0}\" невозможно использовать как имя для листа. Попробуйте сократить имя, убрать непечатемые символы или дать уникальное имя",
                    section.Name));
            }
            finally
            {
                Excel.Range range = sheet.Range[sheet.Cells[1, 1], sheet.Cells[section.Height, section.Width]];
                range.Value = section.ValueMatrix;
            }
        }

        private void ExcelExport(string filepath, RevitDataTable table)
        {
            Excel.Application excelApp = new Excel.Application();
            Excel.Workbook wb = excelApp.Workbooks.Add();

            // заполняем данными
            foreach (var sect in table.Sections)
            {
                Excel.Worksheet ws = wb.Sheets.Add();
                FillExcelWorksheet(ws, sect);
            }

            wb.SaveAs(filepath);
            wb.Close();
        }

        private RevitDataTable GetRevitDataTable(ViewSchedule schedule)
        {
            TableSectionData bodyData = schedule.GetTableData().GetSectionData(SectionType.Body);
            RevitDataTable dataTable = new RevitDataTable();
            RevitDataSection dataSection = null;
            for (int row = 0; row < bodyData.NumberOfRows; row++)
            {
                if (!string.IsNullOrEmpty(bodyData.GetCellText(row, 0))
                    && bodyData.GetCellType(row, 0) == CellType.Text
                    && bodyData.GetMergedCell(row, 0).Right == bodyData.LastColumnNumber)
                {
                    string header = bodyData.GetCellText(row, 0);
                    dataSection = new RevitDataSection(header);
                    dataTable.Sections.Add(dataSection);
                    continue;
                }
                if (dataSection == null) continue;

                RevitDataRow dataRow = new RevitDataRow();
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
            return dataTable;
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            doc = commandData.Application.ActiveUIDocument.Document;
            var viewSchedules = new FilteredElementCollector(doc).OfClass(typeof(ViewSchedule)).WhereElementIsNotElementType().ToArray();
            var form = new UI.ExportSchedulesForm(viewSchedules);
            if (form.DialogResult != Forms.DialogResult.OK || form.Result.Count() == 0) return Result.Cancelled;

            string path = GetSaveFile();
            if (path == null) return Result.Cancelled;

            RevitDataTable mergedTable = new RevitDataTable();
            foreach (ViewSchedule schedule in form.Result)
            {
                var table = GetRevitDataTable(schedule);
                mergedTable.Merge(table);
            }           
            ExcelExport(path, mergedTable);
            TaskDialog.Show("Успешно", "Экспорт завершен");
            return Result.Succeeded;
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
        public RevitDataCell this[int c] { get => Cells.Count() > c ? Cells[c] : new RevitDataCell(); }
        public string[] RowValues { get => Cells.Select(x => x.Value).ToArray(); }
        public RevitDataRow()
        {
            Cells = new List<RevitDataCell>();
        }
    }

    class RevitDataSection
    {
        public List<RevitDataRow> Rows { get; set; }
        public int Height { get => Rows.Count; }
        public int Width { get => Rows.Max(x => x.Width); }
        public RevitDataCell this[int r,int c] { get => Rows[r][c]; }
        public string[,] ValueMatrix {
            get
            {
                string[,] values = new string[Height, Width];
                for (int r = 0; r < Height; r++)
                {
                    for (int c = 0; c < Width; c++)
                    {
                        values[r,c] = Rows[r][c].Value;
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
    }

    class RevitDataTable:IEnumerable
    {
        public List<RevitDataSection> Sections { get; set; }
        public IEnumerator GetEnumerator()
        {
            return Sections.GetEnumerator();
        }
        public RevitDataTable()
        {
            Sections = new List<RevitDataSection>();
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
            foreach (RevitDataSection s in anotherTable)
            {
                var rightSection = this[s.Name];
                if (rightSection != null) rightSection.Rows.AddRange(s.Rows);
                else Sections.Add(s);
            }
        }
        
    }    
}
