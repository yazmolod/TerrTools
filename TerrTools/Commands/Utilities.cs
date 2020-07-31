using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;


namespace TerrTools
{
    static class GlobalVariables
    {
#if DEBUG
        public const bool DebugMode = true;
#else
       public const bool DebugMode = false;
#endif

        public const double MinThreshold = 1.0 / 12.0 / 16.0;
    }

    static class SharedParameterUtils
    {
        static public string sharedParameterFilePath = @"\\serverL\PSD\REVIT\ФОП\ФОП2017.txt";

        static public bool AddSharedParameter(Document doc, Updaters.SharedParameterSettings s)
        {
            return AddSharedParameter(doc, s.ParameterName, s.Categories, s.ParameterGroup, s.IsInstance);
        }

        static public bool AddSharedParameter(
            Document doc,
            string parameterName,
            BuiltInCategory[] categories,
            BuiltInParameterGroup group = BuiltInParameterGroup.PG_ADSK_MODEL_PROPERTIES,
            bool isIntance = true
            )
        {
            // Проверка параметра на наличие
            List<bool> check = new List<bool>();
            foreach (BuiltInCategory cat in categories)
            {
                check.Add(IsParameterInProject(doc, parameterName, cat));
            }

            if (check.All(x => x == true))
            {
                return true;
            }
            else
            {
                CategorySet catSet = doc.Application.Create.NewCategorySet();
                foreach (BuiltInCategory c in categories)
                {
                    catSet.Insert(doc.Settings.Categories.get_Item(c));
                }

                try
                {
                    doc.Application.SharedParametersFilename = sharedParameterFilePath;
                    DefinitionFile spFile = doc.Application.OpenSharedParameterFile();
                    ExternalDefinition sharedDef = null;
                    foreach (DefinitionGroup gr in spFile.Groups)
                    {
                        foreach (Definition def in gr.Definitions)
                        {
                            if (def.Name == parameterName) sharedDef = (ExternalDefinition)def;
                        }
                    }
                    if (sharedDef == null)
                    {
                        TaskDialog.Show("Ошибка", String.Format("Параметр \"{0}\" отсутствует в файле общих параметров", parameterName));
                        return false;
                    }

                    ElementBinding bind;
                    if (isIntance) bind = doc.Application.Create.NewInstanceBinding(catSet);
                    else bind = doc.Application.Create.NewTypeBinding(catSet);
                    bool result = doc.ParameterBindings.Insert(sharedDef, bind, group);
                    // Если параметр уже существует, но данная категория отсутствует в сете - обновляем сет
                    if (!result)
                    {
                        string realName = GetRealParameterName(doc, sharedDef);
                        bind = GetParameterBinding(doc, realName);
                        foreach (Category c in catSet) bind.Categories.Insert(c);
                        bool result2 = doc.ParameterBindings.ReInsert(sharedDef, bind, group);
                        if (!result2)
                        {
                            TaskDialog.Show("Ошибка", String.Format("Произошла ошибка при редактировании привязок существующего параметра \"{0}\"", parameterName));
                            return false;
                        }
                    }
                    return true;

                }
                catch (Exception e)
                {
                    TaskDialog td = new TaskDialog("Ошибка");
                    td.MainInstruction = String.Format("Произошла ошибка добавления общего параметра \"{0}\"", parameterName);
                    td.MainContent = e.ToString();
                    td.Show();
                    return false;
                }
            }
        }

        private static string GetRealParameterName(Document doc, ExternalDefinition def)
        {
            foreach (var i in new FilteredElementCollector(doc).OfClass(typeof(SharedParameterElement)).Cast<SharedParameterElement>())
            {
                if (i.GuidValue.ToString() == def.GUID.ToString()) return i.Name;
            }
            return def.Name;
        }

        static public bool IsParameterInProject(Document doc, string parameterName, BuiltInCategory cat)
        {
            Category category = doc.Settings.Categories.get_Item(cat);
            BindingMap map = doc.ParameterBindings;
            DefinitionBindingMapIterator it = map.ForwardIterator();
            bool result = false;
            it.Reset();
            while (it.MoveNext())
            {
                result = result || (it.Key.Name == parameterName && (it.Current as ElementBinding).Categories.Contains(category));
            }
            return result;
        }

