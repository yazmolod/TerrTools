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
        private Document doc;
        private string openingAreaParameterName = "ADSK_Площадь проемов";
        private string doorOpeningWidthParameterName = "ТеррНИИ_Ширина дверных проемов";
        private string openingFinishingAreaParameterName = "ТеррНИИ_Площадь проемов отделка";
        private string finishingHeightParameterName = "ТеррНИИ_Высота отделки помещения";
        /*
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
                Parameter finishingHeightParam = room.LookupParameter(finishingHeightParameterName);
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
        }*/

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {         
            doc = commandData.Application.ActiveUIDocument.Document;
            CheckDefaultParameters();
            //Calculate();
            return Result.Succeeded;            
        }

        private void CheckDefaultParameters()
        {
            SharedParameterUtils.AddSharedParameter(doc, openingAreaParameterName, 
                new BuiltInCategory[] { BuiltInCategory.OST_Rooms }, BuiltInParameterGroup.PG_DATA);

            SharedParameterUtils.AddSharedParameter(doc, doorOpeningWidthParameterName, 
                new BuiltInCategory[] { BuiltInCategory.OST_Rooms }, BuiltInParameterGroup.PG_DATA);

            SharedParameterUtils.AddSharedParameter(doc, openingFinishingAreaParameterName, 
                new BuiltInCategory[] { BuiltInCategory.OST_Rooms }, BuiltInParameterGroup.PG_DATA);

            SharedParameterUtils.AddSharedParameter(doc, finishingHeightParameterName, 
                new BuiltInCategory[] { BuiltInCategory.OST_Rooms }, BuiltInParameterGroup.PG_DATA);
        }

        static public void Calculate(Room room)
        {
            Document doc = room.Document;
            string openingAreaParameterName = "ADSK_Площадь проемов";
            string doorOpeningWidthParameterName = "ТеррНИИ_Ширина дверных проемов";
            string openingFinishingAreaParameterName = "ТеррНИИ_Площадь проемов отделка";
            string finishingHeightParameterName = "ТеррНИИ_Высота отделки помещения";

            SpatialElementBoundaryOptions sebOpt = new SpatialElementBoundaryOptions { SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center };
            IEnumerable<Element> rooms = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rooms);
            SpatialElementGeometryCalculator calc = new SpatialElementGeometryCalculator(doc, sebOpt);

            if (room == null || room.Location == null || room.Area.Equals(0)) return;
            SpatialElementGeometryResults results = calc.CalculateSpatialElementGeometry(room);
            Solid roomSolid = results.GetGeometry();
            // для идентификации помещения элементов проемов стены
            ElementIntersectsSolidFilter filter = new ElementIntersectsSolidFilter(roomSolid);
            var set = new FilteredElementCollector(doc).WherePasses(filter).ToElementIds();

            double roomHeight = room.UnboundedHeight;
            double finishingHeight = room.LookupParameter(finishingHeightParameterName).AsDouble();

            double openingArea = 0;
            double openingFinishingArea = 0;
            double doorOpeningsWidth = 0;
            foreach (Face face in roomSolid.Faces)
            {
                IList<SpatialElementBoundarySubface> boundaryFaceInfo = results.GetBoundaryFaceInfo(face);
                PlanarFace pl = face as PlanarFace;
                // Разделитель помещения
                if (boundaryFaceInfo.Count == 0 && Math.Abs(pl.FaceNormal.Z) != 1)
                {
                    openingArea += face.Area;
                    doorOpeningsWidth += 0;
                    openingFinishingArea += openingArea * finishingHeight / roomHeight;
                }
                // Стена
                else
                {
                    foreach (var spatialSubFace in boundaryFaceInfo)
                    {
                        if (spatialSubFace.SubfaceType != SubfaceType.Side) continue;
                        Wall wall = doc.GetElement(spatialSubFace.SpatialBoundaryElement.HostElementId) as Wall;
                        if (wall == null) continue;
                        WallType wallType = doc.GetElement(wall.GetTypeId()) as WallType;
                        if (wallType.Kind == WallKind.Curtain) continue;
                        IEnumerable<ElementId> insertsIds = wall.FindInserts(true, false, true, true).Where(x => set.Contains(x));

                        foreach (ElementId insertId in insertsIds)
                        {
                            Element opening = doc.GetElement(insertId) as Element;
                            double openingWidth;
                            double openingHeight;
                            openingArea += GetOpeningArea(wall, opening);

                            Parameter p = opening.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET) ?? opening.get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM);
                            double itemBottomOffset = p.AsDouble();
                        }
                    }
                }
            }
            room.LookupParameter(openingAreaParameterName).Set(openingArea);
            //room.LookupParameter(openingFinishingAreaParameterName).Set(openingFinishingArea);
            //room.LookupParameter(doorOpeningWidthParameterName).Set(doorOpeningsWidth);

        }

        static private double GetOpeningArea(Wall elHost, Element elInsert)
        {
            Document doc = elHost.Document;
            double openingArea = 0;
            if (elInsert is FamilyInstance) 
            {
                FamilyInstance fi = elInsert as FamilyInstance;
                XYZ dir = new XYZ();
                CurveLoop curve = ExporterIFCUtils.GetInstanceCutoutFromWall(doc, elHost, fi, out dir);                    
                IList<CurveLoop> loop = new List<CurveLoop> { curve };
                openingArea = ExporterIFCUtils.ComputeAreaOfCurveLoops(loop);
                
            }
            else if (elInsert is Wall) 
            {
                var hIns = elInsert.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble();
                var hHost = elHost.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble();
                var l = elInsert.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble();
                openingArea = hIns < hHost ? l*hIns : l*hHost;
            }
            else if (elInsert is Opening) 
            {
                Options op = doc.Application.Create.NewGeometryOptions();
                op.IncludeNonVisibleObjects = true;
                op.DetailLevel = ViewDetailLevel.Fine;
                Solid geom = elInsert.get_Geometry(op).First() as Solid;
                openingArea = geom.Faces.Cast<PlanarFace>().Max(x => x.Area);
            }
            return openingArea;
        }
       
    }
}
