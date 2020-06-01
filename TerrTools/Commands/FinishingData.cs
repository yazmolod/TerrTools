using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Architecture;


namespace TerrTools
{
    [Transaction(TransactionMode.Manual)]
    class FinishingData : IExternalCommand
    {        
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            using (Transaction tr = new Transaction(doc, "Обновление данных отделки"))
            {
                tr.Start();
                foreach (Room room in new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rooms)) FinishingData.Calculate(room);
                tr.Commit();
            }
            return Result.Succeeded;
        }

        static public void AggregateFloors(Element el)
        {
            var p1 = el.LookupParameter("ADSK_Позиция отделки");
            var p2 = el.LookupParameter("Номер помещения (полы)");
            if (p1 != null && p2 != null)
            {
                var rooms = new FilteredElementCollector(el.Document).OfCategory(BuiltInCategory.OST_Rooms)
                    .Where(x => x.LookupParameter("ADSK_Позиция отделки").AsString() == p1.AsString()).Cast<Room>();
                var floors = rooms.Select(x => x.Number).ToList();
                floors.Sort();
                string floorsStr = string.Join(", ", floors);
                foreach (var r in rooms) r.LookupParameter("Номер помещения (полы)").Set(floorsStr);
            }
        }


        static public void Calculate(Room room)
        {
            if (room == null) return;
            Document doc = room.Document;
            string openingAreaParameterName = "ADSK_Площадь проемов";
            string doorOpeningWidthParameterName = "ТеррНИИ_Ширина дверных проемов";
            string openingFinishingAreaParameterName = "ТеррНИИ_Площадь проемов отделка";
            string finishingHeightParameterName = "ТеррНИИ_Высота отделки помещения";
            string finishingPerimeterParameterName = "ТеррНИИ_Периметр отделки";
            string slopeAreaParameterName = "ТеррНИИ_Площадь откосов";
            string doorOpeningPlaneAreaParameterName = "ТеррНИИ_Площадь проемов в плане";
            SharedParameterUtils.AddSharedParameter(doc, openingAreaParameterName,
                    new BuiltInCategory[] { BuiltInCategory.OST_Rooms }, BuiltInParameterGroup.PG_ANALYSIS_RESULTS);
            SharedParameterUtils.AddSharedParameter(doc, doorOpeningWidthParameterName,
                    new BuiltInCategory[] { BuiltInCategory.OST_Rooms }, BuiltInParameterGroup.PG_ANALYSIS_RESULTS);
            SharedParameterUtils.AddSharedParameter(doc, openingFinishingAreaParameterName,
                    new BuiltInCategory[] { BuiltInCategory.OST_Rooms }, BuiltInParameterGroup.PG_ANALYSIS_RESULTS);
            SharedParameterUtils.AddSharedParameter(doc, finishingPerimeterParameterName,
                    new BuiltInCategory[] { BuiltInCategory.OST_Rooms }, BuiltInParameterGroup.PG_ANALYSIS_RESULTS);
            SharedParameterUtils.AddSharedParameter(doc, slopeAreaParameterName,
                    new BuiltInCategory[] { BuiltInCategory.OST_Rooms }, BuiltInParameterGroup.PG_ANALYSIS_RESULTS);
            SharedParameterUtils.AddSharedParameter(doc, doorOpeningPlaneAreaParameterName,
                    new BuiltInCategory[] { BuiltInCategory.OST_Rooms }, BuiltInParameterGroup.PG_ANALYSIS_RESULTS);
            SharedParameterUtils.AddSharedParameter(doc, finishingHeightParameterName,
                    new BuiltInCategory[] { BuiltInCategory.OST_Rooms }, BuiltInParameterGroup.PG_CONSTRAINTS);

            if (room == null || room.Location == null || room.Area.Equals(0)) return;

            SpatialElementBoundaryOptions opt = new SpatialElementBoundaryOptions() { SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish };
            double roomHeight = room.UnboundedHeight;
            double finishingHeight = room.LookupParameter(finishingHeightParameterName).AsDouble();
            double allOpeningsArea = 0;
            double allOpeningsFinishingArea = 0;
            double doorOpeningsWidth = 0;
            double finishingPerimeter = 0;
            double allOpeningsSlope = 0;
            double doorOpeningPlaneArea = 0;

            List<ElementId> usedOpenings = new List<ElementId>();

            foreach (var boundary in room.GetBoundarySegments(opt))
            {
                foreach (var segment in boundary)
                {
                    finishingPerimeter += segment.GetCurve().Length;
                    if (segment.ElementId.IntegerValue == -1) continue;
                    double segLength = segment.GetCurve().Length;
                    string catName = doc.GetElement(segment.ElementId).Category.Name;
                    if (catName == "<Разделитель помещений>")
                    {
                        double area = segLength * roomHeight;
                        allOpeningsArea += area;
                        allOpeningsFinishingArea += area * finishingHeight / roomHeight;
                        doorOpeningsWidth += segLength;
                    }
                    else if (catName == "Стены")
                    {
                        Wall wall = doc.GetElement(segment.ElementId) as Wall;
                        if (wall == null) continue;
                        WallType wallType = doc.GetElement(wall.GetTypeId()) as WallType;
                        if (wallType.Kind == WallKind.Curtain)
                        {
                            double wallHeight = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble();
                            double curtainRoomHeight = wallHeight <= roomHeight ? wallHeight : roomHeight;
                            double area = segLength * curtainRoomHeight;
                            allOpeningsArea += area;
                            allOpeningsFinishingArea += area * finishingHeight / roomHeight;
                            doorOpeningsWidth += segment.GetCurve().Length;
                            allOpeningsSlope = (segLength + 2 * curtainRoomHeight) * wall.Width / 2;
                        }
                        else
                        {
                            IEnumerable<ElementId> insertsIds = wall.FindInserts(true, false, true, true).Where(x => IsElementCollideRoom(room, doc.GetElement(x)));

                            foreach (ElementId insertId in insertsIds)
                            {
                                if (usedOpenings.Contains(insertId)) continue;

                                Element opening = doc.GetElement(insertId) as Element;
                                double openingWidth;
                                double openingHeight;
                                double openingThickness;
                                GetOpeningSize(wall, opening, out openingWidth, out openingHeight, out openingThickness);

                                // Проверяем, чтобы ширина отверстия не была больше самой комнаты
                                openingWidth = openingWidth > segLength ? segLength : openingWidth;

                                double openingArea = openingWidth * openingHeight;
                                double openingSlope = (openingWidth + openingHeight * 2) * openingThickness / 2;
                                
                                Parameter p = opening.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET) ?? opening.get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM);
                                double itemBottomOffset = p.AsDouble();
                                double roomBottomOffset = room.get_Parameter(BuiltInParameter.ROOM_LOWER_OFFSET).AsDouble();
                                double openingFinishingArea = 0;
                                if (finishingHeight + roomBottomOffset >= itemBottomOffset + openingHeight) openingFinishingArea = openingArea;
                                else if (finishingHeight + roomBottomOffset > itemBottomOffset) openingFinishingArea = openingArea * (finishingHeight + roomBottomOffset - itemBottomOffset) / openingHeight;
                                                                
                                allOpeningsArea += openingArea;                                
                                allOpeningsFinishingArea += openingFinishingArea;
                                allOpeningsSlope += openingSlope;
                                if (itemBottomOffset == roomBottomOffset)
                                {
                                    doorOpeningsWidth += openingWidth;
                                    doorOpeningPlaneArea += openingWidth * wall.Width / 2;
                                }
                                usedOpenings.Add(insertId);
                            }
                        }
                    }
                }                
            }
            room.LookupParameter(openingAreaParameterName).Set(allOpeningsArea);
            room.LookupParameter(openingFinishingAreaParameterName).Set(allOpeningsFinishingArea);
            room.LookupParameter(doorOpeningWidthParameterName).Set(doorOpeningsWidth);
            room.LookupParameter(finishingPerimeterParameterName).Set(finishingPerimeter);
            room.LookupParameter(slopeAreaParameterName).Set(allOpeningsSlope);
            room.LookupParameter(doorOpeningPlaneAreaParameterName).Set(doorOpeningPlaneArea);
        }
        static public bool IsElementCollideRoom(Room room, Element elInsert)
        {
            // Проверка параметров Room у FamilyInstance
            FamilyInstance fi = elInsert as FamilyInstance;
            if (fi != null && (fi.Room?.Id == room.Id || fi.ToRoom?.Id == room.Id || fi.FromRoom?.Id == room.Id)) return true;

            // Проверка bbox
            BoundingBoxXYZ bb = elInsert.get_BoundingBox(null);
            BoundingBoxContainsPointFilter filterMin = new BoundingBoxContainsPointFilter(bb.Min);
            BoundingBoxContainsPointFilter filterMax = new BoundingBoxContainsPointFilter(bb.Max);
            filterMin.Tolerance = filterMax.Tolerance = 1;
            bool inter = filterMin.PassesFilter(room.Document, room.Id) || filterMax.PassesFilter(room.Document, room.Id);
            return inter;
        }
        static public List<Room> GetCollidedRooms(Element el)
        {
            // Проверка параметров Room у FamilyInstance
            FamilyInstance fi = el as FamilyInstance;
            if (fi != null)
            {
                Room[] rooms = new Room[] { fi?.Room, fi?.FromRoom, fi?.ToRoom };
                if (rooms.Any(x => x != null)) return rooms.Where(x => x != null).ToList();
            }

            BoundingBoxXYZ bb = el.get_BoundingBox(null);
            BoundingBoxContainsPointFilter filterMin = new BoundingBoxContainsPointFilter(bb.Min);
            BoundingBoxContainsPointFilter filterMax = new BoundingBoxContainsPointFilter(bb.Max);
            filterMin.Tolerance = filterMax.Tolerance = 1;

            ElementFilter filter = new LogicalOrFilter(filterMin, filterMax);            
            return new FilteredElementCollector(el.Document).OfCategory(BuiltInCategory.OST_Rooms).WherePasses(filter).Cast<Room>().ToList();
        }

        static private void GetOpeningSize(Wall elHost, Element elInsert, out double width, out double height, out double thickness)
        {
            width = 0;
            height = 0;
            thickness = elHost.Width;
            if (elInsert is FamilyInstance) 
            {
                FamilyInstance fi = elInsert as FamilyInstance;
                XYZ dir = new XYZ();
                try
                {
                    CurveLoop curve = ExporterIFCUtils.GetInstanceCutoutFromWall(elHost.Document, elHost, fi, out dir);
                    Curve widthCurve = curve.Where(x => x.GetEndPoint(0).Z == x.GetEndPoint(1).Z).FirstOrDefault();
                    Curve heightCurve = curve.Where(x => x.GetEndPoint(0).X == x.GetEndPoint(1).X && x.GetEndPoint(0).Z != x.GetEndPoint(1).Z).FirstOrDefault();
                    width = widthCurve != null ? widthCurve.Length : 0;
                    height = heightCurve != null ? heightCurve.Length : 0;
                }
                catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                {
                    Parameter wp = fi.LookupParameter("Ширина");
                    Parameter wpp = fi.LookupParameter("Примерная ширина");
                    Parameter hp = fi.LookupParameter("Высота");
                    Parameter hpp = fi.LookupParameter("Примерная высота");

                    if (wp != null) width = wp.AsDouble();
                    else if (wpp != null) width = wpp.AsDouble();
                    else width = 0;

                    if (hp != null) height = hp.AsDouble();
                    else if (hpp != null) height = hpp.AsDouble();
                    else height = 0;
                }
            }
            else if (elInsert is Wall) 
            {
                var hIns = elInsert.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble();
                var hHost = elHost.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble();
                var l = elInsert.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble();
                
                if (hIns < hHost)
                {
                    width = l;
                    height = hIns;
                }
                else
                {
                    width = l;
                    height = hHost;
                }
            }
            else if (elInsert is Opening) 
            {
                XYZ min = (elInsert as Opening).BoundaryRect[0];
                XYZ max = (elInsert as Opening).BoundaryRect[1];

                width = min.X != max.X ? Math.Abs(max.X - min.X) : Math.Abs(max.Y - min.Y);
                height = Math.Abs(max.Z - min.Z);
            }
        }       
    }
}