        static private ElementBinding GetParameterBinding(Document doc, string parameterName)
        {
            BindingMap map = doc.ParameterBindings;
            DefinitionBindingMapIterator it = map.ForwardIterator();
            it.Reset();
            while (it.MoveNext())
            {
                if (it.Key.Name == parameterName) return it.Current as ElementBinding;
            }
            return null;
        }
    }

    static class FamilyInstanceUtils
    {
        public static string SizeLookup(FamilySizeTable fmt, string columnName, string[] args)
        {
            int columnIndex = -1;
            for (int i = 0; i < fmt.NumberOfColumns; i++)
            {
                if (fmt.GetColumnHeader(i).Name == columnName) columnIndex = i;
            }
            if (columnIndex != -1)
            {
                for (int i = 0; i < fmt.NumberOfRows; i++)
                {
                    bool flag = true;
                    string value;
                    for (int j = 0; j < args.Length; j++)
                    {
                        value = fmt.AsValueString(i, j + 1);
                        flag = flag && value.StartsWith(args[j]);
                    }
                    if (flag) return fmt.AsValueString(i, columnIndex);
                }
            }
            return null;
        }

        public static bool MirroredIndicator(FamilyInstance el)
        {
            Document doc = el.Document;
            string paramName = "ТеррНИИ_Элемент отзеркален";
            int value = el.Mirrored ? 1 : 0;
            using (Transaction tr = new Transaction(doc, "Индикатор отзеркаливания"))
            {
                tr.Start();
                SharedParameterUtils.AddSharedParameter(doc, paramName,
                new BuiltInCategory[] { (BuiltInCategory)el.Category.Id.IntegerValue }, BuiltInParameterGroup.PG_ANALYSIS_RESULTS);
                el.LookupParameter(paramName).Set(value);
                tr.Commit();
            }
            return el.Mirrored;
        }
    }

    static class CurveUtils
    {
        static private bool IsSamePoint(XYZ pt1, XYZ pt2, double thres = 0.017)
        {
            var dist = pt1.DistanceTo(pt2);
            return dist < thres;
        }

        static private bool GetFirstColinear(ref List<MyBoundarySegment> lines)
        {
            for (int li = 0; li < lines.Count; li++)
            {
                Curve thisLine = lines[li].Curve;
                for (int lj = li + 1; lj < lines.Count; lj++)
                {
                    Curve nextLine = lines[lj].Curve;
                    XYZ[] pts = new XYZ[]
                    {
                        thisLine.GetEndPoint(0),
                        thisLine.GetEndPoint(1),
                        nextLine.GetEndPoint(0),
                        nextLine.GetEndPoint(1)
                    };
                    bool samepts = new bool[]
                    {
                        IsSamePoint(pts[0], pts[2]),
                        IsSamePoint(pts[0], pts[3]),
                        IsSamePoint(pts[1], pts[2]),
                        IsSamePoint(pts[1], pts[3])
                    }.Any();
                    XYZ dir1 = (pts[1] - pts[0]).Normalize();
                    XYZ dir2 = (pts[3] - pts[2]).Normalize();
                    bool samedir = dir1.IsAlmostEqualTo(dir2) || dir1.IsAlmostEqualTo(dir2.Negate());

                    if (samepts && samedir)
                    {
                        XYZ minpt = pts.OrderBy(x => x.X).ThenBy(x => x.Y).First();
                        XYZ maxpt = pts.OrderBy(x => x.X).ThenBy(x => x.Y).Last();
                        Curve unionCurve = Line.CreateBound(minpt, maxpt);
                        var unionId = lines[li].ElementId;
                        lines.RemoveAt(lj);
                        lines.RemoveAt(li);
                        lines.Insert(li, new MyBoundarySegment(unionCurve, unionId));
                        return true;
                    }
                }
            }
            return false;
        }



