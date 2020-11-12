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

        protected bool IsExisted(Intersection i)
        {
            string[] ids = ExistingOpenings.Select(x => x.LookupParameter("ТеррНИИ_Идентификатор").AsString()).ToArray();
            return ids.Contains(i.Id.ToString());
        }                

        protected void PlaceOpeningFamilies(IEnumerable<Intersection> intersections)
        {
            var log_param = LoggingMachine.NewLog("Добавление отверстий", intersections.Select(x => x.Id), "Ошибка параметров размеров");
            var log_id = LoggingMachine.NewLog("Добавление отверстий", intersections.Select(x => x.Id), "Ошибка параметров id");
            var log_level = LoggingMachine.NewLog("Добавление отверстий", intersections.Select(x => x.Id), "Ошибка параметров уровня");
            var log_cut = LoggingMachine.NewLog("Добавление отверстий", intersections.Select(x => x.Id), "Ошибка вырезания");
            foreach (Intersection i in intersections)
            {
                    Element holeElement;
                    if (IsExisted(i))
                    {
                        holeElement = ExistingOpenings.Where(x => x.LookupParameter("ТеррНИИ_Идентификатор").AsString() == i.Id.ToString()).First();
                        LocationPoint loc = holeElement.Location as LocationPoint;
                        XYZ vec = new XYZ(i.InsertionPoint.X,
                                          i.InsertionPoint.Y,
                                          i.Level.ProjectElevation) - loc.Point;
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
                        if (i.HasHosts)
                        {
                            foreach (var host in i.Hosts) InstanceVoidCutUtils.AddInstanceVoidCut(doc, host, holeElement);
                        }
                        }
                        catch
                        {
                            log_cut.AddError(i.Id);
                        }                        
                    }
                try
                {
                    //идентификация
                    holeElement.LookupParameter("ТеррНИИ_Идентификатор")?.Set(i.Id.ToString());
                    holeElement.LookupParameter("Связанный файл")?.Set(i.Name);
                }
                catch { log_id.AddError(i.Id); }


                try
                {
                    // задаем параметры
                    holeElement.LookupParameter("ADSK_Отверстие_Ширина").Set(i.HoleWidth);
                    holeElement.LookupParameter("ADSK_Отверстие_Высота").Set(i.HoleHeight);
                    holeElement.LookupParameter("ADSK_Толщина стены").Set(i.HoleDepth);
                    // (временно) работа с отверстиями в кирпиче
                    holeElement.LookupParameter("ТеррНИИ_Отверстие_В кирпиче")?.Set(1);
                }
                catch { log_param.AddError(i.Id); }

                try
                {
                    // назначаем отметки
                    holeElement.LookupParameter("ADSK_Отверстие_Отметка от этажа").Set(i.LevelOffset);
                    if (i.Level.Elevation == 0) holeElement.LookupParameter("ADSK_Отверстие_Отметка этажа").Set(0);
                    else holeElement.LookupParameter("ADSK_Отверстие_Отметка этажа").Set(i.Level.Elevation);
                    // обнуляем смещение
                    holeElement.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM).Set(0);
                }
                catch { log_level.AddError(i.Id); }
            }            
        }

        protected XYZ FindFaceIntersection(Curve DuctCurve, Face WallFace)
        {
            // Коррекция позиции связанного файла
            // Так как перемещаем не ПЛОСКОСТЬ, а ЛУЧИ, то инвертируем перемещение
            Transform tf = DocumentUtils.GetCorrectionTransform(linkedDocInstance);
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
            linkedDocInstance = DocumentUtils.ChooseLinkedDoc(doc);
            if (linkedDocInstance==null)
            {
                return new List<IntersectionMepCurve>();
            }
            else
            {
                var progress = new UI.ProgressBar("Поиск пересечений...", hosts.Count());
                Transform tr = DocumentUtils.GetCorrectionTransform(linkedDocInstance);
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
                                    try
                                    {
                                        IntersectionMepCurve i = new IntersectionMepCurve(host, m, pt, linkedDocInstance);
                                        intersectionList.Add(i);
                                    }
                                    catch (NotImplementedException)
                                    {

                                    }
                                }
                            }
                        }
                    }
                    progress.StepUp();
                }
                progress.Close();
                return intersectionList;
            }
        }


        public void Init(ExternalCommandData commandData)
        {
            doc = commandData.Application.ActiveUIDocument.Document;

            using (Transaction tr = new Transaction(doc, "Загрузка семейства"))
            {
                tr.Start();
                openingFamilySymbol = FamilyInstanceUtils.FindOpeningFamily(doc, openingFamilyName, TerrSettings.OpeningsFolder, "Проем");
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
                    var intersections = form.Intersections.Cast<Intersection>().ToList();
                    if (form.DoMerge)
                    {
                        intersections = Intersection.AutoMerging(intersections, form.MergeTolerance);
                    }
                    PlaceOpeningFamilies(intersections);                    
                    tr.Commit();
                    LoggingMachine.Show();
                    return Result.Succeeded;
                }
                else return Result.Cancelled;
            }
        }
    }

    public class Intersection
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public double Angle { get; protected set; } = 0;             
        public Outline Outline { get; protected set; }

        private List<Element> _hosts = new List<Element>();
        public Element Host { get => _hosts.FirstOrDefault(); set => _hosts = new List<Element>() { value }; }
        public List<Element> Hosts { get => _hosts; set => _hosts = value; }
        public bool HasHosts { get => _hosts.Count() > 0; }
        public bool IsBrick { get; set;} = false;
        public Level Level { get; private set; }
        public XYZ InsertionPoint { get; private set; }
        public double HoleHeight { get => RoundValue(Outline.MaximumPoint.Z - Outline.MinimumPoint.Z, DisplayUnitType.DUT_MILLIMETERS, 1); }
        public double HoleWidth { get => RoundValue(Outline.MaximumPoint.X - Outline.MinimumPoint.X, DisplayUnitType.DUT_MILLIMETERS, 1); }
        public double HoleDepth { get => RoundValue(Outline.MaximumPoint.Y - Outline.MinimumPoint.Y, DisplayUnitType.DUT_MILLIMETERS, 1); }
        public double LevelOffset { get => RoundValue(InsertionPoint.Z - Level.ProjectElevation, DisplayUnitType.DUT_MILLIMETERS, 1); }
        public double GroundOffset { get => RoundValue(LevelOffset + Level.ProjectElevation, DisplayUnitType.DUT_MILLIMETERS, 1); }

        /// <summary>
        /// Округляет число во внутренней системе исчисления относительно указанной
        /// </summary>
        /// <param name="x">Число для округления</param>
        /// <param name="displayUnitType">Система исчисления, относительно которой нужно округлять</param>
        /// <param name="i">Степень округления</param>
        /// <returns></returns>
        public double RoundValue(double x, DisplayUnitType displayUnitType, int i = 1)
        {
            double humanUnitValue = UnitUtils.ConvertFromInternalUnits(x, displayUnitType);
            double roundedValue = Math.Round(humanUnitValue / i) * i;
            return UnitUtils.ConvertToInternalUnits(roundedValue, displayUnitType);
        }

        /// <summary>
        /// Проверка на пересечение с другим отверстием
        /// </summary>
        /// <param name="other">Другое отверстие</param>
        /// <param name="tolerance">При положительных значениях проверяет пересечение в зазоре</param>
        /// <returns></returns>
        public bool IsIntersectedWithAnother(Intersection other, double tolerance)
        {
            // проверка на идентичность
            if (this.Outline.MinimumPoint == other.Outline.MinimumPoint &&
                this.Outline.MaximumPoint == other.Outline.MaximumPoint) return false;
            return this.Outline.Intersects(other.Outline, tolerance);
        }

        public static Outline CreateOutline(XYZ center, double x, double y, double z)
        {
            XYZ min = new XYZ(center.X - x / 2, center.Y - y / 2, center.Z - z / 2);
            XYZ max = new XYZ(center.X + x / 2, center.Y + y / 2, center.Z + z / 2);
            Outline outline = new Outline(min, max);
            return outline;
        }

        #region Initializations

        protected void DefaultInit()
            {
                InsertionPoint = new XYZ(
                (Outline.MinimumPoint.X + Outline.MaximumPoint.X) / 2,
                (Outline.MinimumPoint.Y + Outline.MaximumPoint.Y) / 2,
                Outline.MinimumPoint.Z
                );
                Level = GeometryUtils.GetLevelByPoint(Host.Document, InsertionPoint);
            }

        public Intersection()
        {
        }

        public Intersection(Outline outline)
        {
            Outline = outline;
            DefaultInit();
        }

        public Intersection(Outline outline, double angle, List<Element> hosts)
        {
            Outline = outline;
            Angle = angle;
            Hosts = hosts;
            DefaultInit();
        }

        public Intersection(Outline outline, double angle, Element host)
        {
            Outline = outline;
            Angle = angle;
            Host = host;
            DefaultInit();
        }
        #endregion

        #region Merging
        public static List<Intersection> AutoMerging(List<Intersection> intrs, double tolerance)
        {
            var pairs = SplitOnIntersectedPairs(intrs, tolerance);
            var groups = GroupIntersectedPairs(pairs);
            var intersections = groups.Select(x => Merge(x)).ToList();
            return intersections;

        }

        public static Intersection Merge(IEnumerable<Intersection> intrs)
        {
            Outline outline = intrs.First().Outline;
            for (int i = 1; i < intrs.Count(); i++)
            {
                Outline other = intrs.ElementAt(i).Outline;
                outline.AddPoint(other.MinimumPoint);
                outline.AddPoint(other.MaximumPoint);
            }            

            double angle = intrs.Select(x => x.Angle).FirstOrDefault();
            List<Element> hosts = intrs.Select(x=>x.Host).OfType<Element>().ToList();
            Document doc = hosts[0].Document;

            string name = string.Join("|", intrs.Select(x => x.Name).OfType<string>().Distinct());

            int id = intrs.ElementAt(0).Id;
            for (int i = 1; i < intrs.Count(); i++)
            {
                id = id | intrs.ElementAt(i).Id;
            }

            var intr = new Intersection(outline, angle, hosts);
            intr.Name = name;
            intr.Id = id;
            intr.IsBrick = intrs.Select(x => x.IsBrick).Any();
            return intr;
        }
    

        /// <summary>
        /// Объединяет списки по пересечению
        /// </summary>
        public static List<HashSet<Intersection>> GroupIntersectedPairs(List<HashSet<Intersection>> groups)
        {
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
            return groups;
        }

        /// <summary>
        /// Разбивает массив с пересечениями на пересекающиеся пары (или одиночные непересекаюзиеся)
        /// </summary>
        private static List<HashSet<Intersection>> SplitOnIntersectedPairs(List<Intersection> intersections, double tolerance)
        {
            List<HashSet<Intersection>> groups = new List<HashSet<Intersection>>();
            for (int i = 0; i < intersections.Count() - 1; i++)
            {
                bool collided = false;
                for (int j = i + 1; j < intersections.Count(); j++)
                {
                    var left = intersections.ElementAt(i);
                    var right = intersections.ElementAt(j);
                    if (left.IsIntersectedWithAnother(right, tolerance))
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
        #endregion


    }


    public class IntersectionMepCurve : Intersection
    {        
        public RevitLinkInstance LinkInstance { get; set; }
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
          
        /// <summary>
        /// Изначальная точка пересечения, взятая из отчета или анализа
        /// </summary>
        public XYZ CollisionPoint { get; }     
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
       
        double CalculateHorAngle()
        {
            XYZ vec = GeometryUtils.GetDuctDirection(PipeConnectorManager);
            // проецируем вектор на плоскость XY
            XYZ hor_vec = new XYZ(vec.X, vec.Y, 0);
            return hor_vec.AngleTo(XYZ.BasisY);
        }

        private XYZ calculateProjectedPoint()
        {
            Curve pipe_curve = GeometryUtils.FindDuctCurve(PipeConnectorManager);
            if (pipe_curve == null) return new XYZ(CollisionPoint.X, CollisionPoint.Y, CollisionPoint.Z);
            Transform tr = DocumentUtils.GetCorrectionTransform(LinkInstance);
            pipe_curve = pipe_curve.CreateTransformed(tr.Inverse);

            XYZ pt = null;
            if (Host is Wall)
            {
                Wall w = Host as Wall;
                LocationCurve wloc = w.Location as LocationCurve;
                Curve wall_curve = wloc.Curve;
                pipe_curve.MakeUnbound();       // на тот случай когда линия воздуховоды не проходит сквозь стену, а только ее часть
                var intersectCurve = pipe_curve.Project(wall_curve.GetEndPoint(0));
                pt = intersectCurve.XYZPoint;
            }
            else if (Host is Floor)
            {
                Solid s = GeometryUtils.GetSolid(Host);
                PlanarFace face = s.Faces.Cast<PlanarFace>().Where(x => x.FaceNormal.IsAlmostEqualTo(XYZ.BasisZ)).First();
                Surface surface = face.GetSurface();
                UV uv; 
                double distance;
                surface.Project(pipe_curve.GetEndPoint(0), out uv, out distance);
                pt = face.Evaluate(uv);
            }
            return pt;
        }

        private Outline CalculatePipeOutline()
        {
            // находим габариты MepCurve по размерам коннекторов
            // если они круглые - берем радиус, в противном случае ширина х высота
            var connectors = GeometryUtils.GetConnectors(PipeConnectorManager);
            var roundCons = connectors.Where(x => x.Shape == ConnectorProfileType.Round);
            var rectangleCons = connectors.Where(x => x.Shape == ConnectorProfileType.Rectangular);

            double maxR = roundCons.Count() > 0 ? roundCons.Max(x => x.Radius) * 2 : 0;
            double maxW = rectangleCons.Count() > 0 ? rectangleCons.Max(x => x.Width) : 0;
            double maxH = rectangleCons.Count() > 0 ? rectangleCons.Max(x => x.Height) : 0;

            double width = maxR > maxW ? maxR : maxW;
            double height = maxR > maxH ? maxR : maxH;
            
            // вычисляем толщины конструкции
            double depth = 1;
            if (Host is Wall)
            {
                depth = (Host as Wall).Width;
            }
            else if (Host is Floor)
            {
                depth = (Host as Floor).get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM).AsDouble();
            }

            // Определяем направление воздуховода и пересечение с конструкцией
            var direction = GeometryUtils.GetDuctOrientation(PipeConnectorManager);
            XYZ pt = calculateProjectedPoint();
            // Сеть идет строго вертикально
            if (direction == GeometryUtils.DuctOrientation.StraightVertical)
            {
                width += Offset * 2;
                height += Offset * 2;
                //смещаем на половину глубины, т.к. точка у перекрытий вычисляется на поверхности
                pt = new XYZ(pt.X, pt.Y, pt.Z - depth / 2);

                // определяем большую сторону сети по габаритам
                // т.к. в вертикальной плоскости ширина воздуховода может идти и по оси Х и по оси Y
                var bbox = Pipe.get_BoundingBox(null);
                XYZ diagonal = bbox.Max - bbox.Min;
                Outline outline;
                if (diagonal.X > diagonal.Y)
                {
                    outline = Intersection.CreateOutline(pt, width, height, depth);
                }
                else
                {
                    outline = Intersection.CreateOutline(pt, height, width, depth);
                }
                // угол не задает, т.к. в горзонтальной плоскости уже все развернуто
                return outline;
            }
            // Сеть идет в горизонтальной плоскости
            else if (direction == GeometryUtils.DuctOrientation.Horizontal)
            {
                width += Offset * 2;
                height += Offset * 2;
                var outline = Intersection.CreateOutline(pt, width, depth, height);
                // а здесь угол вычисляем
                Angle = CalculateHorAngle();
                return outline;
            }
            else
            {
                throw new NotImplementedException("Поддерживаются только вертикальные воздуховоды или расположенные в горизонтальной плоскости");
            }            
        }

        private bool IsBrickAnalyze()
        {
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

        public IntersectionMepCurve(Element host, Element pipe, XYZ pt, RevitLinkInstance instance)
        {
            Host = host;
            Pipe = pipe;
            Id = host.Id.GetHashCode() | pipe.Id.GetHashCode();
            LinkInstance = instance;
            CollisionPoint = pt;
            if (Host == null) throw new ArgumentNullException("Host", "Элемент не может быть null");
            if (Pipe == null) throw new ArgumentNullException("Pipe", "Элемент не может быть null");
            Outline = CalculatePipeOutline();
            IsBrick = IsBrickAnalyze();
            DefaultInit();
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
