using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Structure;
using System.Text.RegularExpressions;

namespace TerrTools
{
    abstract public class BaseIntersectionHandler
    {
        abstract internal string openingFamilyName { get; }
        abstract internal IEnumerable<HostObject> hosts { get; }
        protected Document doc;
        protected RevitLinkInstance linkedDocInstance;
        protected FamilySymbol openingFamilySymbol;
        protected Document linkedDoc { get { return linkedDocInstance.GetLinkDocument(); } }

        protected FamilySymbol FindOpeningFamily()
        {
            FamilySymbol symbol;
            Family openingFamily = new FilteredElementCollector(doc)
                .OfClass(typeof(Family))
                .Where(x => x.Name == openingFamilyName)
                .Cast<Family>()
                .FirstOrDefault();
            if (openingFamily != null)
            {
                symbol = (FamilySymbol)doc.GetElement(openingFamily.GetFamilySymbolIds().First());
                return symbol;
            }
            else
            {
                string path = @"\\serverl\psd\REVIT\Семейства\ТеррНИИ\АР\" + openingFamilyName + ".rfa";
                try
                {
                    doc.LoadFamilySymbol(path, "Проем", out symbol);
                    symbol.Activate();
                    return symbol;
                }
                catch (Autodesk.Revit.Exceptions.ArgumentException)
                {
                    TaskDialog.Show("Ошибка", "Не удалось найти семейство, требуемое для работы плагина. Обратитесь к BIM-менеджеру");
                    return null;
                }
            }
        }

        protected void DeleteUnusedOpenings()
        {
            Element[] existingOpenings = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(x => x.Symbol.Id == openingFamilySymbol.Id)
                .ToArray();

            
            // Весь этот код требует тщательной перепроверки, потому что он выдает неадекватные результаты
            // Поэтмому пока что просто удалим все что насоздавали
            
            foreach (Element op in existingOpenings)
            {
                Parameter p = op.LookupParameter("Идентификатор пересечения");
                ElementId opDesignOption = op.DesignOption != null ? op.DesignOption.Id : ElementId.InvalidElementId;
                if (p != null && p.AsString().Contains('-') )
                //{
                //    string opId = p.AsString();
                //    ElementId hostId = new ElementId(int.Parse(opId.Split('-')[0]));
                //    ElementId curveId = new ElementId(int.Parse(opId.Split('-')[1]));
                //    MEPCurve mc = linkedDoc.GetElement(curveId) as MEPCurve;
                //    Curve c = GeometryUtils.FindDuctCurve(mc);
                //    Element host = doc.GetElement(hostId);
                //    var faces = GeometryUtils.GetFaces(host);
                //    if (c != null) 
                //    { 
                //    XYZ[] interPts = (from face in faces select FindFaceIntersection(c, face)).ToArray();
                //        if (interPts.Any(x => x != null))
                        {
                            doc.Delete(op.Id);
                        }
                //    }
                //}
            }         
        }

        protected bool PlaceOpeningFamilies(List<Intersection> intersections)
        {
            try
            {
                List<Intersection> failedIntersections = new List<Intersection>();
                foreach (Intersection i in intersections)
                {
                    ElementId wallDesignOption = i.Host.DesignOption != null ? i.Host.DesignOption.Id : ElementId.InvalidElementId;
                    if (wallDesignOption == DesignOption.GetActiveDesignOptionId(doc))
                    {
                        Element holeElement = doc.Create.NewFamilyInstance(new XYZ(i.CenterPoint.X, i.CenterPoint.Y, i.Level.Elevation), openingFamilySymbol, i.Host, i.Level, StructuralType.NonStructural);
                        holeElement.LookupParameter("ADSK_Отверстие_Ширина").Set(i.HoleWidth);
                        holeElement.LookupParameter("ADSK_Отверстие_Высота").Set(i.HoleHeight);
                        holeElement.LookupParameter("ADSK_Отверстие_Функция").Set(i.Type);
                        holeElement.LookupParameter("ADSK_Отверстие_Отметка от этажа").Set(i.LevelOffset);
                        if (i.Level.Elevation == 0) holeElement.LookupParameter("ADSK_Отверстие_Отметка этажа").Set(0);
                        else holeElement.LookupParameter("ADSK_Отверстие_Отметка этажа").Set(i.Level.Elevation);
                        holeElement.LookupParameter("Идентификатор пересечения").Set(i.Id.ToString());
                    }
                    else
                    {
                        failedIntersections.Add(i);
                    }
                }                
                return true;
            }
            catch (NullReferenceException)
            {                
                return false;
            }
        }