        static Curve CreateReversedCurve(Curve orig)
        {
            if (orig is Line)
            {
                return Line.CreateBound(
                  orig.GetEndPoint(1),
                  orig.GetEndPoint(0));
            }
            else if (orig is Arc)
            {
                return Arc.Create(orig.GetEndPoint(1),
                  orig.GetEndPoint(0),
                  orig.Evaluate(0.5, true));
            }
            else
            {
                throw new Exception(
                  "CreateReversedCurve - Unreachable");
            }
        }

        public static void FixCurvesSelfIntersection(List<Curve> curves)
        {
            for (int x = 0; x < curves.Count; x++)
            {
                for (int y = x + 1; y < curves.Count; y++)
                {
                    var c1 = curves[x];
                    var c2 = curves[y];
                    IntersectionResultArray ResultArray;
                    var result = c1.Intersect(c2, out ResultArray);
                    if (result == SetComparisonResult.Overlap)
                    {
                        var interResult = ResultArray.get_Item(0);  // опасный ход, но вроде должны быть только пересекающиеся линии
                        if (!IsSamePoint(interResult.XYZPoint, c1.GetEndPoint(0))
                            && !IsSamePoint(interResult.XYZPoint, c1.GetEndPoint(1)))
                        {
                            // производим некий аналог Trim, оставляя линии подлинеее
                            var temp1A = Line.CreateBound(c1.GetEndPoint(0), interResult.XYZPoint);
                            var temp1B = Line.CreateBound(c1.GetEndPoint(1), interResult.XYZPoint);
                            var temp2A = Line.CreateBound(c2.GetEndPoint(0), interResult.XYZPoint);
                            var temp2B = Line.CreateBound(c2.GetEndPoint(1), interResult.XYZPoint);
                            var new_c1 = temp1A.Length > temp1B.Length ? temp1A : temp1B;
                            var new_c2 = temp2A.Length > temp2B.Length ? temp2A : temp2B;
                            curves[x] = new_c1;
                            curves[y] = new_c2;
                        }
                    }
                }
            }        
        }

        public static void UniteDisjointCurvesInList(int ind1, int ind2, List<Curve> curves)
        {
            var c1 = curves[ind1];
            var c2 = curves[ind2];
            var union_c = Line.CreateBound(c1.GetEndPoint(0), c2.GetEndPoint(1));
            curves.Insert(ind1, union_c);
            curves.Remove(c1);
            curves.Remove(c2);
            
        }

        public static bool IsCurvesListClosed(List<Curve> curves, out int disjointIndex)
        {
            disjointIndex = -1;
            if (curves.First().Intersect(curves.Last()) == SetComparisonResult.Disjoint)
            {
                disjointIndex = curves.Count - 1;
                return false;
            }
            for (int i = 0; i < curves.Count - 1; i++)
            {
                disjointIndex = i;
                if (curves[i].Intersect(curves[i + 1]) == SetComparisonResult.Disjoint) return false;
            }
            return true;
        }


        public static void FixDeleteUnused(List<Curve> curves)
        {
            var points = curves.Select(x => x.GetEndPoint(0)).Concat(curves.Select(x => x.GetEndPoint(1))).ToList();
            List<int> indexToDelete = new List<int>();
            for (int i=0; i<curves.Count; i++)
            {
                var pt1 = curves[i].GetEndPoint(0);
                var pt2 = curves[i].GetEndPoint(1);
                if (points.Count(x => IsSamePoint(x, pt1)) < 2 ||
                    points.Count(x => IsSamePoint(x, pt2)) < 2)
                {
                    indexToDelete.Add(i);
                }
            }
            indexToDelete.Reverse();
            foreach (int i in indexToDelete) curves.RemoveAt(i);
        }

