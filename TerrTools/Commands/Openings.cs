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
using System.IO;
using System.Linq.Expressions;
using Autodesk.Revit.DB.Electrical;

namespace TerrTools
{
    abstract public class BaseIntersectionHandler
    {
        abstract internal string openingFamilyName { get; }
        abstract internal IEnumerable<HostObject> hosts { get; }
        public Document doc;
        protected RevitLinkInstance linkedDocInstance;
        protected FamilySymbol openingFamilySymbol;
        public Document linkedDoc { get { return linkedDocInstance.GetLinkDocument(); } }

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
                try
                {
                    doc.LoadFamilySymbol(Path.Combine(TerrSettings.OpeningsFolder, openingFamilyName + ".rfa"), "Проем", out symbol);
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

        protected bool PlaceOpeningFamilies(List<IntersectionMepCurve> intersections)
        {
            try
            {
                List<IntersectionMepCurve> failedIntersections = new List<IntersectionMepCurve>();
                foreach (IntersectionMepCurve i in intersections)
                {

                    ElementId wallDesignOption = i.Host.DesignOption != null ? i.Host.DesignOption.Id : ElementId.InvalidElementId;
                    if (wallDesignOption == DesignOption.GetActiveDesignOptionId(doc))
                    {
                        Element holeElement = doc.Create.NewFamilyInstance(
                            new XYZ(
                                i.InsertionPoint.X,
                                i.InsertionPoint.Y,
                                i.Level.Elevation),
                            openingFamilySymbol,
                            i.Host,
                            i.Level,
                            StructuralType.NonStructural);

                        // поворачиваем на нужную позицию
                        Line axe = Line.CreateUnbound(i.InsertionPoint, XYZ.BasisZ);
                        ElementTransformUtils.RotateElement(doc, holeElement.Id, axe, i.Angle);

                        // задаем параметры
                        holeElement.LookupParameter("ADSK_Отверстие_Ширина").Set(i.HoleWidth);
                        holeElement.LookupParameter("ADSK_Отверстие_Высота").Set(i.HoleHeight);
                        holeElement.LookupParameter("ADSK_Толщина стены").Set(i.HoleDepth);
                        holeElement.LookupParameter("ADSK_Отверстие_Функция").Set(i.Type);

                        // назначаем отметки
                        holeElement.LookupParameter("ADSK_Отверстие_Отметка от этажа").Set(i.LevelOffset);
                        if (i.Level.Elevation == 0) holeElement.LookupParameter("ADSK_Отверстие_Отметка этажа").Set(0);
                        else holeElement.LookupParameter("ADSK_Отверстие_Отметка этажа").Set(i.Level.Elevation);

                        //идентификация
                        holeElement.LookupParameter("Идентификатор пересечения").Set(i.Id.ToString());
                        string linkName = doc.GetElement(i.LinkInstance.GetTypeId()).Name;
                        holeElement.LookupParameter("Связанный файл")?.Set(linkName);

                        //делаем вырез в хосте
                        InstanceVoidCutUtils.AddInstanceVoidCut(doc, i.Host, holeElement);
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



        public List<IntersectionMepCurve> GetIntersections()
        {
            linkedDocInstance = GeometryUtils.ChooseLinkedDoc(doc);
            if (linkedDocInstance==null)
            {
                return new List<IntersectionMepCurve>();
            }
            else
            {
                var progress = new UI.ProgressBar("Поиск пересечений...", hosts.Count());
                Transform tr = GeometryUtils.GetCorrectionTransform(linkedDocInstance);
                List<IntersectionMepCurve> intersectionList = new List<IntersectionMepCurve>();
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
                            Curve mepCurve = GeometryUtils.FindDuctCurve(m.ConnectorManager);
                            if (mepCurve != null)
                            {
                                XYZ[] interPts = (from wallFace in hostFaces select FindFaceIntersection(mepCurve, wallFace)).ToArray();
                                if (interPts.Any(x => x != null))
                                {
                                    XYZ pt = interPts.First(x => x != null);
                                    IntersectionMepCurve i = new IntersectionMepCurve(host, m, pt, linkedDocInstance);                                   
                                    intersectionList.Add(i);
                                }
                            }
                        }
                    }
                    progress.StepUp();
                }
                return intersectionList;
            }
        }


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            doc = commandData.Application.ActiveUIDocument.Document;

            using (Transaction tr = new Transaction(doc, "Создание отверстий"))
            {
                tr.Start();              
                openingFamilySymbol = FindOpeningFamily();
                if (openingFamilySymbol == null)
                    return Result.Failed;
                //DeleteUnusedOpenings();
                tr.Commit();

                UI.IntersectionsForm form = new UI.IntersectionsForm(this);
                if (form.DialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    tr.Start();
                    List<IntersectionMepCurve>intersections = form.Intersections;
                    bool result = PlaceOpeningFamilies(intersections);
                    tr.Commit();
                    if (result)
                    {
                        //TaskDialog.Show("Успешно", "Отверстия расставлены, но не забудьте их проверить");
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


    public class IntersectionMepCurve
    {
        public RevitLinkInstance LinkInstance { get; set; }
        public Element Host { get; }
        public Element Pipe { get; }
        private ConnectorManager PipeConnectorManager
        {
            get
            {
                if (Pipe is MEPCurve || Pipe is CableTray) return (Pipe as MEPCurve).ConnectorManager;
                else if (Pipe is FamilyInstance) return (Pipe as FamilyInstance).MEPModel.ConnectorManager;
                else return null;
            }
        }
        public string Id { get { return string.Format("{0}-{1}", Host.Id.ToString(), Pipe.Id.ToString()); } }
        public bool IsBrick 
        { 
            get 
            {
                return false;
                bool state = false;
                if (Host != null && Host is Wall)
                {                    
                    foreach (var layer in (Host as Wall).WallType.GetCompoundStructure().GetLayers())
                    {
                        Element materialElement = Host.Document.GetElement(layer.MaterialId);
                        state = materialElement != null ? materialElement.Name.ToLower().Contains("кирпич") || state : false;
                    }
                }
                return state;
            }
        }       
        /// <summary>
        /// Изначальная точка пересечения, взятая из отчета или анализа
        /// </summary>
        public XYZ CollisionPoint { get; set; }

        /// <summary>
        /// Точка пересечения, спроецированная на линию MEP и центральную линию стены
        /// </summary>
        public XYZ InsertionPoint { get; private set; } 
        

        public Level Level { get { return Host.Document.GetElement(Host.LevelId) as Level; } }
        public double PipeWidth { get; private set; }
        public double PipeHeight { get; private set; }
        public double Offset { get; set; } = 50.0 / 304.8;
        public Outline Outline { get; private set; }
        public double Angle { get; private set; }

        public string Type {
            get
            {
                return "";
                MEPCurve m = Pipe as MEPCurve;
                switch (m.MEPSystem.Category.Id.IntegerValue)
                {
                    //case (int)BuiltInCategory.OST_PipingSystem:
                    //    i.Type = "ВК";
                    //    break;
                    case (int)BuiltInCategory.OST_DuctSystem:
                        return "ОВ";
                    default:
                        return "";
                }
            }
        }

        // отложим подбор под кирпич до лучших времен
        /*
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
                else return Math.Ceiling(nominalWidth);
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
                else return Math.Ceiling(nominalHeight);
            }
        }

        public double HoleDepth
        {
            get
            {
                if (Host is Wall)
                {
                    Wall h = Host as Wall;
                    return h.Width;
                }
                else
                {
                    return 1;
                }
            }
        }

        private double calc_LevelOffset()
        {
            double nominalLevelOffset = InsertionPoint.Z - Level.Elevation - HoleHeight / 2;
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
        */



        public double LevelOffset { get => InsertionPoint.Z - Level.Elevation - HoleHeight / 2; }

        public double HoleHeight { get => Outline.MaximumPoint.Z - Outline.MinimumPoint.Z; }
        public double HoleWidth { get => Outline.MaximumPoint.X - Outline.MinimumPoint.X; }
        public double HoleDepth { get => Outline.MaximumPoint.Y - Outline.MinimumPoint.Y; }

        public double GroundOffset
        {
            get { return LevelOffset + Level.Elevation; }
        }

        private void SetPipeSize()
        {
            var connectors = GeometryUtils.GetConnectors(PipeConnectorManager);
            var roundCons = connectors.Where(x => x.Shape == ConnectorProfileType.Round);
            var rectangleCons = connectors.Where(x => x.Shape == ConnectorProfileType.Rectangular);

            double maxR = roundCons.Count() > 0 ? roundCons.Max(x => x.Radius) * 2 : 0;
            double maxW = rectangleCons.Count() > 0 ? rectangleCons.Max(x => x.Width) : 0;
            double maxH = rectangleCons.Count() > 0 ? rectangleCons.Max(x => x.Height) : 0;

            PipeWidth = maxR > maxW ? maxR : maxW;
            PipeHeight = maxR > maxH ? maxR : maxH;
        }

        public bool IsIntersectedWithAnother(IntersectionMepCurve other, double tolerance)
        {
            if (this.Id == other.Id) return false;
            return this.Outline.Intersects(other.Outline, tolerance);
        }                

        double calc_Angle()
        {
            Curve c = GeometryUtils.FindDuctCurve(PipeConnectorManager);
            if (c == null)
            {
                return 0;
            }
            XYZ vec = c.GetEndPoint(1) - c.GetEndPoint(0);
            return vec.AngleTo(XYZ.BasisY);
        }

    private XYZ calc_InsertionPoint()
        {
            Curve pipe_curve = GeometryUtils.FindDuctCurve(PipeConnectorManager);
            if (pipe_curve == null) return new XYZ(CollisionPoint.X, CollisionPoint.Y, CollisionPoint.Z);
            Transform tr = GeometryUtils.GetCorrectionTransform(LinkInstance);
            pipe_curve = pipe_curve.CreateTransformed(tr.Inverse);

            Wall w = Host as Wall;
            LocationCurve wloc = w.Location as LocationCurve;
            Curve wall_curve = wloc.Curve;
            XYZ pt = pipe_curve.Project(wall_curve.GetEndPoint(0)).XYZPoint;

            return pt;
        }

        private Outline calc_Outline()
        {
            var connectors = GeometryUtils.GetConnectors(PipeConnectorManager);
            var roundCons = connectors.Where(x => x.Shape == ConnectorProfileType.Round);
            var rectangleCons = connectors.Where(x => x.Shape == ConnectorProfileType.Rectangular);

            double maxR = roundCons.Count() > 0 ? roundCons.Max(x => x.Radius) * 2 : 0;
            double maxW = rectangleCons.Count() > 0 ? rectangleCons.Max(x => x.Width) : 0;
            double maxH = rectangleCons.Count() > 0 ? rectangleCons.Max(x => x.Height) : 0;

            PipeWidth = maxR > maxW ? maxR : maxW;
            PipeHeight = maxR > maxH ? maxR : maxH;
            double depth = (Host as Wall).Width;   

            XYZ minPt = new XYZ(InsertionPoint.X - PipeWidth / 2 - Offset, InsertionPoint.Y - depth / 2, InsertionPoint.Z - PipeHeight / 2 - Offset);
            XYZ maxPt = new XYZ(InsertionPoint.X + PipeWidth / 2 + Offset, InsertionPoint.Y + depth / 2, InsertionPoint.Z + PipeHeight / 2 + Offset);
            var outline = new Outline(minPt, maxPt);
            return outline;
        }

        void Init()
        {
            InsertionPoint = calc_InsertionPoint();
            Outline = calc_Outline();
            Angle = calc_Angle();                  
        }

        public IntersectionMepCurve(Element host, Element pipe, XYZ pt, RevitLinkInstance instance)
        {
            Host = host;
            Pipe = pipe;
            if (Host == null || Pipe == null) throw new ArgumentNullException("host, pipe", "Элемент не может быть null");
            LinkInstance = instance;
            CollisionPoint = pt;
            Init();            
        }
    }


    public class IntersectionSet
    {
        List<IntersectionMepCurve> Intersections { get; }

        static private List<IntersectionSet> MergePairs(List<(IntersectionMepCurve, IntersectionMepCurve)> pairs)
        {
            throw new NotImplementedException();
        }
        
        public static void FindSets(List<IntersectionMepCurve> intersections, double tolerance)
        {
            List<(IntersectionMepCurve, IntersectionMepCurve)> collidePairs = new List<(IntersectionMepCurve, IntersectionMepCurve)>();
            for (int i = 0; i < intersections.Count-1; i++)
            {
                for (int j = i+1; j < intersections.Count; j++)
                {
                    var left = intersections[i];
                    var right = intersections[j];
                    if(left.IsIntersectedWithAnother(right, tolerance))
                    {
                        collidePairs.Add((left, right));
                    }
                }
            }
            var merged = MergePairs(collidePairs);
        }

        public IntersectionSet(List<IntersectionMepCurve> intersections)
        {
            Intersections = intersections;
        }
    }


    [Transaction(TransactionMode.Manual)]
    class WallOpeningHandler : BaseIntersectionHandler, IExternalCommand
    {
        internal override string openingFamilyName { get { return TerrSettings.WallOpeningFamilyName; } }

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
        internal override string openingFamilyName { get { return TerrSettings.FloorOpeningFamilyName; } }

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
