using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Architecture;
using System.Diagnostics;

namespace TerrTools.Updaters
{
    public abstract class TerrUpdater : IUpdater
    {
        private string mName;
        private string mInfo;
        private Guid mGuid;
        private ChangePriority mPriority;
        protected Document doc;
        public bool IsActive { get; set; }

        public TerrUpdater(string name, string guid, string info, ChangePriority priority)
        {
            mName = name;
            mInfo = info;
            mGuid = new Guid(guid);
            mPriority = priority;
            IsActive = true;
        }
        public abstract void InnerExecute(UpdaterData data);

        public void Execute(UpdaterData data)
        {
            if (IsActive)
            {
                doc = data.GetDocument();
                InnerExecute(data);
            }
        }

        public string GetAdditionalInformation()
        {
            return mInfo;
        }

        public ChangePriority GetChangePriority()
        {
            return mPriority;
        }

        public UpdaterId GetUpdaterId()
        {
            return new UpdaterId(TerrToolsApp.AddInId, mGuid);
        }

        public string GetUpdaterName()
        {
            return mName;
        }
    }

public class MirroredInstancesUpdater : TerrUpdater
    {
        public MirroredInstancesUpdater(string name, string guid, string info, ChangePriority priority) : base(name, guid, info, priority) { }

        public override void InnerExecute(UpdaterData data)
        {
            string paramName = "ТеррНИИ_Элемент отзеркален";
            foreach (ElementId changedElemId in data.GetModifiedElementIds())
            {
                FamilyInstance el = doc.GetElement(changedElemId) as FamilyInstance;
                if (el != null)
                {
                    BuiltInCategory cat = (BuiltInCategory)el.Category.Id.IntegerValue;
                    bool result = SharedParameterUtils.AddSharedParameter(doc, paramName,
                         new BuiltInCategory[] { cat }, BuiltInParameterGroup.PG_ANALYSIS_RESULTS);
                    int value = el.Mirrored ? 1 : 0;
                    el.LookupParameter(paramName).Set(value);
                }
            }            
        }
    }

    public class SpaceUpdater : TerrUpdater
    {
        public SpaceUpdater(string name, string guid, string info, ChangePriority priority) : base(name, guid, info, priority) { }