        public static void FixCurvesOpenEnd(List<Curve> curves)
        {
            // проверяем первую и последнюю линию в списке

            int disjointIndex;
            while (!IsCurvesListClosed(curves, out disjointIndex))
            {
                if (disjointIndex == curves.Count - 1)
                {
                    UniteDisjointCurvesInList(curves.Count - 1, 0, curves);                    
                }
                else
                {
                    UniteDisjointCurvesInList(disjointIndex, disjointIndex + 1, curves);
                }
            }
        }

        public static void SortCurvesContiguous(IList<Curve> curves)
        {
            //взято с https://thebuildingcoder.typepad.com/blog/2013/03/sort-and-orient-curves-to-form-a-contiguous-loop.html
            int n = curves.Count;

            // Walk through each curve (after the first) 
            // to match up the curves in order

            for (int i = 0; i < n; ++i)
            {
                Curve curve = curves[i];
                XYZ endPoint = curve.GetEndPoint(1);
                XYZ p;

                // Find curve with start point = end point
                for (int j = i + 1; j < n; ++j)
                {
                    p = curves[j].GetEndPoint(0);

                    // If there is a match end->start, 
                    // this is the next curve

                    if (GlobalVariables.MinThreshold > p.DistanceTo(endPoint))
                    {
                        if (i + 1 != j)
                        {
                            Curve tmp = curves[i + 1];
                            curves[i + 1] = curves[j];
                            curves[j] = tmp;
                        }
                        break;
                    }

                    p = curves[j].GetEndPoint(1);

                    // If there is a match end->end, 
                    // reverse the next curve

                    if (GlobalVariables.MinThreshold > p.DistanceTo(endPoint))
                    {
                        if (i + 1 == j)
                        {
                            curves[i + 1] = CreateReversedCurve(curves[j]);
                        }
                        else
                        {
                            Curve tmp = curves[i + 1];
                            curves[i + 1] = CreateReversedCurve(curves[j]);
                            curves[j] = tmp;
                        }
                        break;
                    }
                }
            }
        }

        static public void FixContourProblems(List<Curve> curves)
        {            
            FixCurvesSelfIntersection(curves);
            FixDeleteUnused(curves);
            SortCurvesContiguous(curves);
            FixCurvesOpenEnd(curves);            
        }

        static public List<List<Curve>> GetCurvesListFromSpatialElement(SpatialElement spatial)
        {
            List<List<Curve>> profiles = new List<List<Curve>>();
            SpatialElementBoundaryOptions opt = new SpatialElementBoundaryOptions();
            IList<IList<BoundarySegment>> boundaries = spatial.GetBoundarySegments(opt);
            for (int i = 0; i < boundaries.Count; i++)
            {
                profiles.Add(new List<Curve>());
                foreach (BoundarySegment s in boundaries[i])
                {
                    profiles[i].Add(s.GetCurve());
                }
            }
            return profiles;
        }

        static public List<List<Curve>> GetRoomWithDoorsContour(SpatialElement spatial)
        {
            throw new NotImplementedException();
        }
    }

    static class GeometryUtils
    {       
        static public Solid GetSolid(Element e)
        {
            Options opt = new Options();
            GeometryElement geomElem = e.get_Geometry(opt);
            foreach (GeometryObject geomObj in geomElem)
            {
                Solid geomSolid = geomObj as Solid;
                if (null != geomSolid) return geomSolid;
            }
            return null;
        }

        static public List<Connector> GetConnectors(ConnectorManager conMngr)
        {
            List<Connector> list = new List<Connector>();
            ConnectorSetIterator csi = conMngr.Connectors.ForwardIterator();
            while (csi.MoveNext())
            {
                Connector conn = csi.Current as Connector;
                list.Add(conn);
            }
            return list;
        }

        static public Curve FindDuctCurve(ConnectorManager conMngr)
        {
            try
            {
                var connectors = GetConnectors(conMngr);
                var connectorsPts = connectors.Select(x => x.Origin);
                Curve curve = Line.CreateBound(connectorsPts.First(), connectorsPts.Last());
                return curve;
            }
            catch (Autodesk.Revit.Exceptions.InvalidOperationException)
            {
                return null;
            }
            catch (Autodesk.Revit.Exceptions.ArgumentsInconsistentException)
            {
                return null;
            }
        }

