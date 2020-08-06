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
using System.Security.Cryptography;
using System.Security.Policy;

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

        public FamilyInstance[] ExistingOpenings { get; private set; }

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

        protected bool IsExisted(Intersection i)
        {
            return ExistingOpenings.Where(x => x.LookupParameter("Идентификатор пересечения").AsString() == i.Id).Count() > 0;
        }

        protected void PlaceOpeningFamilies(IEnumerable<Intersection> intersections)
        {
            var log_general = LoggingMachine.NewLog("Добавление отверстий", intersections.Select(x => x.Id), "Ошибка параметров");
            var log_cut = LoggingMachine.NewLog("Добавление отверстий", intersections.Select(x => x.Id), "Ошибка вырезания");
            foreach (Intersection i in intersections)
            {
                try
                {
                    Element holeElement;
                    if (IsExisted(i))
                    {
                        holeElement = ExistingOpenings.Where(x => x.LookupParameter("Идентификатор пересечения").AsString() == i.Id).First();
                        LocationPoint loc = holeElement.Location as LocationPoint;
                        XYZ vec = new XYZ(i.InsertionPoint.X,
                                          i.InsertionPoint.Y,
                                          i.Level.Elevation) - loc.Point;
                        ElementTransformUtils.MoveElement(doc, holeElement.Id, vec);
                    }
                    else
                    {
                        holeElement = doc.Create.NewFamilyInstance(
                            new XYZ(
                                i.InsertionPoint.X,
                                i.InsertionPoint.Y,
                                i.Level.Elevation),
                            openingFamilySymbol,
                            i.Level,
                            StructuralType.NonStructural);
                        // поворачиваем на нужную позицию
                        Line axe = Line.CreateUnbound(i.InsertionPoint, XYZ.BasisZ);
                        ElementTransformUtils.RotateElement(doc, holeElement.Id, axe, i.Angle);

                        //делаем вырез в хосте
                        try
                        {
                            List<Element> hosts = new List<Element>();
                            if (i is IntersectionSet) hosts = (i as IntersectionSet).Intersections.Cast<IntersectionMepCurve>().Select(x => x.Host).ToList();
                            else hosts.Add((i as IntersectionMepCurve).Host);
                            foreach (var host in hosts) InstanceVoidCutUtils.AddInstanceVoidCut(doc, host, holeElement);
                        }
                        catch
                        {
                            log_cut.AddError(i.Id);
                        }                        
                    }

                    // задаем параметры
                    holeElement.LookupParameter("ADSK_Отверстие_Ширина").Set(i.HoleWidth);
                    holeElement.LookupParameter("ADSK_Отверстие_Высота").Set(i.HoleHeight);
                    holeElement.LookupParameter("ADSK_Толщина стены").Set(i.HoleDepth);

                    // назначаем отметки
                    holeElement.LookupParameter("ADSK_Отверстие_Отметка от этажа").Set(i.LevelOffset);
                    if (i.Level.Elevation == 0) holeElement.LookupParameter("ADSK_Отверстие_Отметка этажа").Set(0);
                    else holeElement.LookupParameter("ADSK_Отверстие_Отметка этажа").Set(i.Level.Elevation);
                    // обнуляем смещение
                    holeElement.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM).Set(0);

                    //идентификация
                    holeElement.LookupParameter("Идентификатор пересечения")?.Set(i.Id.ToString());
                    holeElement.LookupParameter("Связанный файл")?.Set(i.Name);                    
                }
                catch { log_general.AddError(i.Id); }
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


        public void Init(ExternalCommandData commandData)
        {
            doc = commandData.Application.ActiveUIDocument.Document;

            using (Transaction tr = new Transaction(doc, "Загрузка семейства"))
            {
                tr.Start();
                openingFamilySymbol = FindOpeningFamily();
                if (openingFamilySymbol == null)
                    throw new ArgumentException("Ошибка загрузки семейства");
                tr.Commit();
            }

            ExistingOpenings = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(x => x.Symbol.Id == openingFamilySymbol.Id)
                .ToArray();           
        }


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Init(commandData);
            using (Transaction tr = new Transaction(doc, "Создание отверстий"))
            {                
                UI.IntersectionsForm form = new UI.IntersectionsForm(this);
                if (form.DialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    tr.Start();
                    IEnumerable<Intersection>intersections = form.Intersections.Cast<Intersection>().ToList();
                    if (form.DoMerge) intersections = IntersectionSet.Merge(intersections, form.MergeTolerance).Cast<Intersection>();
                    PlaceOpeningFamilies(intersections);                    
                    tr.Commit();
                    LoggingMachine.Show();
                    return Result.Succeeded;
                }
                else return Result.Cancelled;
            }
        }
    }

    public abstract class Intersection
    {
        abstract public string Name { get; }
        abstract public Level Level { get; }
        abstract public Outline Outline { get; protected set; }
        abstract public double Angle { get; protected set; }
        abstract public XYZ InsertionPoint { get; protected set; }
        abstract public string Id { get; }
        public double HoleHeight { get => Outline.MaximumPoint.Z - Outline.MinimumPoint.Z; }
        public double HoleWidth { get => Outline.MaximumPoint.X - Outline.MinimumPoint.X; }
        public double HoleDepth { get => Outline.MaximumPoint.Y - Outline.MinimumPoint.Y; }
        public double LevelOffset { get => InsertionPoint.Z - Level.Elevation - HoleHeight / 2; }
        public double GroundOffset { get => LevelOffset + Level.Elevation; }

        abstract public void Init();

        /// <summary>
        /// Проверка на пересечение с другим отверстием
        /// </summary>
        /// <param name="other">Другое отверстие</param>
        /// <param name="tolerance">При положительных значениях проверяет пересечение в зазоре</param>
        /// <returns></returns>
        public bool IsIntersectedWithAnother(Intersection other, double tolerance)
        {
            if (this.Id == other.Id) return false;
            return this.Outline.Intersects(other.Outline, tolerance);
        }
    }

    public class SimpleIntersection : Intersection
    {
        public override string Name { get; }
        public override Level Level { get; }
        public override Outline Outline { get ; protected set ; }
        public override double Angle { get ; protected set ; }
        public override XYZ InsertionPoint { get ; protected set; }
        public override string Id { get; }
        public Element Host { get; }

        public override void Init()
        {
            throw new NotImplementedException();
        }
        public SimpleIntersection(XYZ pt, Outline outline, double angle, Element host, string name, string id)
        {
            Outline = outline;
            InsertionPoint = pt;
            Angle = angle;
            Host = host;
            Level = host.Document.GetElement(Host.LevelId) as Level;
            Name = name;
            Id = id;
        }
    }


    public class IntersectionMepCurve : Intersection
    {
        public override string Id { get => string.Format("{0}-{1}", Host.Id.ToString(), Pipe.Id.ToString()); }
        public override string Name { get => LinkInstance.Name; }
        public override Level Level { get { return Host.Document.GetElement(Host.LevelId) as Level; } }
        public override double Angle { get; protected set; } 
        public override XYZ InsertionPoint { get; protected set; } 
        public override Outline Outline { get; protected set; } 
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
        public double PipeWidth { get => HoleWidth - Offset * 2; }
        public double PipeHeight { get => HoleHeight - Offset * 2; }
        public double Offset { get; set; } = 50.0 / 304.8;

        public string HoleType {
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

            double width = maxR > maxW ? maxR : maxW;
            double height = maxR > maxH ? maxR : maxH;
            double depth = (Host as Wall).Width;   

            XYZ minPt = new XYZ(InsertionPoint.X - width / 2 - Offset, InsertionPoint.Y - depth / 2, InsertionPoint.Z - height / 2 - Offset);
            XYZ maxPt = new XYZ(InsertionPoint.X + width / 2 + Offset, InsertionPoint.Y + depth / 2, InsertionPoint.Z + height / 2 + Offset);
            var outline = new Outline(minPt, maxPt);
            return outline;
        }

        public override void Init()
        {
            InsertionPoint = calc_InsertionPoint();
            Angle = calc_Angle();
            Outline = calc_Outline();
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


    public class IntersectionSet :Intersection
    {
        public HashSet<Intersection> Intersections { get; }

        public override string Id { get; }
        public override string Name { get; }
        public override Level Level { get; }
        public override Outline Outline { get; protected set; }
        public override double Angle { get; protected set; }
        public override XYZ InsertionPoint { get; protected set; }

        public static IEnumerable<IntersectionSet> Merge(IEnumerable<Intersection> intrs, double tolerance)
        {
            var groups = FindSets(intrs, tolerance);

            bool flag = true;
            int i = 0;
            while (flag)
            {
                bool changedFlag = false;
                for (int j = groups.Count() - 1; j > i; j--)
                {
                    var left = new HashSet<Intersection>(groups.ElementAt(i));
                    var right = new HashSet<Intersection>(groups.ElementAt(j));
                    left.IntersectWith(right);
                    if (left.Count() > 0)
                    {
                        groups.ElementAt(i).UnionWith(right);
                        groups.RemoveAt(j);
                        changedFlag = true;
                    }
                }
                flag = i < groups.Count() - 1;
                if (!changedFlag) i++;
            }
            return groups.Select(x => new IntersectionSet(x));
        }
        
        /// <summary>
        /// Разбивает массив с пересечениями на пересекающиеся пары (или одиночные непересекаюзиеся)
        /// </summary>
        /// <param name="intersections"></param>
        /// <param name="tolerance"></param>
        /// <returns>List of HashSet<Intersection></Intersection></returns>
        public static List<HashSet<Intersection>> FindSets(IEnumerable<Intersection> intersections, double tolerance)
        {
            List<HashSet<Intersection>> groups = new List<HashSet<Intersection>>();
            for (int i = 0; i < intersections.Count()-1; i++)
            {
                bool collided = false;                
                for (int j = i+1; j < intersections.Count(); j++)
                {
                    var left = intersections.ElementAt(i);
                    var right = intersections.ElementAt(j);
                    if(left.IsIntersectedWithAnother(right, tolerance))
                    {
                        HashSet<Intersection> x = new HashSet<Intersection>() { left, right };
                        collided = true;
                        groups.Add(x);
                    }
                }
                if (!collided)
                {
                    groups.Add(new HashSet<Intersection> { intersections.ElementAt(i) });
                }
            }
            return groups;
        }

        public override void Init()
        {
            Outline outline = Intersections.First().Outline;
            for (int i = 1; i < Intersections.Count(); i++)
            {
                Outline other = Intersections.ElementAt(i).Outline;
                outline.AddPoint(other.MinimumPoint);
                outline.AddPoint(other.MaximumPoint);
            }
            Outline = outline;

            InsertionPoint = new XYZ(
                outline.MinimumPoint.X + (outline.MaximumPoint.X - outline.MinimumPoint.X) / 2,
                outline.MinimumPoint.Y + (outline.MaximumPoint.Y - outline.MinimumPoint.Y) / 2,
                outline.MinimumPoint.Z + (outline.MaximumPoint.Z - outline.MinimumPoint.Z) / 2
                );

            Angle = Intersections.Select(x => x.Angle).First();
        }       

        public IntersectionSet(HashSet<Intersection> intersections)
        {
            Intersections = intersections;
            Id = string.Join("|", intersections.Select(x => x.Id));
            Name = string.Join("|", intersections.Select(x=>x.Name));
            double mL = intersections.Min(x => x.Level.Elevation);
            Level = intersections.Where(x => x.Level.Elevation == mL).First().Level;
            Init();
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
