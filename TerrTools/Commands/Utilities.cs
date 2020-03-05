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
    static class SharedParameterUtils
    {
        static public string sharedParameterFilePath = @"\\serverL\PSD\REVIT\ФОП\ФОП2017.txt";
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
        static public List<List<Curve>> GetCurvesListFromRoom(Room room)
        {
            List<List<Curve>> profiles = new List<List<Curve>>();
            SpatialElementBoundaryOptions opt = new SpatialElementBoundaryOptions();
            IList<IList<BoundarySegment>> boundaries = room.GetBoundarySegments(opt);
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
        static public RevitLinkInstance GetLinkedDoc(Document doc)
        {
            RevitLinkInstance[] linkedDocs = new FilteredElementCollector(doc).OfClass(typeof(RevitLinkInstance)).Cast<RevitLinkInstance>().ToArray();
            var form = new UI.OneComboboxForm("Выберите связанный файл", (from d in linkedDocs select d.Name).ToArray());
            if (form.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                RevitLinkInstance linkInstance = (from d in linkedDocs where d.Name == form.SelectedItem select d).First();
                return linkInstance;
            }
            else
            {
                return null;
            }
        }
        static public Transform GetCorrectionTransform(RevitLinkInstance linkedDocInstance)
        {
            Transform transform = linkedDocInstance.GetTransform();
            if (!transform.AlmostEqual(Transform.Identity)) return transform.Inverse;
            else return Transform.Identity;
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
    }
}
