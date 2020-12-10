using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Architecture;

namespace TerrTools
{
    public class ClassSelectionFilter<T> : ISelectionFilter
    {
        bool ISelectionFilter.AllowElement(Element elem)
        {
            return elem is T;
        }

        bool ISelectionFilter.AllowReference(Reference reference, XYZ position)
        {
            throw new NotImplementedException();
        }
    }

    [Transaction(TransactionMode.Manual)]
    abstract class WallSplit : IExternalCommand
    {
        UIDocument UIDoc { get; set; }
        Document Doc { get => UIDoc.Document; }
        Wall ParentWall { get; set; }
        WallType ParentWallType { get => ParentWall.WallType; }
        Curve ParentLocationCurve { get; set; }
        public abstract MaterialFunctionAssignment OnLayerSplit { get; }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDoc = commandData.Application.ActiveUIDocument;
            Reference wallRef;
            try
            {
                wallRef = UIDoc.Selection.PickObject(ObjectType.Element, new ClassSelectionFilter<Wall>(), "Выберите стену");
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            ParentWall = Doc.GetElement(wallRef.ElementId) as Wall;
            ParentLocationCurve = (ParentWall.Location as LocationCurve).Curve;

            CompoundStructure wallStructure = ParentWallType.GetCompoundStructure();
            int bearingIndex;
            List<CompoundStructure> listOfStructures;
            SplitStructureByFunction(wallStructure, out listOfStructures, out bearingIndex);

            // обрабатываем случай когда стена развернута
            if (ParentWall.Flipped)
            {
                listOfStructures.Reverse();
                bearingIndex = listOfStructures.Count - 1 - bearingIndex;
            }

            if (bearingIndex < 0)
            {
                TaskDialog.Show("Ошибка", "В стене нет несущего слоя. Проверьте структуру стены");
                return Result.Failed;
            }

            using (Transaction tr = new Transaction(Doc, "Разбить стену по несущему слою"))
            {
                tr.Start();
                SplitWall(listOfStructures, bearingIndex);
                tr.Commit();
            }
            return Result.Succeeded;
        }

        void MergeWalls(IList<Wall> walls)
        {
            throw new NotImplementedException();
        }

        void SplitWall(List<CompoundStructure> listOfStructures, int bearingIndex)
        {
            List<Wall> splittedWalls = new List<Wall>();
            ParentWall.get_Parameter(BuiltInParameter.WALL_STRUCTURAL_SIGNIFICANT).Set(1);
            WallType bearingWallType = null;
            double bearingOffset = 0;
            for (int i = 0; i < listOfStructures.Count; i++)
            {
                string newWallTypeName = GenerateNewWallTypeName(listOfStructures[i]);
                WallType newWallType = FindWallTypeByStructure(listOfStructures[i]);
                if (newWallType == null)
                {
                    newWallType = ParentWallType.Duplicate(newWallTypeName) as WallType;
                    newWallType.SetCompoundStructure(listOfStructures[i]);
                }
                double wallOffset = CalculateLayerOffset(listOfStructures, i);
                if (i != bearingIndex)
                {
                    Wall newWall = CreateWall(newWallType);
                    MoveLayer(newWall, wallOffset);
                    splittedWalls.Add(newWall);
                }
                else
                {
                    bearingWallType = newWallType;
                    bearingOffset = wallOffset;
                    splittedWalls.Add(ParentWall);
                }
            }
            // переопределяем привязку стены, чтобы при смене типа стена не сместилась относительно оси
            ParentWall.get_Parameter(BuiltInParameter.WALL_KEY_REF_PARAM).Set((int)WallLocationLine.WallCenterline);
            ParentWall.ChangeTypeId(bearingWallType.Id);
            MoveLayer(ParentWall, bearingOffset);
            // Объединяем стены для вырезания проемов
            for (int i = 0; i < splittedWalls.Count - 1; i++)
            {
                try
                {
                    JoinGeometryUtils.JoinGeometry(Doc, splittedWalls[i], splittedWalls[i + 1]);
                }
                catch { }
            }
        }

        WallType FindWalltypeByName(string name)
        {
            List<WallType> existedWallTypes = new FilteredElementCollector(Doc).OfClass(typeof(WallType)).Cast<WallType>().ToList();
            WallType wallType = existedWallTypes.Where(x => x.Name == name).FirstOrDefault();
            return wallType;
        }

        WallType FindWallTypeByStructure(CompoundStructure structure)
        {
            List<WallType> existedWallTypes = new FilteredElementCollector(Doc).OfClass(typeof(WallType)).Cast<WallType>().ToList();
            IList<CompoundStructureLayer> layers = structure.GetLayers();
            foreach (var wallType in existedWallTypes)
            {
                CompoundStructure existedStructure = wallType.GetCompoundStructure();
                if (existedStructure != null && existedStructure.LayerCount == layers.Count)
                {
                    var existedLayers = existedStructure.GetLayers();
                    bool breaked = false;
                    for (int i = 0; i < layers.Count; i++)
                    {
                        var existedLayer = existedLayers[i];
                        var layer = layers[i];
                        if (existedLayer.Function != layer.Function)
                        {
                            breaked = true;
                            break;
                        }
                        if (existedLayer.Width != layer.Width)
                        {
                            breaked = true;
                            break;
                        }
                        if (existedLayer.MaterialId != layer.MaterialId)
                        {
                            breaked = true;
                            break;
                        }
                    }
                    if (!breaked)
                    {
                        return wallType;
                    }
                }
            }
            return null;
        }