        static public List<Face> GetFaces(Element e)
        {
            List<Face> normalFaces = new List<Face>();

            Solid solid = GeometryUtils.GetSolid(e);
            foreach (Face face in solid.Faces)
            {
                PlanarFace pf = face as PlanarFace;
                if (pf != null) normalFaces.Add(pf);
            }
            return normalFaces;
        }                  

        static public RevitLinkInstance GetLinkedDoc(Document doc)
        {
            RevitLinkInstance[] linkedDocs = new FilteredElementCollector(doc).OfClass(typeof(RevitLinkInstance)).Cast<RevitLinkInstance>().ToArray();
            var form = new UI.OneComboboxForm("Выберите связанный файл", (from d in linkedDocs select d.Name).ToArray());
            if (form.DialogResult == System.Windows.Forms.DialogResult.OK && form.SelectedItem!="")
            {
                RevitLinkInstance linkInstance = (from d in linkedDocs where d.Name == form.SelectedItem select d).First();
                return linkInstance;
            }
            else
            {
                return null;
            }
        }

        static public RevitLinkInstance ChooseLinkedDoc(Document currentDoc)
        {
            RevitLinkInstance linkedDocInstance = GetLinkedDoc(currentDoc);
            if (linkedDocInstance == null)
                return null;

            else if (linkedDocInstance.GetLinkDocument() == null)
            {
                TaskDialog.Show("Ошибка", "Обновите элемент связи в проекте");
                return null;
            }
            else
            {
                return linkedDocInstance;
            }
        }

        static public Transform GetCorrectionTransform(RevitLinkInstance linkedDocInstance)
        {
            Transform transform = linkedDocInstance.GetTransform();
            if (!transform.AlmostEqual(Transform.Identity)) return transform.Inverse;
            else return Transform.Identity;
        }

    }

    class MyBoundarySegment
    {
        public Curve Curve { get; set; }
        public ElementId ElementId { get; set; }
        public MyBoundarySegment (BoundarySegment b)
        {
            Curve = b.GetCurve();
            ElementId = b.ElementId;
        }
        public MyBoundarySegment(Curve c, ElementId id)
        {
            Curve = c;
            ElementId = id;
        }
    }

    class ModelCurveCreator {

        Autodesk.Revit.ApplicationServices.Application _app;
        Document _doc;

        public ModelCurveCreator(Document doc)
        {
            _doc = doc;
            _app = doc.Application;
        }

        static public Curve GetFamilyInstanceCutBaseLine(FamilyInstance fi)
        {
            XYZ dir;
            CurveLoop loop = ExporterIFCUtils.GetInstanceCutoutFromWall(fi.Document, fi.Host as Wall, fi, out dir);
            Curve curve = loop.Where(x => x.GetEndPoint(0).Z == x.GetEndPoint(1).Z).OrderBy(x => x.GetEndPoint(0).Z).FirstOrDefault();
            return curve;
        }

        private Plane NewPlanePassLine(Curve curve)
        {
            XYZ p = curve.GetEndPoint(0);
            XYZ q = curve.GetEndPoint(1);
            if (p.X == q.X) return Plane.CreateByNormalAndOrigin(XYZ.BasisX, p);
            else if (p.Y == q.Y) return Plane.CreateByNormalAndOrigin(XYZ.BasisY, p);
            else if (p.Z == q.Z) return Plane.CreateByNormalAndOrigin(XYZ.BasisZ, p);
            else return Plane.CreateByThreePoints(p, q, XYZ.Zero);
        }

        public ModelCurve MakeModelCurve(Curve curve)
        {
            SketchPlane skPlane = SketchPlane.Create(_doc, NewPlanePassLine(curve));
            try
            {
                ModelCurve modelCurve = _doc.Create.NewModelCurve(curve, skPlane);
                return modelCurve;
            }
            catch (Autodesk.Revit.Exceptions.ArgumentException)
            {
                TaskDialog.Show("Error", "Не найдена плоскость для линии");
                return null;
            }
        }

