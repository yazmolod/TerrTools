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
    class FinishingData : IExternalCommand
    {
        private Document doc;
        private string openingAreaParameterName = "ADSK_Площадь проемов";
        private string doorOpeningWidthParameterName = "ТеррНИИ_Ширина дверных проемов";
        private string openingPlanAreaParameterName = "ТеррНИИ_Площадь проемов в плане";
        private string openingFinishingAreaParameterName = "ТеррНИИ_Площадь проемов отделка";
        private string finishingHeightAreaParameterName = "ТеррНИИ_Высота отделки помещения";

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
                Parameter finishingHeightParam = room.LookupParameter(finishingHeightAreaParameterName);
                if (itemBottomOffsetParam != null && finishingHeightParam != null)
                {
                    double finishingHeight = finishingHeightParam.AsDouble();
                    double itemBottomOffset = itemBottomOffsetParam.AsDouble();
                    double deltaH = finishingHeight - itemBottomOffset;
                    double deltaS = 0f;
                    if (finishingHeight > 0 && deltaH > 0 && H > 0)
                    {
                        if (finishingHeight >= itemBottomOffset + H) deltaS = S;
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
            doc = commandData.Application.ActiveUIDocument.Document;        
            SpatialElementBoundaryOptions opt = new SpatialElementBoundaryOptions();        
            List<Element> doors = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Doors).WhereElementIsNotElementType().ToList();        
            List<Element> windows = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Windows).WhereElementIsNotElementType().ToList();        
            List<Element> rooms = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rooms).WhereElementIsNotElementType().ToList();

            CheckDefaultParameters();

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
                        double finishingHeight = room.LookupParameter(finishingHeightAreaParameterName).AsDouble();
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
                        room.LookupParameter(openingAreaParameterName).Set(S);
                        room.LookupParameter(doorOpeningWidthParameterName).Set(W);
                        room.LookupParameter(openingPlanAreaParameterName).Set(D);
                        room.LookupParameter(openingFinishingAreaParameterName).Set(deltaS);
                    }
                }
                tr.Commit();
            }
            return Result.Succeeded;
        }

        private void CheckDefaultParameters()
        {
            SharedParameterUtils.AddSharedParameter(doc, openingAreaParameterName, true,
                new BuiltInCategory[] { BuiltInCategory.OST_Rooms }, BuiltInParameterGroup.PG_DATA);

            SharedParameterUtils.AddSharedParameter(doc, doorOpeningWidthParameterName, true,
                new BuiltInCategory[] { BuiltInCategory.OST_Rooms }, BuiltInParameterGroup.PG_DATA);

            SharedParameterUtils.AddSharedParameter(doc, openingPlanAreaParameterName, true,
                new BuiltInCategory[] { BuiltInCategory.OST_Rooms }, BuiltInParameterGroup.PG_DATA);

            SharedParameterUtils.AddSharedParameter(doc, openingFinishingAreaParameterName, true,
                new BuiltInCategory[] { BuiltInCategory.OST_Rooms }, BuiltInParameterGroup.PG_DATA);

            SharedParameterUtils.AddSharedParameter(doc, finishingHeightAreaParameterName, true,
                new BuiltInCategory[] { BuiltInCategory.OST_Rooms }, BuiltInParameterGroup.PG_DATA);
        }
    }
}
