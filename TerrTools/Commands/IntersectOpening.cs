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
    public class Intersection
    {
        public Element Host { get; set; }
        public Element Pipe { get; set; }
        public long Id { get {return long.Parse(Host.Id.ToString() + Pipe.Id.ToString()); } }
        public bool IsBrick { get; set; }
        public bool IsRound { get; set; }
        public XYZ CenterPoint { get; set; }
        public Level Level { get; set; }
        public double PipeWidth{get; set;}
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
                    int bricks = 1;
                    double bricksHeight = (bricks * 65 + (bricks - 1) * 10) / 304.8;
                    while (bricksHeight + 65/304.8 < nominalLevelOffset)
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
    class IntersectOpening : IExternalCommand
    {
        UIApplication uiapp;
        UIDocument uidoc;
        Document doc;
        Document linkedDoc;
        XYZ linkedDocOrigin;
        FamilySymbol openingFamilySymbol;

        private FamilySymbol FindOpeningFamily()
        {
            FamilySymbol symbol;
            Family openingFamily = new FilteredElementCollector(doc)
                .OfClass(typeof(Family))                
                .Where(x => x.Name == "ТеррНИИ_Компонент_Отверстие прямоугольное")
                .Cast<Family>()
                .FirstOrDefault();
            if (openingFamily != null)
            {
                symbol = (FamilySymbol)doc.GetElement(openingFamily.GetFamilySymbolIds().First());
                return symbol;
            }
            else
            {
                using (Transaction tr = new Transaction(doc, "Загрузка семейства"))
                {
                    tr.Start();
                    string path = @"\\serverl\psd\REVIT\Семейства\ТеррНИИ\АР\ТеррНИИ_Компонент_Отверстие прямоугольное.rfa";
                    doc.LoadFamilySymbol(path, "Прямоугольное", out symbol);
                    symbol.Activate();
                    tr.Commit();
                    return symbol;
                }

            }        
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uiapp = commandData.Application;
            uidoc = uiapp.ActiveUIDocument;
            doc = uidoc.Document;

            GetLinkedDoc();
            if (linkedDoc == null)
                return Result.Cancelled;            

            openingFamilySymbol = FindOpeningFamily();
            if (openingFamilySymbol == null)
            {
                TaskDialog.Show("Ошибка", "В проекте отсутствет семейство \"ТеррНИИ_Компонент_Отверстие прямоугольное\"\nДобавьте его и повторите попытку");
                return Result.Failed;
            }

            List<Intersection> intersections = GetIntersections();
            UI.IntersectionsForm form = new UI.IntersectionsForm(intersections);

            if (form.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                intersections = form.UpdatedIntersections;
                DeleteUnusedOpenings();
                bool result = PlaceOpeningFamilies(intersections);
                return Result.Succeeded;
            }
            else
            {
                return Result.Cancelled;
            }
        }

        private void DeleteUnusedOpenings()
        {
            using (Transaction tr = new Transaction(doc, "Удаление неиспользуемых отверстий"))
            {
                tr.Start();
                Element[] existingOpenings = new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilyInstance))
                    .Cast<FamilyInstance>()
                    .Where(x => x.Symbol.Id == openingFamilySymbol.Id)
                    .ToArray();
                foreach (Element op in existingOpenings)
                {
                    if (IsUnusedOpening(op)) doc.Delete(op.Id);
                }
                
                tr.Commit();
            }
        }

        private bool IsUnusedOpening(Element op)
        {
            // метод не реализован
            return false;
        }

        private bool PlaceOpeningFamilies(List<Intersection> intersections)
        {
            using (Transaction tr = new Transaction(doc, "Создание отверстий"))
            {
                tr.Start();
                try
                {
                    foreach (Intersection i in intersections)
                    {
                        Element holeElement = doc.Create.NewFamilyInstance(new XYZ (i.CenterPoint.X, i.CenterPoint.Y, i.Level.Elevation), openingFamilySymbol, i.Host, i.Level, StructuralType.NonStructural);
                        holeElement.LookupParameter("ADSK_Отверстие_Ширина").Set(i.HoleWidth);
                        holeElement.LookupParameter("ADSK_Отверстие_Высота").Set(i.HoleHeight);
                        holeElement.LookupParameter("ADSK_Отверстие_Функция").Set(i.Type);
                        holeElement.LookupParameter("ADSK_Отверстие_Отметка от этажа").Set(i.LevelOffset);
                        if (i.Level.Elevation == 0) holeElement.LookupParameter("ADSK_Отверстие_Отметка этажа").Set(0);
                        else holeElement.LookupParameter("ADSK_Отверстие_Отметка этажа").Set(i.Level.Elevation);
                        holeElement.LookupParameter("Идентификатор пересечения").Set(i.Id.ToString());
                    }
                    tr.Commit();
                    return true;
                }
                catch (NullReferenceException)
                {
                    TaskDialog.Show("Ошибка", "Отсутствуют требуемые параметры в семействе отверстий. Операция отменена");
                    tr.RollBack();
                    return false;
                }
            }
        }

        public void GetLinkedDoc()
        {
            try
            {
                Reference linkInstanceRef = uidoc.Selection.PickObject(
                    Autodesk.Revit.UI.Selection.ObjectType.Element,
                    new LinkedDocFilter(),
                    "Выберите документ, в котором содержатся помещения");
                RevitLinkInstance linkInstance = doc.GetElement(linkInstanceRef) as RevitLinkInstance;
                linkedDoc = linkInstance.GetLinkDocument();
                linkedDocOrigin = linkInstance.GetTransform().Origin;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                linkedDoc = null;
                linkedDocOrigin = null;
            }
        }

        private List<Intersection> GetIntersections()
        {
            FilteredElementCollector WallCollector = new FilteredElementCollector(doc);
            WallCollector.OfClass(typeof(Wall));
            List<Wall> walls = WallCollector.Cast<Wall>().ToList();

            FilteredElementCollector MEPCollector = new FilteredElementCollector(linkedDoc);
            MEPCollector.OfClass(typeof(MEPCurve));
            List<MEPCurve> meps = MEPCollector.Cast<MEPCurve>().ToList();

            List<Intersection> intersectionList = new List<Intersection>();
            foreach (Wall w in walls)
            {
                Face wallFace = FindWallFace(w);                
                //Transform tf = Transform.CreateTranslation(-linkedDocOrigin);
                //mepCurve = mepCurve.CreateTransformed(tf);
                foreach (MEPCurve m in meps)
                {
                    Curve mepCurve = FindDuctCurve(m);
                    if (mepCurve != null)
                    {
                        double height = mepCurve.GetEndPoint(0).Z;
                        XYZ interPt = null;
                        interPt = FindFaceIntersection(mepCurve, wallFace);
                        if (null != interPt)
                        {
                            Intersection i = new Intersection();
                            i.CenterPoint = interPt;
                            i.Host = w;
                            i.Pipe = m;
                            i.Level = doc.GetElement(w.LevelId) as Level;
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
                                i.PipeWidth = m.Width;
                                i.PipeHeight = m.Height;
                                i.IsRound = false;
                            }
                            //Определение, кирпичная ли стена
                            i.IsBrick = false;
                            foreach (var layer in w.WallType.GetCompoundStructure().GetLayers())
                            {
                                i.IsBrick = doc.GetElement(layer.MaterialId).Name.Contains("Кирпич") || i.IsBrick;
                            }
                            intersectionList.Add(i);
                        }
                    }                
                }
            }
            return intersectionList;
        }

        //Find the wind pipe corresponding curve
        public Curve FindDuctCurve(MEPCurve mepCurve)
        {
            //The wind pipe curve
            IList<XYZ> list = new List<XYZ>();
            try
            {
                ConnectorSetIterator csi = mepCurve.ConnectorManager.Connectors.ForwardIterator();
                while (csi.MoveNext())
                {
                    Connector conn = csi.Current as Connector;
                    list.Add(conn.Origin);
                }
                Curve curve = Line.CreateBound(list.ElementAt(0), list.ElementAt(1)) as Curve;
                return curve;
            }
            catch (Autodesk.Revit.Exceptions.InvalidOperationException)
            {
                return null;
            }
        }

        public Face FindWallFace(Wall wall)
        {
            List<Face> normalFaces = new List<Face>();

            Options opt = new Options();
            opt.ComputeReferences = true;
            opt.DetailLevel = ViewDetailLevel.Fine;

            GeometryElement e = wall.get_Geometry(opt);

            foreach (GeometryObject obj in e)
            {
                Solid solid = obj as Solid;

                if (solid != null && solid.Faces.Size > 0)
                {
                    foreach (Face face in solid.Faces)
                    {
                        PlanarFace pf = face as PlanarFace;
                        if (pf != null)
                        {
                            normalFaces.Add(pf);
                        }
                    }
                }
            }
            return normalFaces.OrderBy(i => i.Area).Last();
        }

        public XYZ FindFaceIntersection(Curve DuctCurve, Face WallFace)
        {
            //The intersection point
            IntersectionResultArray intersectionR = new IntersectionResultArray();//Intersection point set
            SetComparisonResult results;//Results of Comparison
            results = WallFace.Intersect(DuctCurve, out intersectionR);
            XYZ intersectionResult = null;//Intersection coordinate
            if (SetComparisonResult.Disjoint != results)            {
                if (intersectionR != null)                {
                    if (!intersectionR.IsEmpty)                    {
                        intersectionResult = intersectionR.get_Item(0).XYZPoint;
                    }
                }
            }
            return intersectionResult;
        }
    }
}
