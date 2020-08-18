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
using System.Security.Policy;

namespace TerrTools.Updaters
{
    public class TriggerPair
    {
        public ElementFilter Filter { get; set; }
        public ChangeType ChangeType { get; set; }
        public TriggerPair(ElementFilter f, ChangeType c)
        {
            Filter = f;
            ChangeType = c;
        }
    }
    public class SharedParameterSettings
    {
        public BuiltInCategory[] Categories { get; set; }
        public string ParameterName { get; set; }
        public BuiltInParameterGroup ParameterGroup { get; set; }
        public bool IsInstance { get; set; }
        public SharedParameterSettings(BuiltInCategory c, string p)
        {
            Categories = new BuiltInCategory[]{ c };
            ParameterName = p;
            ParameterGroup = BuiltInParameterGroup.PG_ADSK_MODEL_PROPERTIES;
            IsInstance = true;
        }
        public SharedParameterSettings(BuiltInCategory[] c, string p)
        {
            Categories = c;
            ParameterName = p;
            ParameterGroup = BuiltInParameterGroup.PG_ADSK_MODEL_PROPERTIES;
            IsInstance = true;
        }
        public SharedParameterSettings(BuiltInCategory[] c, string p, BuiltInParameterGroup g)
        {
            Categories = c;
            ParameterName = p;
            ParameterGroup = g;
            IsInstance = true;
        }
        public SharedParameterSettings(BuiltInCategory[] c, string p, bool inst)
        {
            Categories = c;
            ParameterName = p;
            ParameterGroup = BuiltInParameterGroup.PG_ADSK_MODEL_PROPERTIES;
            IsInstance = inst;
        }
        public SharedParameterSettings(BuiltInCategory[] c, string p, BuiltInParameterGroup g, bool inst)
        {
            Categories = c;
            ParameterName = p;
            ParameterGroup = g;
            IsInstance = inst;
        }
    }

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

        public List<TriggerPair> TriggerPairs { get; set; }
        public List<SharedParameterSettings> SharedParameterSettings { get; set; }

        public TerrUpdater(ElementFilter filter, ChangeType chtype)
        {
            TriggerPairs = new List<TriggerPair>() { new TriggerPair(filter, chtype) };
            SharedParameterSettings = new List<SharedParameterSettings>();
            SelfInvoked = false;
        }
        public abstract void InnerExecute(UpdaterData data);
        public abstract void GlobalExecute(Document doc);

        public void AddTriggerPair(ElementFilter filter, ChangeType chtype)
        {
            TriggerPairs.Add(new TriggerPair(filter, chtype));
        }
        public void AddSharedSettings(SharedParameterSettings item)
        {
            SharedParameterSettings.Add(item);
        }

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
        public override string Info => "Сопоставляет номер помещения из связанного проекта с номером пространства в текущем проекте";
        public override string Guid => "b49432e1-c88d-4020-973d-1464f2d7b121";
        public override ChangePriority Priority => ChangePriority.RoomsSpacesZones;

        public SpaceUpdater
            (ElementFilter filter, ChangeType chtype)
            : base(filter, chtype) { }

        public override void InnerExecute(UpdaterData data)
        {            
            var modified = data.GetModifiedElementIds();
            var added = data.GetAddedElementIds();
            var elements = added.Concat(modified).Select(x => doc.GetElement(x));
            foreach (Element e in elements) SpaceNaming.TransferData(e);         
        }