        private string GenerateNewWallTypeName(CompoundStructure compoundStructure)
        {
            string name = "_Слои";
            foreach (CompoundStructureLayer layer in compoundStructure.GetLayers())
            {
                string func = ((int)layer.Function).ToString();
                Element material = Doc.GetElement(layer.MaterialId);
                string smaterial = material == null ? "ПоКатегории" : material.Name.Replace("ADSK_", "").Replace("ТеррНИИ_", "");
                string width = Math.Round(layer.Width * 304.8).ToString();
                string composedLayerName = string.Format("_{0}*{1}({2})", func, smaterial, width);
                name += composedLayerName;
            }
            return name;
        }

        double CalculateLayerOffset(IEnumerable<CompoundStructure> wallTypes, int currentWallTypeIndex)
        {
            // Растояние от начала стены (наружный слой) до середины стены
            double wallW = wallTypes.Sum(x => x.GetWidth()) / 2;
            // Растояние от начала стены (наружный слой) до середины нужного слоя
            double layerW = wallTypes.Take(currentWallTypeIndex).Sum(x => x.GetWidth()) + wallTypes.ElementAt(currentWallTypeIndex).GetWidth() / 2;
            return layerW - wallW;
        }

        void MoveLayer(Wall wall, double wallOffset)
        {
            XYZ translationVector = (ParentLocationCurve.GetEndPoint(1) - ParentLocationCurve.GetEndPoint(0)).CrossProduct(XYZ.BasisZ).Normalize();
            translationVector = translationVector * wallOffset;
            ElementTransformUtils.MoveElement(Doc, wall.Id, translationVector);
        }

        private Wall CreateWall(WallType newWallType)
        {
            ElementId topLevel = ParentWall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).AsElementId();
            double topOffset = ParentWall.get_Parameter(BuiltInParameter.WALL_TOP_OFFSET).AsDouble();
            double baseOffset = ParentWall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET).AsDouble();
            Wall newWall = Wall.Create(
                    Doc,
                    ParentLocationCurve,
                    newWallType.Id,
                    ParentWall.LevelId,
                    1,
                    baseOffset,
                    ParentWall.Flipped,
                    false
                    );
            newWall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(topLevel);
            newWall.get_Parameter(BuiltInParameter.WALL_TOP_OFFSET).Set(topOffset);
            return newWall;
        }

        public void SplitStructureByFunction(CompoundStructure structure, out List<CompoundStructure> splittedStructures, out int bearingIndex)
        {
            bearingIndex = -1;
            var layers = structure.GetLayers();
            List<CompoundStructureLayer> compoundStructureLayers = new List<CompoundStructureLayer>();
            splittedStructures = new List<CompoundStructure>();
            for (int i = 0; i < layers.Count; i++)
            {
                var layer = layers[i];               
                
                if (layer.Function == OnLayerSplit)
                {
                    if (compoundStructureLayers.Count > 0)
                    {
                        CompoundStructure newStructure = CompoundStructure.CreateSimpleCompoundStructure(compoundStructureLayers);
                        splittedStructures.Add(newStructure);
                    }
                    splittedStructures.Add(CompoundStructure.CreateSimpleCompoundStructure(new List<CompoundStructureLayer> { layer }));
                    compoundStructureLayers.Clear();
                }
                else
                {
                    compoundStructureLayers.Add(layer);
                }
                if (layer.Function == MaterialFunctionAssignment.Structure)
                {
                    bearingIndex = splittedStructures.Count;
                }
            }
            if (compoundStructureLayers.Count > 0)
            {
                CompoundStructure newStructure = CompoundStructure.CreateSimpleCompoundStructure(compoundStructureLayers);
                splittedStructures.Add(newStructure);
            }
        }

        public void SplitStructure(CompoundStructure structure, out List<CompoundStructure> splittedStructures, out int bearingIndex)
        {
            var layers = structure.GetLayers();
            bearingIndex = structure.StructuralMaterialIndex;
            splittedStructures = layers.Select(x => CompoundStructure.CreateSimpleCompoundStructure(new List<CompoundStructureLayer> { x })).ToList();
        }
    }

    [Transaction(TransactionMode.Manual)]
    class LayerSplit_AllLayers : WallSplit
    {
        public override MaterialFunctionAssignment OnLayerSplit { get => MaterialFunctionAssignment.None; }
    }

    [Transaction(TransactionMode.Manual)]
    class LayerSplit_ByStructure : WallSplit
    {
        public override MaterialFunctionAssignment OnLayerSplit { get => MaterialFunctionAssignment.Structure; }
    }
}