        protected XYZ FindFaceIntersection(Curve DuctCurve, Face WallFace)
        {
            // Коррекция позиции связанного файла
            // Так как перемещаем не ПЛОСКОСТЬ, а ЛУЧИ, то инвертируем перемещение
            Transform tf = GeometryUtils.GetCorrectionTransform(linkedDocInstance);
            tf = tf.Inverse;
            Curve correctedCurve = DuctCurve.CreateTransformed(tf);
            //The intersection point
            //Intersection point set
            SetComparisonResult results;
            results = WallFace.Intersect(correctedCurve, out IntersectionResultArray intersectionR);
            if (SetComparisonResult.Disjoint != results
                && intersectionR != null
                && !intersectionR.IsEmpty)
                return intersectionR.get_Item(0).XYZPoint;
            else
                return null;
        }

        internal List<Intersection> GetIntersections(IEnumerable<HostObject> hosts)
        {
            var progress = new UI.ProgressBar("Поиск пересечений...", hosts.Count());
            Transform tr = GeometryUtils.GetCorrectionTransform(linkedDocInstance);
            List<Intersection> intersectionList = new List<Intersection>();
            foreach (HostObject host in hosts)
            {
                List<Face> hostFaces = GeometryUtils.GetFaces(host);
                if (hostFaces.Count > 0)
                {
                    BoundingBoxXYZ bb = host.get_BoundingBox(null);
                    Outline outline = new Outline(tr.OfPoint(bb.Min), tr.OfPoint(bb.Max));
                    BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(outline);

                    List<MEPCurve> meps = new FilteredElementCollector(linkedDoc)
                        .OfClass(typeof(MEPCurve))
                        .WherePasses(filter)
                        .Cast<MEPCurve>()
                        .ToList();

                    foreach (MEPCurve m in meps)
                    {
                        Curve mepCurve = GeometryUtils.FindDuctCurve(m);
                        if (mepCurve != null)
                        {
                            XYZ[] interPts = (from wallFace in hostFaces select FindFaceIntersection(mepCurve, wallFace)).ToArray();
                            if (interPts.Any(x => x != null))
                            {
                                Intersection i = new Intersection();
                                i.CenterPoint = interPts.First(x => x != null);
                                i.Host = host;
                                i.Pipe = m;
                                i.Level = doc.GetElement(host.LevelId) as Level;
                                i.MinOffset = 50 / 304.8;
                                // Определение типа систем
                                switch (m.MEPSystem.Category.Id.IntegerValue)
                                {
                                    //case (int)BuiltInCategory.OST_PipingSystem:
                                    //    i.Type = "ВК";
                                    //    break;
                                    case (int)BuiltInCategory.OST_DuctSystem:
                                        i.Type = "ОВ";
                                        break;
                                    default:
                                        break;
                                }
                                //Определение размера трубы
                                try
                                {
                                    i.PipeWidth = m.Diameter;
                                    i.PipeHeight = m.Diameter;
                                    i.IsRound = true;
                                }
                                catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                                {
                                    // из-за того, что воздуховод может быть развернут, мы не можем обращаться к внутренним размерам и смотрим на BBox
                                    BoundingBoxXYZ bbmep = m.get_BoundingBox(null);
                                    XYZ res = bbmep.Max - bbmep.Min;
                                    double x = Math.Abs(res.X);
                                    double y = Math.Abs(res.Y);
                                    double z = Math.Abs(res.Z);
                                    i.PipeWidth = host != null ? m.Width : x;
                                    i.PipeHeight = host != null ? m.Height : y;
                                    i.IsRound = false;
                                }
                                //Определение, кирпичная ли стена
                                i.IsBrick = false;                                
                                if (host != null && host is Wall)
                                {
                                    foreach (var layer in (host as Wall).WallType.GetCompoundStructure().GetLayers())
                                    {
                                        Element materialElement = doc.GetElement(layer.MaterialId);
                                        i.IsBrick = materialElement != null ? materialElement.Name.Contains("Кирпич") || i.IsBrick : false;
                                    }
                                }
                                intersectionList.Add(i);
                            }
                        }
                    }
                }
                progress.StepUp();
            }
            return intersectionList;
        }


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            doc = commandData.Application.ActiveUIDocument.Document;