        public override void GlobalExecute(Document doc)
        {
            var filter = new ElementCategoryFilter(BuiltInCategory.OST_MEPSpaces);
            var elements = new FilteredElementCollector(doc).WherePasses(filter).ToElements();
            foreach (var e in elements) SpaceNaming.TransferData(e);
        }
    }
        
    public class DuctsUpdater : TerrUpdater
    {
        public override string Name => "DuctsUpdater";
        public override string Info => "Назначает воздуховодам в соответствующие параметры толщину стенки, категорию защиты и отметку от нуля";
        public override string Guid => "93dc3d80-0c29-4af5-a509-c36dfd497d66";
        public override ChangePriority Priority => ChangePriority.MEPAccessoriesFittingsSegmentsWires;

        public DuctsUpdater
            (ElementFilter filter, ChangeType chtype)
            : base(filter, chtype) { }
        private string GetDuctClass(Duct el)
        {
            bool isInsul = el.get_Parameter(BuiltInParameter.RBS_REFERENCE_INSULATION_THICKNESS).AsDouble() > 0;
            /* 23.06.2020 Попросили убрать логику с поиском огнезащиты, теперь просто маркируем все с изоляцией
             * 
            string insulType = el.get_Parameter(BuiltInParameter.RBS_REFERENCE_INSULATION_TYPE).AsString();
            Element[] insulTypes = new FilteredElementCollector(doc).OfClass(typeof(DuctInsulationType)).Where(x => x.Name == insulType).ToArray();
            // Параметр "Комментарий к типоразмеру"
            string comment = insulTypes.Length > 0 ? insulTypes[0].get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS).AsString() : "";
            if (insulTypes.Length > 0 && comment?.IndexOf("огнезащит", StringComparison.OrdinalIgnoreCase) >= 0) return "B с огнезащитой";
            */
            if (isInsul) return "B в изоляции";
            else return "А";
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

        string thickParameter = "ADSK_Толщина стенки";
        string classParameter = "ТеррНИИ_Класс герметичности";
        string levelParameter = "ТеррНИИ_Отметка от нуля";
        public override void InnerExecute(UpdaterData data)
        {            
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
            foreach (Duct el in new FilteredElementCollector(doc).WherePasses(TriggerPairs[0].Filter).ToElements())
            {
                el.LookupParameter(thickParameter).Set(GetDuctThickness(el));
                el.LookupParameter(classParameter).Set(GetDuctClass(el));
                el.LookupParameter(levelParameter).Set(GetLevelHeight(el));
            }
        }
    }

    public class DuctsAccessoryUpdater : TerrUpdater
    {
        public override string Name => "DuctsAccessoryUpdater";
        public override string Info => "Арматура воздуховодов: копирует размеры коннектора в марку";
        public override string Guid => "79e309d3-bd2d-4255-84b8-2133c88b695d";
        public override ChangePriority Priority => ChangePriority.MEPAccessoriesFittingsSegmentsWires;
        public DuctsAccessoryUpdater
            (ElementFilter filter, ChangeType chtype)
            : base(filter, chtype) { }

        public override void InnerExecute(UpdaterData data)
        {
            var modified = data.GetModifiedElementIds();
            var added = data.GetAddedElementIds();
            foreach (ElementId id in modified.Concat(added)) UpdateMark(id);            
        }
        
        private void UpdateMark(ElementId id)
        {
            Element el = doc.GetElement(id);
            string rawSize = el.get_Parameter(BuiltInParameter.RBS_CALCULATED_SIZE).AsString();
            string size = rawSize.Split('-')[0].Replace("м", "").Replace(" ", "");
            el.LookupParameter("Марка").Set(size);
        }

        public override void GlobalExecute(Document doc)
        {
            var filter = new LogicalAndFilter(new ElementCategoryFilter(BuiltInCategory.OST_DuctAccessory), new ElementIsElementTypeFilter(true));
            var elementids = new FilteredElementCollector(doc).WherePasses(filter).ToElementIds();
            foreach (var id in elementids) UpdateMark(id);
        }
    }

    public class PartUpdater : TerrUpdater
    {
        public override string Name => "PartUpdater";
        public override string Info => "Части стен: добавляет параметр с реальной толщиной";
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

    public class SystemNamingUpdater : TerrUpdater
    {
        /// Переменная для предотвращения рекурсии. 
        /// Обновляется на false внутри апдейтера, на true при DocumentChangedEvent (см. Application.cs)
        static public bool FirstExecutionInTransaction = true;
        string systemNameP = "ТеррНИИ_Наименование системы";
        private Element[] LastUpdatedElements = new Element[0];
        private int repeatCounter = 0;

        // Порядок был объявлен при инициализации апдейтера в Application.cs
        ElementFilter elemFilter { get => this.TriggerPairs[0].Filter; }         
        ElementFilter sysFilter { get => this.TriggerPairs[1].Filter; }
        BuiltInCategory[] elemCats { get; set; }
        BuiltInCategory[] sysCats { get; set; }

        public SystemNamingUpdater
        (ElementFilter filter, ChangeType chtype, BuiltInCategory[] ecats, BuiltInCategory[] scats) : base(filter, chtype) 
        {
            elemCats = ecats;
            sysCats = scats;
        }
        public override string Name => "SystemNamingUpdater";

        public override string Info => "Позволяет работать с названиями систем";

        public override string Guid => "66f6e035-e2d7-4c81-a253-c6b3efe38d9d";

        public override ChangePriority Priority =>  ChangePriority.MEPSystems;

        public override void GlobalExecute(Document doc)
        {
            var systems = new FilteredElementCollector(doc).WherePasses(sysFilter).ToArray();
            var items = new FilteredElementCollector(doc).WherePasses(elemFilter).ToArray();
            foreach (var s in systems) UpdateSystem(s);
            foreach (var i in items) UpdateItemSystemName(i);
        }

        private void UpdateSystem(Element system)
        {
            string localSystem = system.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM).AsString();
            string globalSystem = system.LookupParameter(systemNameP).AsString();
            globalSystem = globalSystem ?? "";
            var collector = new FilteredElementCollector(doc).WherePasses(elemFilter).ToElements();
            foreach (var item in collector)
            {
                Parameter itemLocalP = item.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM);
                string itemLocal = itemLocalP != null ? itemLocalP.AsString() : null;
                if (itemLocal != null && itemLocal.Split(',').Contains(localSystem))
                {
                    try
                    {
                        item.LookupParameter(systemNameP).Set(globalSystem);
                    }
                    catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                    {
                        //catch readonly family parameter 
                    }
                }
            }
        }

        private void UpdateItemSystemName(Element item)
        {
            Parameter p = item.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM);
            string value = p.AsString();
            HashSet<string> localSystems = new HashSet<string>();
            if (value != null)
            {
                string[] values = value.Split(',');
                foreach (string v in values) localSystems.Add(v);
            }
           
            Element[] allSystems = new FilteredElementCollector(doc).WherePasses(sysFilter).ToArray();
            string[] currentAllSystem = allSystems.Where(x => localSystems.Contains(x.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM).AsString()))
                      .Select(x => x.LookupParameter(systemNameP).AsString()).ToArray();
            HashSet<string> globalSystems = new HashSet<string>(currentAllSystem);
            try
            {
                item.LookupParameter(systemNameP).Set(string.Join(",", globalSystems));
            }
            catch(Autodesk.Revit.Exceptions.InvalidOperationException)
            { 
                //catch readonly family parameter 
            }
        }

        private bool NewArrayIsTheSame(Element[] elements)
        {
            try
            {
                HashSet<int> lastIds = new HashSet<int>(LastUpdatedElements.Select(x => x.Id.IntegerValue));
                HashSet<int> newIds = new HashSet<int>(elements.Select(x => x.Id.IntegerValue));
                lastIds.SymmetricExceptWith(newIds);
                return lastIds.Count == 0;
            }
            catch (Autodesk.Revit.Exceptions.InvalidObjectException)
            {
                return false;
            }
        }

        public override void InnerExecute(UpdaterData data)
        {
            if (FirstExecutionInTransaction)
            {
                ElementId[] addedIds = data.GetAddedElementIds().ToArray();
                Element[] addedElements = addedIds.Select(x => doc.GetElement(x)).ToArray();
                Element[] addedSystems = addedElements.Where(x => sysFilter.PassesFilter(x)).ToArray();
                Element[] addedItems = addedElements.Where(x => elemFilter.PassesFilter(x)).ToArray();

                ElementId[] modifiedIds = data.GetModifiedElementIds().ToArray();
                Element[] modifiedElements = modifiedIds.Select(x => doc.GetElement(x)).ToArray();
                Element[] modifiedSystems = modifiedElements.Where(x => sysFilter.PassesFilter(x)).ToArray();
                Element[] modifiedItems = modifiedElements.Where(x => elemFilter.PassesFilter(x)).ToArray();

                // обновление элемента в MEP влечет за собой каскадное обновление элементов в системе
                // поэтому в целях оптимизации разбиваем на конкретные ситуации
                if (addedItems.Length > 0)
                {
                    foreach (var el in addedItems) UpdateItemSystemName(el);
                    LastUpdatedElements = addedItems;
                }
                else if (addedSystems.Length > 0)
                {
                    foreach (var el in addedSystems) UpdateSystem(el);
                    LastUpdatedElements = addedSystems;
                }
                else if (modifiedItems.Length > 0)
                {
                    foreach (var el in modifiedItems) UpdateItemSystemName(el);
                    LastUpdatedElements = modifiedItems;
                }
                else if (modifiedSystems.Length > 0)
                {
                    foreach (var el in modifiedSystems) UpdateSystem(el);
                    LastUpdatedElements = modifiedSystems;
                }
                FirstExecutionInTransaction = false;
            }
        }
    }
}
