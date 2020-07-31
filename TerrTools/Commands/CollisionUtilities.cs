using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Autodesk.Revit.DB;
using System.Globalization;
using Microsoft.Scripting.Utils;
using HtmlAgilityPack;
using System.IO;

namespace TerrTools
{
    public class CollisionReportTable
    {
        public string Path { get; }
        public string FolderPath { get => System.IO.Path.GetDirectoryName(Path); }
        public CollisionReportHeader Header { get; }
        public List<CollisionReportRow> Rows { get; }
        public CollisionReportRow this[int i] { get => Rows[i]; }
        public CollisionReportTable(string path)
        {
            Path = path;
            HtmlDocument report = new HtmlDocument();
            report.Load(path, Encoding.UTF8);
            var headerNodes = report.DocumentNode.SelectNodes("//table[@class='mainTable']/tr[@class='headerRow']");
            HtmlNodeCollection nodes;
            if (headerNodes != null)
            {
                Header = new CollisionReportHeader(headerNodes[1]);

                // обычная таблица
                nodes = report.DocumentNode.SelectNodes("//table[@class='mainTable']/tr[@class='contentRow']");
                //таблица с группами
                if (nodes == null)
                {
                    nodes = report.DocumentNode.SelectNodes("//table[@class='mainTable']/tr[@class='childRow']");
                    nodes.AddRange(report.DocumentNode.SelectNodes("//table[@class='mainTable']/tr[@class='childRowLast']"));
                }          
            }
            else
            {
                nodes = report.DocumentNode.SelectNodes("//div[@class='viewpoint']");
            }
            List<CollisionReportRow> rows = new List<CollisionReportRow>();
            foreach (var node in nodes) rows.Add(new CollisionReportRow(this, node));
            Rows = rows;
        }
    }

    public class CollisionReportHeader
    {
        public List<(string, string)> Cells = new List<(string, string)>();

        public CollisionReportHeader(HtmlNode node)
        {
            HtmlNodeCollection cells = node.SelectNodes("./td");
            for (int i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                string cellClass = cell.GetAttributeValue("class", null);
                string cellValue = cell.InnerText;
                Cells.Add((cellClass, cellValue));
            }
        }
    }

