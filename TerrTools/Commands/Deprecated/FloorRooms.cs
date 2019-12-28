using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace TerrTools
{
    [Transaction(TransactionMode.Manual)]
    class FloorRooms : IExternalCommand
        /*
         * Данная команда обновляет переченб помещений
         * в ключевой спецификации отделки пола от разработчиков
         * шаблона ADSK.
         * На 03.12.2019 данная функция не используется, т.к. 
         * расчет отделки пола производится путем генерации
         * отдельных элементов в каждом помещении
         * (смотри файл FloorFinishing.cs)
         * 
         */
    {
        UIApplication uiapp;
        UIDocument uidoc;
        Document doc;

        private Dictionary<string, string> GetRoomsByFloor()
        {
            List<Element> rooms = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rooms).ToList();
            Dictionary<string, List<string>> floorDict = new Dictionary<string, List<string>>();
            foreach (Room room in rooms)
            {
                string floor_type = room.LookupParameter("Отделка пола").AsString();
                string room_number = room.LookupParameter("Номер").AsString();
                if (floorDict.ContainsKey(floor_type)) floorDict[floor_type].Add(room_number);
                else floorDict.Add(floor_type, new List<string> { room_number });
            }
            Dictionary<string, string> floorDict_withStrings = new Dictionary<string, string>();
            foreach (KeyValuePair<string, List<string>> pair in floorDict){
                string concatString = pair.Value.OrderBy(q => q).Aggregate((i, j) => i + ", " + j);
                floorDict_withStrings.Add(pair.Key, concatString);
            }
            return floorDict_withStrings;
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uiapp = commandData.Application;
            uidoc = uiapp.ActiveUIDocument;
            doc = uidoc.Document;

            FloorRoomsForm form = new FloorRoomsForm(doc);
            ElementId KeyScheduleId = form.ScheduleId;
            if (KeyScheduleId != null && doc.GetElement(KeyScheduleId) == null)
            {
                TaskDialog.Show("Ошибка", "Не найдена спецификация");
                return Result.Failed;
            }

            Dictionary<string, string> floorRooms = GetRoomsByFloor();
            List<Element> items = new FilteredElementCollector(doc, KeyScheduleId).ToList();

            using (Transaction tr = new Transaction(doc, "Обновить параметры пола"))
            {
                tr.Start();
                bool doneFlag = false;
                foreach (Element item in items)
                {
                    Parameter floorParameter = item.LookupParameter("Отделка пола");
                    if (floorParameter != null)
                    {
                        doneFlag = true;
                        string floorType = floorParameter.AsString();                            
                        string value = "";
                        floorRooms.TryGetValue(floorType, out value);
                        try { item.LookupParameter("Номер помещения (полы)").Set(value); }
                        catch {
                            TaskDialog.Show("Ошибка", "Непредвиденная ошибка");
                            return Result.Failed;
                        }

                    }
                }

                tr.Commit();
                if (doneFlag) TaskDialog.Show("Результат", "Параметр успешно обновлен");
                else TaskDialog.Show("Результат", "Параметр не обновлен. Попробуйте другую спецификацию");
                return Result.Succeeded;
            }
        }
    }
}