        public override void InnerExecute(UpdaterData data)
        {
            var modified = data.GetModifiedElementIds();
             var added = data.GetAddedElementIds();
            foreach (ElementId id in added.Concat(modified))
            {
                try
                {
                    Element space = doc.GetElement(id);
                    SpaceNaming.TransferData(space);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.StackTrace);
                    Debug.WriteLine("Element id: " + id.IntegerValue.ToString());
                }
            }            
        }
    }
        
    public class DuctsUpdater : TerrUpdater
    {
        public DuctsUpdater(string name, string guid, string info, ChangePriority priority) : base(name, guid, info, priority) { }
        private string GetDuctClass(Duct el)
        {
            bool isInsul = el.get_Parameter(BuiltInParameter.RBS_REFERENCE_INSULATION_THICKNESS).AsDouble() > 0;
            string insulType = el.get_Parameter(BuiltInParameter.RBS_REFERENCE_INSULATION_TYPE).AsString();
            Element[] insulTypes = new FilteredElementCollector(doc).OfClass(typeof(DuctInsulationType)).Where(x => x.Name == insulType).ToArray();
            // Параметр "Комментарий к типоразмеру"
            string comment = insulTypes.Length > 0 ? insulTypes[0].get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS).AsString() : "";
            if (insulTypes.Length > 0 && comment?.IndexOf("огнезащит", StringComparison.OrdinalIgnoreCase) >= 0) return "А с огнезащитой";
            return "B";
        }
        private double GetDuctThickness(Duct el)
        {
            double thickness = -1;
            bool isInsul = el.get_Parameter(BuiltInParameter.RBS_REFERENCE_INSULATION_THICKNESS).AsDouble() > 0;
            string systemType = el.get_Parameter(BuiltInParameter.RBS_DUCT_SYSTEM_TYPE_PARAM).AsValueString();
            bool isRect = false;
            bool isRound = false;
            double size;
            try
            {
                size = el.Width >= el.Height ? el.Width : el.Height;
                isRect = true;
            }
            catch
            {
                size = el.Diameter;
                isRound = true;
            }
            size = UnitUtils.ConvertFromInternalUnits(size, DisplayUnitType.DUT_MILLIMETERS);

            if (systemType.IndexOf("дым", StringComparison.OrdinalIgnoreCase) >= 0) 
            {
                thickness = 1.0;
            }
            else
            {
                if (isRect)
                {
                    if (size <= 250) thickness = isInsul ? 0.8 : 0.5;
                    else if (250 < size && size <= 1000) thickness = isInsul ? 0.8 : 0.7;
                    else if (1000 < size && size <= 2000) thickness = isInsul ? 0.9 : 0.9;
                }
                else if (isRound)
                {
                    if (size <= 200) thickness = isInsul ? 0.8 : 0.5;
                    else if (200 < size && size <= 450) thickness = isInsul ? 0.8 : 0.6;
                    else if (450 < size && size <= 800) thickness = isInsul ? 0.8 : 0.7;
                    else if (800 < size && size <= 1250) thickness = isInsul ? 1.0 : 1.0;
                }
            }
            thickness = UnitUtils.ConvertToInternalUnits(thickness, DisplayUnitType.DUT_MILLIMETERS);
            return thickness;
        }

        private double GetLevelHeight(Duct el)
        {
            double[] zs = (from Connector con in el.ConnectorManager.Connectors select con.Origin.Z).ToArray();
            return zs.Min();
        }

        public override void InnerExecute(UpdaterData data)
        {
            string thickParameter = "ADSK_Толщина стенки";
            string classParameter = "ТеррНИИ_Класс герметичности";
            string levelParameter = "ТеррНИИ_Отметка от нуля";
            SharedParameterUtils.AddSharedParameter(doc, thickParameter, new BuiltInCategory[] { BuiltInCategory.OST_DuctCurves });
            SharedParameterUtils.AddSharedParameter(doc, classParameter, new BuiltInCategory[] { BuiltInCategory.OST_DuctCurves });
            SharedParameterUtils.AddSharedParameter(doc, levelParameter, new BuiltInCategory[] { BuiltInCategory.OST_DuctCurves });
            foreach (ElementId id in data.GetModifiedElementIds().Concat(data.GetAddedElementIds()))
            {
                try
                {
                    Duct el = (Duct)doc.GetElement(id);
                    el.LookupParameter(thickParameter).Set(GetDuctThickness(el));
                    el.LookupParameter(classParameter).Set(GetDuctClass(el));
                    el.LookupParameter(levelParameter).Set(GetLevelHeight(el));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.StackTrace);
                    Debug.WriteLine("Element id: " + id.IntegerValue.ToString());
                }
            }
        }
        
    }

    public class DuctsAccessoryUpdater : TerrUpdater
    {
        public DuctsAccessoryUpdater(string name, string guid, string info, ChangePriority priority) : base(name, guid, info, priority) { }

        public override void InnerExecute(UpdaterData data)
        {

            var modified = data.GetModifiedElementIds();
            var added = data.GetAddedElementIds();
            foreach (ElementId id in modified.Concat(added))
            {
                try
                {
                    Element el = doc.GetElement(id);
                    string rawSize = el.get_Parameter(BuiltInParameter.RBS_CALCULATED_SIZE).AsString();
                    string size = rawSize.Split('-')[0].Replace("м", "").Replace(" ", "");
                    el.LookupParameter("Марка").Set(size);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.StackTrace);
                    Debug.WriteLine("Element id: " + id.IntegerValue.ToString());
                }
            }
        }
    }

    public class PartUpdater : TerrUpdater
    {
        public PartUpdater(string name, string guid, string info, ChangePriority priority) : base(name, guid, info, priority) { }
        private double GetThickness(Part el)
        {
            double layerWidth = el.get_Parameter(BuiltInParameter.DPART_LAYER_WIDTH).AsDouble();
            Options opt = new Options();
            Solid solid = el.get_Geometry(opt).FirstOrDefault() as Solid;
            if (solid != null)
            {
                List<Face> faces = new List<Face>();
                foreach (Face face in solid.Faces)
                {
                    bool widthface = false;
                    foreach (CurveLoop loop in face.GetEdgesAsCurveLoops())
                    {
                        foreach (Curve edge in loop)
                        {
                            widthface = widthface || Math.Abs(edge.Length - layerWidth) < GlobalVariables.MinThreshold;
                        }
                    }
                    if (el.GetFaceOffset(face) != 0 && !widthface) faces.Add(face);
                }
                if (faces.Count() != 0) layerWidth += el.GetFaceOffset(faces[0]);
            }
            return layerWidth;
        }
        public override void InnerExecute(UpdaterData data)
        {
            string thickParameter = "ADSK_Размер_Толщина";
            SharedParameterUtils.AddSharedParameter(doc, thickParameter, new BuiltInCategory[] { BuiltInCategory.OST_Parts }, group: BuiltInParameterGroup.PG_GEOMETRY);

            foreach (ElementId id in data.GetModifiedElementIds().Concat(data.GetAddedElementIds()))
            {
                try
                {
                    Part el = (Part)doc.GetElement(id);
                    el.LookupParameter(thickParameter).Set(GetThickness(el));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.StackTrace);
                    Debug.WriteLine("Element id: " + id.IntegerValue.ToString());
                }
            }

        }
    }


}
