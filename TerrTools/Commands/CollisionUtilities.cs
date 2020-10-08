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
using TerrTools.UI;
using Autodesk.Revit.UI;
using TerrTools.UI;

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
                return Regex.Match(res, @"[a-zA-Zа-яА-Я0-9_][a-zA-Zа-яА-Я0-9_ ]+(\.ifc)?").Value;
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
                        //value = value.Replace("&gt;", " > ");
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
                value = Regex.Replace(value, @"\r\n\s*", "");
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
                        //value = value.Replace("&gt;", " > ");
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
        public static List<IntersectionMepCurve> HTMLToMEPIntersections(Document hostdoc, string path)
        {            
            List<IntersectionMepCurve> result = new List<IntersectionMepCurve>();
            CollisionReportTable table = new CollisionReportTable(path);

            // логгирование
            LoggingMachine.Reset();
            var log_lostHost = LoggingMachine.NewLog("Чтение строк HTML", 
                table.Rows.Select(x => x.E1_Id), "Элемент отсутствует в текущем документе");
            var log_lostLink = LoggingMachine.NewLog("Чтение строк HTML", 
                table.Rows.Select(x => x.E2_Id), "Элемент отсутствует в связанном документе");

            var dialog = new UI.TwoDoc(hostdoc, table);
            if (System.Windows.Forms.DialogResult.OK == dialog.ShowDialog())
            {
                foreach (CollisionReportRow row in table.Rows)
                {
                    /* спарсенный отчет мы превращаем в массив из объектов кастомного класса 
                     * IntersectionMepCurve (см. реализацию в файле Openings.cs)
                     * Ниже прописана логика забора элемента из текущего и связанного документа
                     * на основе выбора в диалоге. На данный момент сейчас работает ТОЛЬКО
                     * один вариант - один в текущем, другой в связанном. Для остальных случаев тут 
                     * надо править логику
                     */
                    ElementId hostid = ElementId.InvalidElementId;
                    ElementId pipeid = ElementId.InvalidElementId;
                    RevitLinkInstance linkdocInstance = null;
                    Element host = null;
                    Element pipe = null;
                    switch (dialog.LinkElementNumber)
                    {
                        case 0:
                            //ни один не линк
                            break;
                        case 1:
                            //линк первый элемент
                            linkdocInstance = dialog.LinkInstance1;
                            pipeid = row.E1_Id;
                            pipe = linkdocInstance.GetLinkDocument().GetElement(pipeid);
                            break;
                        case 2:
                            //линк второй элемент
                            linkdocInstance = dialog.LinkInstance2;
                            pipeid = row.E2_Id;
                            pipe = linkdocInstance.GetLinkDocument().GetElement(pipeid);
                            break;
                        case 3:
                            // оба линки
                            linkdocInstance = dialog.LinkInstance1;
                            Element e1 = linkdocInstance.GetLinkDocument().GetElement(row.E1_Id);
                            linkdocInstance = dialog.LinkInstance2;
                            Element e2 = linkdocInstance.GetLinkDocument().GetElement(row.E2_Id);
                            if (e1 is HostObject)
                            {
                                host = e1;
                                hostid = row.E1_Id;
                                pipe = e2;
                                pipeid = row.E2_Id;
                            }
                            else
                            {
                                host = e2;
                                hostid = row.E2_Id;
                                pipe = e1;
                                pipeid = row.E1_Id;
                            }
                            break;
                        default:
                            break;
                    }

                    switch (dialog.HostElementNumber)
                    {
                        case 0:
                            //ни один не в текущем документе
                            break;
                        case 1:
                            //линк первый элемент
                            hostid = row.E1_Id;
                            host = hostdoc.GetElement(hostid);
                            break;
                        case 2:
                            //линк второй элемент
                            hostid = row.E2_Id;
                            host = hostdoc.GetElement(hostid);
                            break;
                        case 3:
                            //оба в текущем документе
                            Element e1 = hostdoc.GetElement(row.E1_Id);
                            Element e2 = hostdoc.GetElement(row.E2_Id);
                            if (e1 is HostObject)
                            {
                                host = e1;
                                hostid = row.E1_Id;
                                pipe = e2;
                                pipeid = row.E2_Id;
                            }
                            else
                            {
                                host = e2;
                                hostid = row.E2_Id;
                                pipe = e1;
                                pipeid = row.E1_Id;
                            }
                            break;
                        default:
                            break;
                    }

                    // точка из отчета дана относительно площадки - корректируем на проект
                    ProjectLocation pl = hostdoc.ActiveProjectLocation;
                    XYZ corrPt = pl.GetTransform().OfPoint(row.Point);
                    if (host == null)
                    {
                        log_lostHost.AddError(hostid);
                    }
                    else if (pipe == null)
                    {
                        log_lostHost.AddError(pipeid);
                    }
                    else
                    {
                        var intr = new IntersectionMepCurve(host, pipe, corrPt, linkdocInstance);
                        result.Add(intr);
                    }
                }
            }
            LoggingMachine.Show();
            return result;
        }   
    }
}
