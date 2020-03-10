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

        static public void Calculate(Room room)
        {
            if (room == null) return;
            Document doc = room.Document;
            string openingAreaParameterName = "ADSK_Площадь проемов";
            string doorOpeningWidthParameterName = "ТеррНИИ_Ширина дверных проемов";
            string openingFinishingAreaParameterName = "ТеррНИИ_Площадь проемов отделка";
            string finishingHeightParameterName = "ТеррНИИ_Высота отделки помещения";
            SharedParameterUtils.AddSharedParameter(doc, openingAreaParameterName,
                    new BuiltInCategory[] { BuiltInCategory.OST_Rooms }, BuiltInParameterGroup.PG_ANALYSIS_RESULTS);
            SharedParameterUtils.AddSharedParameter(doc, doorOpeningWidthParameterName,
                    new BuiltInCategory[] { BuiltInCategory.OST_Rooms }, BuiltInParameterGroup.PG_ANALYSIS_RESULTS);
            SharedParameterUtils.AddSharedParameter(doc, openingFinishingAreaParameterName,
                    new BuiltInCategory[] { BuiltInCategory.OST_Rooms }, BuiltInParameterGroup.PG_ANALYSIS_RESULTS);
            SharedParameterUtils.AddSharedParameter(doc, finishingHeightParameterName,
                    new BuiltInCategory[] { BuiltInCategory.OST_Rooms }, BuiltInParameterGroup.PG_CONSTRAINTS);

            if (room == null || room.Location == null || room.Area.Equals(0)) return;

            SpatialElementBoundaryOptions opt = new SpatialElementBoundaryOptions() { SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish };
            double roomHeight = room.UnboundedHeight;
            double finishingHeight = room.LookupParameter(finishingHeightParameterName).AsDouble();
            double openingArea = 0;
            double openingFinishingArea = 0;
            double doorOpeningsWidth = 0;

            List<ElementId> usedOpenings = new List<ElementId>();

            foreach (var boundary in room.GetBoundarySegments(opt))
            {
                foreach (var segment in boundary)
                {
                    if (segment.ElementId.IntegerValue == -1) continue;
                    double segLength = segment.GetCurve().Length;
                    string catName = doc.GetElement(segment.ElementId).Category.Name;
                    if (catName == "<Разделитель помещений>")
                    {
                        double area = segLength * roomHeight;
                        openingArea += area;
                        openingFinishingArea += area * finishingHeight / roomHeight;
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
                            double area = wallHeight <= roomHeight ? segLength * wallHeight : segLength * roomHeight;
                            openingArea += area;
                            openingFinishingArea += area * finishingHeight / roomHeight;
                            doorOpeningsWidth += segment.GetCurve().Length;
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
                                double area;
                                GetOpeningSize(wall, opening, out openingWidth, out openingHeight, out area);                               
                                
                                Parameter p = opening.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET) ?? opening.get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM);
                                double itemBottomOffset = p.AsDouble();
                                double areaF = 0;
                                if (finishingHeight >= itemBottomOffset + openingHeight) areaF = area;
                                else if (finishingHeight > itemBottomOffset) areaF = area * (finishingHeight - itemBottomOffset) / openingHeight;

                                openingArea += area;
                                doorOpeningsWidth += itemBottomOffset == 0 ? openingWidth : 0;
                                openingFinishingArea += areaF;

                                usedOpenings.Add(insertId);
                            }
                        }
                    }
                }
                room.LookupParameter(openingAreaParameterName).Set(openingArea);
                room.LookupParameter(openingFinishingAreaParameterName).Set(openingFinishingArea);
                room.LookupParameter(doorOpeningWidthParameterName).Set(doorOpeningsWidth);
            }
        }


        static public bool IsElementCollideRoom(Room room, Element elInsert)
        {
            BoundingBoxXYZ bb = elInsert.get_BoundingBox(null);
            BoundingBoxContainsPointFilter filterMin = new BoundingBoxContainsPointFilter(bb.Min);
            BoundingBoxContainsPointFilter filterMax = new BoundingBoxContainsPointFilter(bb.Max);
            filterMin.Tolerance = filterMax.Tolerance = 1;
            return filterMin.PassesFilter(room.Document, room.Id) || filterMax.PassesFilter(room.Document, room.Id);
        }
        static public List<Room> GetCollidedRooms(Element el)
        {
            BoundingBoxXYZ bb = el.get_BoundingBox(null);
            BoundingBoxContainsPointFilter filterMin = new BoundingBoxContainsPointFilter(bb.Min);
            BoundingBoxContainsPointFilter filterMax = new BoundingBoxContainsPointFilter(bb.Max);
            filterMin.Tolerance = filterMax.Tolerance = 1;

            ElementFilter filter = new LogicalOrFilter(filterMin, filterMax);            
            return new FilteredElementCollector(el.Document).OfCategory(BuiltInCategory.OST_Rooms).WherePasses(filter).Cast<Room>().ToList();
        }

        static private void GetOpeningSize(Wall elHost, Element elInsert, out double width, out double height, out double area)
        {
            width = 0;
            height = 0;
            area = 0;
            if (elInsert is FamilyInstance) 
            {
                FamilyInstance fi = elInsert as FamilyInstance;
                XYZ dir = new XYZ();
                CurveLoop curve = ExporterIFCUtils.GetInstanceCutoutFromWall(elHost.Document, elHost, fi, out dir);
                Curve widthCurve = curve.Where(x => x.GetEndPoint(0).Z == x.GetEndPoint(1).Z).FirstOrDefault();
                Curve heightCurve = curve.Where(x => x.GetEndPoint(0).X == x.GetEndPoint(1).X || x.GetEndPoint(0).Y == x.GetEndPoint(1).Y).FirstOrDefault();
                width = widthCurve != null ? widthCurve.Length : 0;
                height = heightCurve != null ? heightCurve.Length : 0;
                area = width * height;
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
                    area = width * height;
                }
                else
                {
                    width = l;
                    height = hHost;
                    area = width * height;
                }
            }
            else if (elInsert is Opening) 
            {
                XYZ min = (elInsert as Opening).BoundaryRect[0];
                XYZ max = (elInsert as Opening).BoundaryRect[1];

                width = min.X != max.X ? Math.Abs(max.X - min.X) : Math.Abs(max.Y - max.Y);
                height = Math.Abs(max.Z - min.Z);
                area = width * height;
            }
        }       
    }
}
