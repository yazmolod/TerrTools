using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

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
                        true, new BuiltInCategory[] { cat }, BuiltInParameterGroup.PG_ANALYSIS_RESULTS, true);
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
            SharedParameterUtils.AddSharedParameter(doc, openingAreaParameterName, true,
    new BuiltInCategory[] { BuiltInCategory.OST_Rooms }, BuiltInParameterGroup.PG_DATA, true);        }

    }
}