        public ModelCurveArray MakeModelCurve(CurveArray arr)
        {
            ModelCurveArray array = new ModelCurveArray();
            foreach (Curve c in arr)
            {
                ModelCurve mc = MakeModelCurve(c);
                array.Append(mc);
            }
            return array;
        }

        public ModelCurve MakeModelCurve(XYZ p1, XYZ p2)
        {
            Curve curve = Line.CreateBound(p1, p2);
            ModelCurve modelCurve = MakeModelCurve(curve);
            return modelCurve;
        }

        public ModelCurveArray MakeModelCurve(XYZ[] pts, bool close = true)
        {
            ModelCurve modelCurve;
            ModelCurveArray array = new ModelCurveArray();
            if (pts.Length < 3)
            {
                throw new ArgumentException("Требуется больше трех точек");
            }
            for (int i = 0; i < pts.Length - 1; i++)
            {
                modelCurve = MakeModelCurve(pts[i], pts[i + 1]);
                array.Append(modelCurve);
            }
            if (close)
            {
                modelCurve = MakeModelCurve(pts.First(), pts.Last());
                array.Append(modelCurve);
            }
            return array;
        }                

        public ModelCurveArray MakeModelCurve(IEnumerable<Curve> curves)
        {
            ModelCurveArray array = new ModelCurveArray();
            foreach (Curve curve in curves)
            {
                ModelCurve modelCurve = MakeModelCurve(curve);
                array.Append(modelCurve);
            }
            return array;
        }

        public ModelCurveArray MakeModelCurve(BoundingBoxXYZ bb)
        {
            ModelCurveArray array = new ModelCurveArray();
            XYZ p1 = bb.Min;
            XYZ p2 = bb.Max;

            XYZ[][] pairs =
            {
                new XYZ[] {new XYZ(p1.X, p1.Y, p1.Z), new XYZ(p2.X, p1.Y, p1.Z) },
                new XYZ[] {new XYZ(p2.X, p2.Y, p1.Z), new XYZ(p2.X, p1.Y, p1.Z) },
                new XYZ[] {new XYZ(p2.X, p2.Y, p1.Z), new XYZ(p1.X, p2.Y, p1.Z) },
                new XYZ[] {new XYZ(p1.X, p1.Y, p1.Z), new XYZ(p1.X, p2.Y, p1.Z) },

                new XYZ[] {new XYZ(p1.X, p1.Y, p2.Z), new XYZ(p2.X, p1.Y, p2.Z) },
                new XYZ[] {new XYZ(p2.X, p2.Y, p2.Z), new XYZ(p2.X, p1.Y, p2.Z) },
                new XYZ[] {new XYZ(p2.X, p2.Y, p2.Z), new XYZ(p1.X, p2.Y, p2.Z) },
                new XYZ[] {new XYZ(p1.X, p1.Y, p2.Z), new XYZ(p1.X, p2.Y, p2.Z) },

                new XYZ[] {new XYZ(p1.X, p1.Y, p1.Z), new XYZ(p1.X, p1.Y, p2.Z) },
                new XYZ[] {new XYZ(p2.X, p1.Y, p1.Z), new XYZ(p2.X, p1.Y, p2.Z) },
                new XYZ[] {new XYZ(p2.X, p2.Y, p1.Z), new XYZ(p2.X, p2.Y, p2.Z) },
                new XYZ[] {new XYZ(p1.X, p2.Y, p1.Z), new XYZ(p1.X, p2.Y, p2.Z) }
            };

            foreach (var pair in pairs)
            {
                ModelCurve modelCurve = MakeModelCurve(pair[0], pair[1]);
                array.Append(modelCurve);
            }
            return array;
        }

        public void DrawGroup(CurveArray curves, string name)
        {
            ModelCurveArray roomShape = MakeModelCurve(curves);
            Group group = _doc.Create.NewGroup(roomShape.Cast<ModelCurve>().Select(x => x.Id).ToList());
            group.GroupType.Name = name;
        }
    }
}