            using (Transaction tr = new Transaction(doc, "Создание отверстий"))
            {
                tr.Start();

                linkedDocInstance = GeometryUtils.GetLinkedDoc(doc);
                if (linkedDocInstance == null)
                    return Result.Cancelled;

                if (linkedDocInstance.GetLinkDocument() == null)
                {
                    TaskDialog.Show("Ошибка", "Обновите элемент связи в проекте");
                    return Result.Failed;
                }

                openingFamilySymbol = FindOpeningFamily();
                if (openingFamilySymbol == null)
                    return Result.Failed;

                DeleteUnusedOpenings();
                tr.Commit();

                tr.Start();
                List<Intersection> intersections = GetIntersections(hosts);
                UI.IntersectionsForm form = new UI.IntersectionsForm(intersections);

                if (form.DialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    intersections = form.UpdatedIntersections;
                    bool result = PlaceOpeningFamilies(intersections);
                    tr.Commit();
                    if (result)
                    {
                        TaskDialog.Show("Успешно", "Отверстия расставлены, но не забудьте их проверить");
                    }
                    else
                    {
                        TaskDialog.Show("Ошибка", "Отсутствуют требуемые параметры в семействе отверстий. Операция отменена");
                    }                    
                    return Result.Succeeded;
                }
                else return Result.Cancelled;
            }
        }
    }

    public class Intersection
    {
        public Element Host { get; set; }
        public Element Pipe { get; set; }
        public string Id { get { return string.Format("{0}-{1}", Host.Id.ToString(), Pipe.Id.ToString()); } }
        public bool IsBrick { get; set; }
        public bool IsRound { get; set; }
        public XYZ CenterPoint { get; set; }
        public Level Level { get; set; }
        public double PipeWidth { get; set; }
        public double PipeHeight { get; set; }
        public double MinOffset { get; set; }
        public string Type { get; set; }

        public double HoleWidth
        {
            get
            {
                double nominalWidth = PipeWidth + MinOffset * 2;
                if (IsBrick)
                {
                    int bricks = 1;
                    double bricksWidth = (bricks * 120 + (bricks - 1) * 10 + 20) / 304.8;
                    while (bricksWidth < nominalWidth)
                    {
                        bricks++;
                        bricksWidth = (bricks * 120 + (bricks - 1) * 10 + 20) / 304.8;
                    }
                    return bricksWidth;
                }
                else return Math.Ceiling(nominalWidth * 304.8 / 100) * 100 / 304.8;
            }
        }
        public double HoleHeight
        {
            get
            {
                double nominalHeight = PipeHeight + MinOffset * 2;
                if (IsBrick)
                {
                    int bricks = 1;
                    double bricksHeight = (bricks * 65 + (bricks - 1) * 10 + 20) / 304.8;
                    while (bricksHeight < nominalHeight)
                    {
                        bricks++;
                        bricksHeight = (bricks * 65 + (bricks - 1) * 10 + 20) / 304.8;
                    }
                    return bricksHeight;
                }
                else return Math.Ceiling(nominalHeight * 304.8 / 100) * 100 / 304.8;
            }
        }
        public XYZ InsertPoint { get { return new XYZ(CenterPoint.X, CenterPoint.Y, LevelOffset); } }
        public double LevelOffset
        {
            get
            {
                double nominalLevelOffset = CenterPoint.Z - Level.Elevation - HoleHeight / 2;
                if (IsBrick)
                {
                    int bricks = 0;
                    double bricksHeight = (bricks * 65 + (bricks - 1) * 10) / 304.8;
                    while (bricksHeight < nominalLevelOffset)
                    {
                        bricks++;
                        bricksHeight = (bricks * 65 + (bricks - 1) * 10) / 304.8;
                    }
                    return bricksHeight;
                }
                else return Math.Round(nominalLevelOffset * 304.8 / 50) * 50 / 304.8;
            }
        }
        public double GroundOffset
        {
            get { return LevelOffset + Level.Elevation; }
        }
    }

    [Transaction(TransactionMode.Manual)]
    class WallOpeningHandler : BaseIntersectionHandler, IExternalCommand
    {
        internal override string openingFamilyName { get { return "ТеррНИИ_ОтверстиеПрямоугольное_Стена"; } }

        internal override IEnumerable<HostObject> hosts
        {
            get
            {
                return new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Walls)
                    .WhereElementIsNotElementType()
                    .Cast<HostObject>();
            }
        }
    }


    [Transaction(TransactionMode.Manual)]
    class FloorOpeningHandler : BaseIntersectionHandler, IExternalCommand
    {
        internal override string openingFamilyName { get { return "ТеррНИИ_ОтверстиеПрямоугольное_Перекрытие"; } }

        internal override IEnumerable<HostObject> hosts
        {
            get
            {
                return new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_Floors)
                    .WhereElementIsNotElementType()
                    .Cast<HostObject>();
            }
        }
    }
}
