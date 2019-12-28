using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Architecture;

namespace TerrTools
{
    [Transaction(TransactionMode.Manual)]
    class Finishing : IExternalCommand
    {
        UIApplication uiapp;
        UIDocument uidoc;
        Document doc;

        Dictionary<int, double> holesAreaDict = new Dictionary<int, double>();
        Dictionary<int, double> doorsWidthDict = new Dictionary<int, double>();
        Dictionary<int, double> doorsPlaneDict = new Dictionary<int, double>();
        Dictionary<int, double> finishingHolesAreaDict = new Dictionary<int, double>();

        List<List<string>> dimensionsOrder = new List<List<string>>
            {
            new List<string> { "Ширина", "Высота", "Экземпляр" },
            new List<string> { "Ширина", "Высота", "Тип" },
            new List<string> { "Примерная ширина", "Примерная высота", "Экземпляр" },
            new List<string> { "Примерная ширина", "Примерная высота", "Тип" }
            };

        private void GetElementSizes(Element item, out double area, out double width, out double height)
        {
            width = 0;
            height = 0;
            area = 0;
            Element itemType = doc.GetElement(item.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM).AsElementId());
            foreach (var pairs in dimensionsOrder)
            {
                Element el = pairs[2] == "Тип" ? itemType : item;
                Parameter w = el.LookupParameter(pairs[0]);
                Parameter h = el.LookupParameter(pairs[1]);
                if (w != null && h != null)
                {
                    width = w.AsDouble();
                    height = h.AsDouble();
                    area = width * height;
                }
            }
        }
        private void UpdateDicts(Room room, Element item, bool itemIsDoor)
        {
            if (room != null)
            {
                double S;
                double W;
                double H;
                GetElementSizes(item, out S, out W, out H);
                int roomId = room.Id.IntegerValue;

                if (holesAreaDict.ContainsKey(roomId)) holesAreaDict[roomId] += S;
                else holesAreaDict[roomId] = S;

                Parameter itemBottomOffsetParam = item.LookupParameter("Высота нижнего бруса");
                Parameter finishingHeightParam = room.LookupParameter("ТеррНИИ_Высота отделки помещения");
                if (itemBottomOffsetParam != null && finishingHeightParam != null)
                {
                    double finishingHeight = finishingHeightParam.AsDouble();
                    double itemBottomOffset = itemBottomOffsetParam.AsDouble();
                    double deltaH = finishingHeight - itemBottomOffset;
                    double deltaS = 0f;
                    if (finishingHeight>0 && deltaH > 0 && H > 0)
                    {
                        if (finishingHeight>= itemBottomOffset+H) deltaS = S;
                        else deltaS = S * deltaH / H;
                        if (finishingHolesAreaDict.ContainsKey(roomId)) finishingHolesAreaDict[roomId] += deltaS;
                        else finishingHolesAreaDict[roomId] = deltaS;
                    
                    }

                }

                if (itemIsDoor)
                {
                    if (doorsWidthDict.ContainsKey(roomId)) doorsWidthDict[roomId] += W;
                    else doorsWidthDict[roomId] = W;

                    double wallWidth = ((item as FamilyInstance).Host as Wall).Width;
                    double doorFloorArea = W * wallWidth / 2;
                    if (doorsPlaneDict.ContainsKey(roomId)) doorsPlaneDict[roomId] += doorFloorArea;
                    else doorsPlaneDict[roomId] = doorFloorArea;
                }
           }
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {            
            uiapp = commandData.Application;
            uidoc = uiapp.ActiveUIDocument;
            doc = uidoc.Document;

            FinishingForm inputForm = new FinishingForm();
            var input1 = inputForm.AreaParameter;
            var input2 = inputForm.WidthParameter;
            var input3 = inputForm.DoorPlaneParameter;
            var input4 = inputForm.FinishingHoleAreaParameter;

            SpatialElementBoundaryOptions opt = new SpatialElementBoundaryOptions();
            List<Element> doors = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Doors).WhereElementIsNotElementType().ToList();
            List<Element> windows = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Windows).WhereElementIsNotElementType().ToList();
            List<Element> rooms = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rooms).WhereElementIsNotElementType().ToList();

            try
            {
                if (rooms[0].LookupParameter(input1) == null || 
                    rooms[0].LookupParameter(input2) == null ||
                    rooms[0].LookupParameter(input3) == null ||
                    rooms[0].LookupParameter(input4) == null) {
                    TaskDialog.Show("Ошибка", "Не найден(ы) параметр(ы)");
                    return Result.Failed;
                };
                if (rooms[0].LookupParameter("ТеррНИИ_Высота отделки помещения") == null)
                {
                    TaskDialog.Show("Ошибка", "Отсутствует параметр \"ТеррНИИ_Высота отделки помещения\". Невозможно расчитать параметры)");
                    return Result.Failed;
                }
            }
            catch
            {
                TaskDialog.Show("Ошибка", "Не найдены помещения в проекте");
                return Result.Failed;
            }

            foreach (Element door in doors)
            {
                FamilyInstance itemFI = (FamilyInstance)door;
                Room fromRoom = itemFI.FromRoom;
                Room toRoom = itemFI.ToRoom;
                UpdateDicts(fromRoom, door, true);
                UpdateDicts(toRoom, door, true);
            }
            foreach (Element window in windows)
            {
                FamilyInstance itemFI = (FamilyInstance)window;
                Room fromRoom = itemFI.FromRoom;
                Room toRoom = itemFI.ToRoom;
                UpdateDicts(fromRoom, window, false);
                UpdateDicts(toRoom, window, false);
            }

            using (Transaction tr = new Transaction(doc, "Обновить параметры отделки"))
            { tr.Start();
                foreach (Room room in rooms)
                {
                    if (room.Area > 0)
                    {
                        int roomId = room.Id.IntegerValue;
                        double S = holesAreaDict.ContainsKey(roomId) ? holesAreaDict[roomId] : 0f;
                        double W = doorsWidthDict.ContainsKey(roomId) ? doorsWidthDict[roomId] : 0f;
                        double D = doorsPlaneDict.ContainsKey(roomId) ? doorsPlaneDict[roomId] : 0f;
                        double deltaS = finishingHolesAreaDict.ContainsKey(roomId) ? finishingHolesAreaDict[roomId] : 0f;
                        double finishingHeight = room.LookupParameter("ТеррНИИ_Высота отделки помещения").AsDouble();
                        IList<IList<BoundarySegment>> bounds = room.GetBoundarySegments(opt);
                        foreach (IList<BoundarySegment> contour in bounds)
                        {
                            foreach (BoundarySegment bound in contour)
                            {
                                Element boundElement = doc.GetElement(bound.ElementId);
                                if (boundElement != null)
                                {
                                    switch (boundElement.Category.Name)
                                    {
                                        case "Стены":
                                            Wall boundWall = boundElement as Wall;
                                            if (boundWall.CurtainGrid != null)
                                            {
                                                S += room.UnboundedHeight * bound.GetCurve().Length;
                                                deltaS += finishingHeight * bound.GetCurve().Length;
                                                W += bound.GetCurve().Length;
                                                D += bound.GetCurve().Length;
                                            }
                                            break;

                                        case "<Разделитель помещений>":
                                            S += room.UnboundedHeight * bound.GetCurve().Length;
                                            deltaS += finishingHeight * bound.GetCurve().Length;
                                            W += bound.GetCurve().Length;
                                            D += bound.GetCurve().Length;
                                            break;

                                        default:
                                            break;
                                    }
                                }
                            }
                        }
                        room.LookupParameter(input1).Set(S);
                        room.LookupParameter(input2).Set(W);
                        room.LookupParameter(input3).Set(D);
                        room.LookupParameter(input4).Set(deltaS);
                    }
                }
                tr.Commit();
            }


            return Result.Succeeded;
        }
    }
}
