using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;

namespace TerrTools.Updaters
{
    public class MirroredInstancesUpdater : IUpdater
    {
        public static Guid Guid { get { return new Guid("a0643a35-5e9d-4569-9a29-53042c023725"); } }
        public static UpdaterId UpdaterId { get { return new UpdaterId(App.AddInId, Guid); } }

        public UpdaterId GetUpdaterId()
        {
            return UpdaterId;
        }

        public string GetUpdaterName()
        {
            return "Family Instance Mirror Updater";
        }
        public string GetAdditionalInformation()
        {
            return "Текст";
        }
        public ChangePriority GetChangePriority()
        {
            return ChangePriority.DoorsOpeningsWindows;
        }
        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();
            string paramName = "ТеррНИИ_Элемент отзеркален";
            foreach (ElementId changedElemId in data.GetModifiedElementIds())
            {
                FamilyInstance el = doc.GetElement(changedElemId) as FamilyInstance;
                if (el != null)
                {
                    BuiltInCategory cat = (BuiltInCategory)el.Category.Id.IntegerValue;
                    bool result = SharedParameterUtils.AddSharedParameter(doc, paramName,
                         new BuiltInCategory[] { cat }, BuiltInParameterGroup.PG_ANALYSIS_RESULTS, InTransaction: true);
                    int value = el.Mirrored ? 1 : 0;
                    el.LookupParameter(paramName).Set(value);
                }
            }
        }
    }

    public class SpaceUpdater : IUpdater
    {
        public static Guid Guid { get { return new Guid("b49432e1-c88d-4020-973d-1464f2d7b121"); } }
        public static UpdaterId UpdaterId { get { return new UpdaterId(App.AddInId, Guid); } }

        public UpdaterId GetUpdaterId()
        {
            return UpdaterId;
        }

        public string GetUpdaterName()
        {
            return "Space Updater";
        }
        public string GetAdditionalInformation()
        {
            return "Текст";
        }
        public ChangePriority GetChangePriority()
        {
            return ChangePriority.RoomsSpacesZones;
        }
        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();
            foreach (ElementId elemId in data.GetAddedElementIds())
            {
                Element space = doc.GetElement(elemId);
                SpaceNaming.TransferData(space);
            }
            foreach (ElementId elemId in data.GetModifiedElementIds())
            {
                Element space = doc.GetElement(elemId);
                SpaceNaming.TransferData(space);
            }
        }
    }

    public class RoomUpdater : IUpdater
    {
        public static Guid Guid { get { return new Guid("a82a5ae5-9c21-4645-b029-d3d0b67312f1"); } }
        public static UpdaterId UpdaterId { get { return new UpdaterId(App.AddInId, Guid); } }

        public UpdaterId GetUpdaterId()
        {
            return UpdaterId;
        }

        public string GetUpdaterName()
        {
            return "Room Updater";
        }
        public string GetAdditionalInformation()
        {
            return "Текст";
        }
        public ChangePriority GetChangePriority()
        {
            return ChangePriority.RoomsSpacesZones;
        }
        public void Execute(UpdaterData data)
        {
            string openingAreaParameterName = "ADSK_Площадь проемов";
            Document doc = data.GetDocument();
            SharedParameterUtils.AddSharedParameter(doc, openingAreaParameterName, 
    new BuiltInCategory[] { BuiltInCategory.OST_Rooms }, BuiltInParameterGroup.PG_DATA, InTransaction: true);
        }
    }

    public class DuctsUpdater : IUpdater
    {
        public static Guid Guid { get { return new Guid("93dc3d80-0c29-4af5-a509-c36dfd497d66"); } }
        public static UpdaterId UpdaterId { get { return new UpdaterId(App.AddInId, Guid); } }

        public UpdaterId GetUpdaterId()
        {
            return UpdaterId;
        }

        public string GetUpdaterName()
        {
            return "DuctUpdater";
        }
        public string GetAdditionalInformation()
        {
            return "Обновляет параметры толщины стенки и класса герметичности в зависимости от габаритов";
        }
        public ChangePriority GetChangePriority()
        {
            return ChangePriority.MEPAccessoriesFittingsSegmentsWires;
        }
        private string GetDuctClass(Duct el)
        {
            bool isInsul = el.get_Parameter(BuiltInParameter.RBS_REFERENCE_INSULATION_THICKNESS).AsDouble() > 0;
            string cl = isInsul ? "\"А\" с огнезащитой" : "\"B\"";
            return cl;
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

        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();
            string thickParameter = "ADSK_Толщина стенки";
            string classParameter = "ТеррНИИ_Класс герметичности";
            SharedParameterUtils.AddSharedParameter(doc, thickParameter, new BuiltInCategory[] { BuiltInCategory.OST_DuctCurves }, InTransaction: true);
            SharedParameterUtils.AddSharedParameter(doc, classParameter, new BuiltInCategory[] { BuiltInCategory.OST_DuctCurves }, InTransaction: true);
            foreach (ElementId id in data.GetModifiedElementIds().Concat(data.GetAddedElementIds()))
            {
                Duct el = (Duct)doc.GetElement(id);
                el.LookupParameter(thickParameter).Set(GetDuctThickness(el));
                el.LookupParameter(classParameter).Set(GetDuctClass(el));
            }
        }
    }


    public class DuctsAccessoryUpdater : IUpdater
    {
        public static Guid Guid { get { return new Guid("79e309d3-bd2d-4255-84b8-2133c88b695d"); } }
        public static UpdaterId UpdaterId { get { return new UpdaterId(App.AddInId, Guid); } }

        public UpdaterId GetUpdaterId()
        {
            return UpdaterId;
        }

        public string GetUpdaterName()
        {
            return "DuctsAccessoryUpdater";
        }
        public string GetAdditionalInformation()
        {
            return "Задает корректную марку для арматуры воздуховодов";
        }
        public ChangePriority GetChangePriority()
        {
            return ChangePriority.MEPAccessoriesFittingsSegmentsWires;
        }
        
        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();
            string tagParameter = "ADSK_Марка";
            //SharedParameterUtils.AddSharedParameter(doc, tagParameter, new BuiltInCategory[] { BuiltInCategory.OST_DuctAccessory }, group: BuiltInParameterGroup.PG_DATA, InTransaction: true);
            var modified = data.GetModifiedElementIds();
            var added = data.GetAddedElementIds();
            foreach (ElementId id in modified.Concat(added))
            {
                Element el = doc.GetElement(id);
                string rawSize = el.get_Parameter(BuiltInParameter.RBS_CALCULATED_SIZE).AsString();
                string size = rawSize.Split('-')[0].Replace("м", "").Replace(" ", "");
                el.LookupParameter("Марка").Set(size);
            }
        }
    }
}
