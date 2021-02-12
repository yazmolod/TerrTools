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

namespace TerrTools
{
    public class CollisionReportTable
    {
        public string Path { get; }
        public string FolderPath { get => System.IO.Path.GetDirectoryName(Path); }
        public CollisionReportHeader Header { get; }
        public List<CollisionReportRow> Rows { get; }
        public CollisionReportRow this[int i] { get => Rows[i]; }
        public string Document1Name { get; }
        public string Document2Name { get; }
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
                    var childNodes = report.DocumentNode.SelectNodes("//table[@class='mainTable']/tr[@class='childRowLast']");
                    if (childNodes != null) {
                        nodes.AddRange(childNodes); }
                }          
            }
            else
            {
                nodes = report.DocumentNode.SelectNodes("//div[@class='viewpoint']");
            }
            List<CollisionReportRow> rows = new List<CollisionReportRow>();
            if (nodes != null)
            {
                foreach (var node in nodes) rows.Add(new CollisionReportRow(this, node));                
            }
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
                res = Regex.Match(res, @"([a-zA-Zа-яА-Я0-9_ \.]+)\..{3}").Groups[1].Value;
                return res;
                    
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

    class CollisionUtils
    {
        /// <summary>
        /// Поиск элементов из строки отчета в текущем документе и связанных файлах
        /// </summary>
        /// <param name="elementsInDocument">Коллекция, в которую складываются элементы из текущего документа</param>
        /// /// <param name="elementsInLink">Коллекция, в которую складываются элементы из связанного</param>
        /// /// <param name="linkInstance">Если найдены элементы в связи, возвращает объект связи; в противном случае null</param>
        static public void GetElementsFromReportRow(Document doc,
                                                          CollisionReportRow row,
                                                          out ICollection<Element> elementsInDocument,
                                                          out ICollection<Element> elementsInLink,
                                                          out RevitLinkInstance linkInstance)
        {
            linkInstance = null;
            DocumentDataSet docSet = new DocumentDataSet(doc, true);
            DocumentData docData1 = docSet[row.E1_DocumentName];
            DocumentData docData2 = docSet[row.E2_DocumentName];
            Document doc1 = docData1.Document;
            Document doc2 = docData2.Document;
            Element e1 = doc1.GetElement(row.E1_Id);
            Element e2 = doc2.GetElement(row.E2_Id);

            elementsInDocument = new List<Element>();
            elementsInLink = new List<Element>();
            if (e1 != null)
            {
                if (doc1.IsLinked)
                {
                    elementsInLink.Add(e1);
                    linkInstance = docData1.Instance;
                }
                else elementsInDocument.Add(e1);
            }
            if (e2 != null)
            {
                if (doc2.IsLinked)
                {
                    elementsInLink.Add(e2);
                    linkInstance = docData2.Instance;
                }
                else elementsInDocument.Add(e2);
            }
        }

        public static IntersectionMepCurve HTMLRowToIntersection(CollisionReportRow row, Document hostdoc, ElementProcessingLog rowLog, ElementProcessingLog angledPipeLog)
        {
            GetElementsFromReportRow(hostdoc, row,
                    out ICollection<Element> docElems, out ICollection<Element> linkElems, out RevitLinkInstance linkdocInstance);
            // в Naviswork система координат по внутренней площадке документа
            // поэтому необходимо и корректировать по местной системе координат
            ProjectLocation pl = hostdoc.ActiveProjectLocation;
            XYZ centerPoint = pl.GetTransform().OfPoint(row.Point);
            // определяем, где конструкция, а где сеть
            var findedElems = docElems.Concat(linkElems);
            Element host = findedElems.FirstOrDefault(x => x is Wall || x is Floor);
            if (host == null)
            {
                // Случай, когда стена разбита на части
                Part part = findedElems.FirstOrDefault(x => x is Part) as Part;
                if (part != null)
                {
                    LinkElementId linkId = part.GetSourceElementIds().First();
                    host = hostdoc.GetElement(linkId.HostElementId);
                }
            }

            Element pipe = findedElems.FirstOrDefault(x => x is MEPCurve ||
                                                (x is FamilyInstance && (x as FamilyInstance).MEPModel.ConnectorManager != null));
            // создание объекта пересечения
            if (host != null && pipe != null)
            {
                try
                {
                    var intr = new IntersectionMepCurve(host, pipe, centerPoint, linkdocInstance);
                    return intr;
                }
                catch (NotImplementedException)
                {
                    angledPipeLog.AddError(row.Name);
                }
            }
            else
            {
                rowLog.AddError(row.Name);
            }
            return null;
        }


        public static List<IntersectionMepCurve> HTMLToMEPIntersections(Document hostdoc, string path)
        {
            var rowLog = LoggingMachine.NewLog("Распознование коллизий", "", "Не найден файл с элементом, не обновлена связь или элементы уже удалены");
            var angledPipeLog = LoggingMachine.NewLog("Распознование коллизий", "", "Труба или воздуховод не лежат в допустимой плоскости");

            CollisionReportTable table = new CollisionReportTable(path);

            List<IntersectionMepCurve> result = new List<IntersectionMepCurve>();
            try
            {
                foreach (var row in table.Rows)
                {

                    var item = HTMLRowToIntersection(row, hostdoc, rowLog, angledPipeLog);
                    result.Add(item);
                }
                result.RemoveAll(x => x == null);
                LoggingMachine.Show();
            }
            catch (DocumentNotFoundException e)
            {
                TaskDialog.Show("Ошибка", e.Message);
            }
            catch (DocumentNotLoadedException e)
            {
                var td = new TaskDialog("Ошибка");
                td.MainInstruction = e.Message;
                td.MainContent = "Обновите связанный документ и повторите попытку";
                td.Show();
            }
            return result;
        }
    }
}