    public class CollisionReportRow
    {
        CollisionReportTable Table { get; }
        public string ImagePath { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public double Distance { get; set; }
        public string Grid { get; set; }
        public string Description { get; set; }
        public DateTime FoundDate { get; set; }
        public XYZ Point { get; set; }
        public ElementId E1_Id { get; set; }
        public ElementId E2_Id { get; set; }
        public string E1_Path { get; set; }
        public string E1_DocumentName {get => GetDocNameFromPath(E1_Path); }
        public string E2_Path { get; set; }
        public string E2_DocumentName { get => GetDocNameFromPath(E2_Path); }
        public string E1_Layer { get; set; }
        public string E2_Layer { get; set; }
        public string E1_Name { get; set; }
        public string E2_Name { get; set; }

        private string GetDocNameFromPath(string v)
        {
            string res;
            if (v == null) return "";
            else
            {
                res = Regex.Split(v, "&gt;")[2];
                return Regex.Match(res, @"[a-zA-Zа-яА-Я0-9_][a-zA-Zа-яА-Я0-9_ ]+").Value;
            }
        }

        public CollisionReportRow (CollisionReportTable parent, HtmlNode node)
        {
            Table = parent;
            switch (node.Name)
            {
                case "div":
                    SimpleRowParse(node);
                    break;

                case "tr":
                    TableRowParse(node);
                    break;

                default:
                    throw new NotSupportedException("Неопознанная строчка отчета");
            }
        }


        /// <summary>
        /// Выгружает данные из строчки простого html отчета о коллизиях
        /// </summary>
        /// <param name="node">Нод tr, class=contentRow, childRow или childRowLast из отчета</param>
        private void SimpleRowParse(HtmlNode node)
        {
            string href = node.SelectSingleNode("./a").GetAttributeValue("href", "");
            ImagePath = new Uri(System.IO.Path.Combine(Table.FolderPath, href)).LocalPath;
            bool firstElementFlag = true;
            foreach (HtmlNode pairnode in node.SelectNodes("./span[@class='namevaluepair']"))
            {
                var spans  = pairnode.SelectNodes("./span");
                var name = spans[0].InnerText;
                var value = spans[1].InnerText;                
                switch (name)
                {               
                    case "Имя":
                        Name = value;
                        break;

                    case "Расстояние":
                        Distance = double.Parse(Regex.Match(value, @"-?\d+\.\d+").Value.Replace('.', ','));
                        break;

                    case "Описание:":
                        Description = value;
                        break;

                    case "Статус":
                        Status = value;
                        break;

                    case "Точка конфликта":
                        MatchCollection matches = Regex.Matches(value, @"\d+\.\d+");
                        double[] coords = matches.Cast<Match>().Select(x => double.Parse(x.Value.Replace('.', ','))).ToArray();
                        double cx = UnitUtils.ConvertToInternalUnits(coords[0], DisplayUnitType.DUT_METERS);
                        double cy = UnitUtils.ConvertToInternalUnits(coords[1], DisplayUnitType.DUT_METERS);
                        double cz = UnitUtils.ConvertToInternalUnits(coords[2], DisplayUnitType.DUT_METERS);
                        Point = new XYZ(cx, cy, cz);
                        break;

                    case "Расположение сетки":
                        Grid = value;
                        break;

                    case "Дата создания":
                        string datacell = value;
                        FoundDate = DateTime.ParseExact(datacell, "yyyy/M/d \r\n  HH:mm", CultureInfo.InvariantCulture);
                        break;

                    case "ID объекта":
                        var id = new ElementId(int.Parse(value));
                        if (firstElementFlag) E1_Id = id;
                        else E2_Id = id;
                        break;

                    case "Слой":
                        if (firstElementFlag) E1_Layer = value;
                        else E2_Layer = value;
                        break;

                    case "Путь":
                        if (firstElementFlag) E1_Path = value;
                        else E2_Path = value;
                        break;

                    case "Элемент Имя":
                        if (firstElementFlag)
                        {
                            E1_Name = value;
                            firstElementFlag = false;
                        }
                        else E2_Name = value;
                        break;

                    default:
                        break;
                }

            }
        }

        /// <summary>
        /// Выгружает данные из строчки табличного html отчета о коллизиях
        /// </summary>
        /// <param name="node">Нод div class=viewpoint из отчета</param>
        private void TableRowParse(HtmlNode node)
        {
            HtmlNodeCollection cells = node.SelectNodes("./td");
            for (int i = 0; i < cells.Count; i++)
            {
                HtmlNode cell = cells[i];
                string value = cell.InnerText;
                switch (Table.Header.Cells[i].Item2)
                {
                    case "Изображение":
                        var a = cell.SelectSingleNode("./a");
                        string href = a.GetAttributeValue("href", "");
                        ImagePath = new Uri(System.IO.Path.Combine(Table.FolderPath, href)).LocalPath;
                        break;

                    case "Наименование конфликта":
                        Name = value;
                        break;

                    case "Статус":
                        Status = value;
                        break;

                    case "Расстояние":
                        Distance = double.Parse(Regex.Match(value, @"-?\d+\.\d+").Value.Replace('.', ','));
                        break;

                    case "Расположение сетки":
                        Grid = value;
                        break;

                    case "Описание:":
                        Description = value;
                        break;

                    //case "Дата обнаружения":
                    //    value = value.Replace(" ", string.Empty).Replace("\r\n", string.Empty);
                    //    FoundDate = DateTime.ParseExact(value, "yyyy/M/d HH:mm", CultureInfo.InvariantCulture);
                    //    break;

                    case "Точка конфликта":
                        MatchCollection matches = Regex.Matches(value, @"\d+\.\d+");
                        double[] coords = matches.Cast<Match>().Select(x => double.Parse(x.Value.Replace('.', ','))).ToArray();
                        double cx = UnitUtils.ConvertToInternalUnits(coords[0], DisplayUnitType.DUT_METERS);
                        double cy = UnitUtils.ConvertToInternalUnits(coords[1], DisplayUnitType.DUT_METERS);
                        double cz = UnitUtils.ConvertToInternalUnits(coords[2], DisplayUnitType.DUT_METERS);
                        Point = new XYZ(cx, cy, cz);
                        break;

                    case "Идентификатор элемента":
                        int idint = int.Parse(Regex.Match(cell.InnerText, @"\d+").Value);
                        ElementId id = new ElementId(idint);
                        if (Table.Header.Cells[i].Item1 == "item1Header") E1_Id = id;                        
                        else E2_Id = id;                        
                        break;

                    case "Слой":
                        if (Table.Header.Cells[i].Item1 == "item1Header") E1_Layer = value;                        
                        else E2_Layer = value;                        
                        break;

                    case "Путь":
                        if (Table.Header.Cells[i].Item1 == "item1Header") E1_Path = value;                        
                        else E2_Path = value;                        
                        break;

                    case "Элемент Имя":
                        if (Table.Header.Cells[i].Item1 == "item1Header") E1_Name = value;                        
                        else E2_Name = value;                        
                        break;

                    default:
                        break;
                }
            }
        }
    }

    class CollisionUtilities
    {
        public static List<IntersectionMepCurve> HTMLReportParse(Document hostdoc, string path)
        {            
            List<IntersectionMepCurve> result = new List<IntersectionMepCurve>();
            RevitLinkInstance linkdocInstance = GeometryUtils.ChooseLinkedDoc(hostdoc);
            if (linkdocInstance == null)
            {
                return result;
            }
            else
            {               
                CollisionReportTable table = new CollisionReportTable(path);
                Document linkdoc = linkdocInstance.GetLinkDocument();
                ElementId hostid, linkid;
                string hostdocTitle = Regex.Match(hostdoc.Title, @"(.+)_").Groups[1].Value; // отделяем имя пользователя от имени файла в локальной копии
                foreach (CollisionReportRow row in table.Rows)
                {
                    int x1 = hostdocTitle == row.E1_DocumentName ? 1 : 0;
                    int x2 = hostdocTitle == row.E2_DocumentName ? 2 : 0;
                    int x3 = linkdoc.Title == row.E1_DocumentName ? 4 : 0;
                    int x4 = linkdoc.Title == row.E2_DocumentName ? 8 : 0;

                    if (linkdoc.Title == row.E1_DocumentName)
                    {
                        hostid = row.E2_Id;
                        linkid = row.E1_Id;
                    }
                    else if (linkdoc.Title == row.E2_DocumentName)
                    {
                        hostid = row.E1_Id;
                        linkid = row.E2_Id;
                    }
                    else
                    {
                        Autodesk.Revit.UI.TaskDialog.Show("Ошибка", "Не удалось определить файлы из отчета");
                        return result;
                    }

                    // точка из отчета дана относительно площадки - корректируем на проект
                    ProjectLocation pl = hostdoc.ActiveProjectLocation;
                    XYZ corrPt = pl.GetTransform().OfPoint(row.Point);
                    result.Add(new IntersectionMepCurve(hostdoc.GetElement(hostid), linkdoc.GetElement(linkid), corrPt, linkdocInstance));
                }
                return result;
            }
        }     
    }
}
