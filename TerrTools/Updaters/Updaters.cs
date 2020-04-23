﻿using System;
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
        public abstract string Name { get; }
        public abstract string Info { get; }
        public abstract string Guid { get; }
        public abstract ChangePriority Priority { get; }
        private bool SelfInvoked { get; set; }
        
        protected Document doc;
        public bool IsActive
        {
            get
            {
                return UpdaterRegistry.IsUpdaterEnabled(GetUpdaterId());
            }
            set
            {
                if (value) UpdaterRegistry.EnableUpdater(GetUpdaterId());
                else UpdaterRegistry.DisableUpdater(GetUpdaterId());
            }
        }

        public ElementFilter Filter { get; set; }
        public ChangeType ChangeType { get; set; }

        public TerrUpdater(ElementFilter filter, ChangeType chtype)
        {
            Filter = filter;
            ChangeType = chtype;
            SelfInvoked = false;
        }
        public abstract void InnerExecute(UpdaterData data);
        public abstract void GlobalExecute(Document doc);

        public void GlobalUpdate(Document doc)
        {
            using (Transaction tr = new Transaction(doc, Name))
            {
                tr.Start();
                GlobalExecute(doc);                
                tr.Commit();
                SelfInvoked = true;
                TaskDialog.Show("Отчет апдейтера", "Обновление прошло успешно");
            }            
        }

        public void Execute(UpdaterData data)
        {
            if (IsActive && !SelfInvoked)
            {
                try
                {
                    doc = data.GetDocument();
                    InnerExecute(data);
                }
                catch (Exception e)
                {
                    var td = new TaskDialog("Ошибка");
                    td.MainInstruction = "В апдейтере " + Name + " возникла ошибка. Рекомендуется отключить его в наcтройках и сообщить об ошибке BIM-менеджеру";
                    td.MainContent = e.ToString();
                    td.Show();
                }
            }
            SelfInvoked = false;
        }       

        public string GetAdditionalInformation()
        {
            return Info;
        }

        public ChangePriority GetChangePriority()
        {
            return Priority;
        }

        public UpdaterId GetUpdaterId()
        {
            return new UpdaterId(App.AddInId, new Guid(Guid));
        }

        public string GetUpdaterName()
        {
            return Name;
        }
    }

    public class SpaceUpdater : TerrUpdater
    {
        public override string Name => "SpaceUpdater";
        public override string Info => "";
        public override string Guid => "b49432e1-c88d-4020-973d-1464f2d7b121";
        public override ChangePriority Priority => ChangePriority.RoomsSpacesZones;

        public SpaceUpdater
            (ElementFilter filter, ChangeType chtype)
            : base(filter, chtype) { }

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

        public override void GlobalExecute(Document doc)
        {
            throw new NotImplementedException();
        }
    }
        
    public class DuctsUpdater : TerrUpdater
    {
        public override string Name => "DuctsUpdater";
        public override string Info => "";
        public override string Guid => "93dc3d80-0c29-4af5-a509-c36dfd497d66";
        public override ChangePriority Priority => ChangePriority.MEPAccessoriesFittingsSegmentsWires;

        public DuctsUpdater
            (ElementFilter filter, ChangeType chtype)
            : base(filter, chtype) { }
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

        public override void GlobalExecute(Document doc)
        {
            throw new NotImplementedException();
        }
    }

    public class DuctsAccessoryUpdater : TerrUpdater
    {
        public override string Name => "DuctsAccessoryUpdater";
        public override string Info => "";
        public override string Guid => "79e309d3-bd2d-4255-84b8-2133c88b695d";
        public override ChangePriority Priority => ChangePriority.MEPAccessoriesFittingsSegmentsWires;
        public DuctsAccessoryUpdater
            (ElementFilter filter, ChangeType chtype)
            : base(filter, chtype) { }

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

        public override void GlobalExecute(Document doc)
        {
            throw new NotImplementedException();
        }
    }

    public class PartUpdater : TerrUpdater
    {
        public override string Name => "PartUpdater";
        public override string Info => "";
        public override string Guid => "79ef66cc-2d1a-4bdd-9bae-dae5aa8501f0";
        public override ChangePriority Priority => ChangePriority.FloorsRoofsStructuralWalls;
        public PartUpdater
            (ElementFilter filter, ChangeType chtype)
            : base(filter, chtype) { }
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

        public override void GlobalExecute(Document doc)
        {
            foreach (Part part in new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Parts))
            {
                part.LookupParameter("ADSK_Размер_Толщина").Set(GetThickness(part));
            }
        }
    }


}