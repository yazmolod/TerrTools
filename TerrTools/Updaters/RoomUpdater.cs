using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.Attributes;

namespace TerrTools.Updaters
{
    public class RoomUpdater : TerrUpdater
    {
        public RoomUpdater(string name, string guid, string info, ChangePriority priority) : base(name, guid, info, priority) { }
        public override void InnerExecute(UpdaterData data)
        {
            var modified = data.GetModifiedElementIds();
            var added = data.GetAddedElementIds();
            var deleted = data.GetDeletedElementIds();
            foreach (ElementId id in modified.Concat(added))
            {
                try
                {
                    Element el = doc.GetElement(id);
                    if (el is Room) FinishingData.Calculate(el as Room);
                    else
                    {
                        if (el != null)
                        {
                            List<Room> rooms = FinishingData.GetCollidedRooms(el);
                            foreach (Room r in rooms) FinishingData.Calculate(r);
                        }
                        else
                        {
                            foreach (Room r in new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_Rooms).Cast<Room>()) FinishingData.Calculate(r);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.StackTrace);
                    Debug.WriteLine("Addition/Modification error: id " + id.IntegerValue.ToString());
                }
            }
            if (deleted.Count > 0)
            {
                try
                {
                    foreach (Room r in new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_Rooms).Cast<Room>()) FinishingData.Calculate(r);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.StackTrace);
                    Debug.WriteLine("Deletion error");
                }
            }
        }
    }
}
