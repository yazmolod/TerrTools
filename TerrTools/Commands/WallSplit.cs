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
        protected UIDocument UIDoc { get; set; }
        protected Document Doc { get => UIDoc.Document; }
        /// <summary>
        /// Стена, разбиваемая на слои
        /// </summary>
        protected Wall ParentWall { get; set; }
        protected WallType ParentWallType { get => ParentWall.WallType; }
        protected Curve ParentLocationCurve { get => (ParentWall.Location as LocationCurve).Curve; }
        /// <summary>
        /// Метод разбивает исходную структуру на несколько структур
        /// </summary>
        /// <param name="structure">Изначальная структура для разбиения</param>
        /// <param name="splittedStructures">Выходной лист со разбитыми структурами</param>
        /// <param name="bearingIndex">Индекс несущего слоя в выходном листе. Структура с этим индексом будет превращена в несущую стену со скопированными проемами</param>
        public abstract void SplitStructure(CompoundStructure structure, out List<CompoundStructure> splittedStructures, out int bearingIndex);
        public virtual Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // инициализация   
            UIDoc = commandData.Application.ActiveUIDocument;
            List<Wall> walls = null;
            try
            {
                walls = SelectionHandler();
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            using (Transaction tr = new Transaction(Doc, "Расслоить стены"))
            {
                tr.Start();
                ManyWallsSplit(walls);
                tr.Commit();
            }

            return Result.Succeeded;
        }

        void ManyWallsSplit(List<Wall> walls)
        {
            foreach (Wall wall in walls)
            {
                ParentWall = wall;

                // получаем разбитые слои
                CompoundStructure wallStructure = ParentWallType.GetCompoundStructure();
                int bearingIndex;
                List<CompoundStructure> listOfStructures;
                SplitStructure(wallStructure, out listOfStructures, out bearingIndex);

                if (bearingIndex < 0)
                {
                    TaskDialog.Show("Ошибка", "В стене id " + wall.Id.IntegerValue.ToString() + " нет несущего слоя. Проверьте структуру стены");
                    continue;
                }

                // обрабатываем случай когда стена развернута
                if (ParentWall.Flipped)
                {
                    listOfStructures.Reverse();
                    bearingIndex = listOfStructures.Count - 1 - bearingIndex;
                }

                SplitWall(listOfStructures, bearingIndex);
            }
        }

        public List<Wall> SelectionHandler()
        {
            List<Wall> walls;
            var selected = UIDoc.Selection.GetElementIds();
            if (selected.Count == 0)
            {
                List<Reference> wallRefs = UIDoc.Selection.PickObjects(ObjectType.Element, new ClassSelectionFilter<Wall>(), "Выберите стены").ToList();
                walls = wallRefs.Select(x => Doc.GetElement(x.ElementId) as Wall).ToList();
            }
            else
            {
                walls = selected.Select(x => Doc.GetElement(x) as Wall).ToList();
            }
            return walls;
        }

        public List<CompoundStructureLayer> GetSortedLayers(List<Wall> walls, out int bearingIndex)
        {
            //Располагаем слои относительно координат их начала
            walls = walls.OrderBy(x => (x.Location as LocationCurve).Curve.GetEndPoint(0).X)
                         .ThenBy(y => (y.Location as LocationCurve).Curve.GetEndPoint(0).Y).ToList();

            // если стена была начерчена с определенным углом (а именно 0 >= a > -180) то нужно обратить порядок слоев
            Curve wallLocation = (ParentWall.Location as LocationCurve).Curve;
            XYZ wallDirection = (wallLocation.GetEndPoint(1) - wallLocation.GetEndPoint(0)).Normalize();        
            if (wallDirection.IsAlmostEqualTo(XYZ.BasisX) || wallDirection.Y < 0)
            {
                walls.Reverse();
            }
            if (ParentWall.Flipped)
            {
                walls.Reverse();
            }

            var layers = walls.SelectMany(x => x.WallType.GetCompoundStructure().GetLayers()).ToList();
            Wall bearingWall = walls.Where(x => x.StructuralUsage == Autodesk.Revit.DB.Structure.StructuralWallUsage.Bearing).FirstOrDefault();
            bearingIndex = walls.IndexOf(bearingWall);
            return layers;
        }

        public void MergeWalls(List<Wall> walls)
        {
            int bearingIndex = 0;
            var layers = GetSortedLayers(walls, out bearingIndex);
            var mergedStructure = CompoundStructure.CreateSimpleCompoundStructure(layers);
            mergedStructure.StructuralMaterialIndex = 0;
            WallType mergedWallType = FindWallTypeByStructure(mergedStructure);

            if (mergedWallType == null)
            {
                string name = GenerateNewWallTypeName(mergedStructure);
                mergedWallType = FindWalltypeByName(name);
                if (mergedWallType == null)
                {
                    mergedWallType = walls[0].WallType.Duplicate(name) as WallType;
                    mergedStructure.StructuralMaterialIndex = bearingIndex;
                    mergedWallType.SetCompoundStructure(mergedStructure);
                }
            }
            ParentWall.ChangeTypeId(mergedWallType.Id);          
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

        /// <summary>
        /// Метод возвращает первый типоразмер с аналогичной структурой (или null)
        /// </summary>
        /// <param name="structure"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Метод расчитывает расстояние между центральной осью всех слоев и центральной осью выбранного слоя
        /// </summary>
        /// <param name="structures"></param>
        /// <param name="currentStructureIndex"></param>
        /// <returns></returns>
        public double CalculateLayerOffset(IEnumerable<CompoundStructure> structures, int currentStructureIndex)
        {
            // Растояние от начала стены (наружный слой) до середины стены
            double wallW = structures.Sum(x => x.GetWidth()) / 2;
            // Растояние от начала стены (наружный слой) до середины нужного слоя
            double layerW = structures.Take(currentStructureIndex).Sum(x => x.GetWidth()) + structures.ElementAt(currentStructureIndex).GetWidth() / 2;
            return layerW - wallW;
        }

        public double CalculateLayerOffset(CompoundStructure structure)
        {
            int currentStructureIndex = structure.StructuralMaterialIndex;
            // Растояние от начала стены (наружный слой) до середины стены
            double wallW = structure.GetWidth() / 2;
            // Растояние от начала стены (наружный слой) до середины нужного слоя
            double layerW = 0;
            for (int i = 0; i < currentStructureIndex; i++)
            {
                layerW += structure.GetLayerWidth(i);
            }
            layerW += structure.GetLayerWidth(currentStructureIndex) / 2;
            return wallW - layerW;
        }

        /// <summary>
        /// Параллельное смещение стены. Положительные значения - вправо от вектора стены, отрицательные - влево. Вектор стены - от начала к концу при создании
        /// </summary>
        /// <param name="wall">Стена для смещения</param>
        /// <param name="wallOffset">Длина вектора смещения</param>
        public void MoveLayer(Wall wall, double wallOffset)
        {
            XYZ translationVector = (ParentLocationCurve.GetEndPoint(1) - ParentLocationCurve.GetEndPoint(0)).CrossProduct(XYZ.BasisZ).Normalize();
            translationVector = translationVector * wallOffset;
            ElementTransformUtils.MoveElement(Doc, wall.Id, translationVector);
        }

        /// <summary>
        /// Создание копии стены
        /// </summary>
        /// <param name="newWallType">Тип новой стены</param>
        /// <returns></returns>
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
    }

    [Transaction(TransactionMode.Manual)]
    class LayerSplit_AllLayers : WallSplit
    {
        public override void SplitStructure(CompoundStructure structure, out List<CompoundStructure> splittedStructures, out int bearingIndex)
        {
            var layers = structure.GetLayers();
            bearingIndex = structure.StructuralMaterialIndex;
            splittedStructures = layers.Select(x => CompoundStructure.CreateSimpleCompoundStructure(new List<CompoundStructureLayer> { x })).ToList();
        }
    }


    [Transaction(TransactionMode.Manual)]
    class LayerSplit_ExtractBearing : WallSplit
    {
        public override void SplitStructure(CompoundStructure structure, out List<CompoundStructure> splittedStructures, out int bearingIndex)
        {
            splittedStructures = new List<CompoundStructure>();            
            int structuralIndex = structure.StructuralMaterialIndex;
            if (structuralIndex == -1)
            {
                // в стене нет несущей части
                bearingIndex = -1;
                return;
            }
            var layers = structure.GetLayers();
            var outerLayers = layers.Take(structuralIndex).ToList();
            var bearingLayers = layers.Skip(structuralIndex).Take(1).ToList();
            var innerLayers = layers.Skip(structuralIndex + 1).ToList();
            
            if (outerLayers.Count > 0)
            {
                splittedStructures.Add(CompoundStructure.CreateSimpleCompoundStructure(outerLayers));
                bearingIndex = 1;
            }
            else
            {
                bearingIndex = 0;
            }

            splittedStructures.Add(CompoundStructure.CreateSimpleCompoundStructure(bearingLayers));

            if (innerLayers.Count > 0)
            {
                splittedStructures.Add(CompoundStructure.CreateSimpleCompoundStructure(innerLayers));
            }

        }
    }


  [Transaction(TransactionMode.Manual)]
    class LayerSplit_ExtractFinish : WallSplit
    {
        public override void SplitStructure(CompoundStructure structure, out List<CompoundStructure> splittedStructures, out int bearingIndex)
        {
            bearingIndex = -1;
            var layers = structure.GetLayers();
            List<CompoundStructureLayer> compoundStructureLayers = new List<CompoundStructureLayer>();
            splittedStructures = new List<CompoundStructure>();
            for (int i = 0; i < layers.Count; i++)
            {
                var layer = layers[i];

                if (layer.Function == MaterialFunctionAssignment.Finish1 || layer.Function == MaterialFunctionAssignment.Finish2)
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
    }

    [Transaction(TransactionMode.Manual)]
    class LayerSplit_Merge : WallSplit
    {
        public override Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // инициализация
            UIDoc = commandData.Application.ActiveUIDocument;
            List<Wall> ParentWalls = SelectionHandler();
            ParentWall = ParentWalls.Where(x => x.StructuralUsage == Autodesk.Revit.DB.Structure.StructuralWallUsage.Bearing).FirstOrDefault();
            if (ParentWall == null)
            {
                TaskDialog.Show("Ошибка", "Не найден несущий слой. Объединение невозможно");
                return Result.Failed;
            }

            using (Transaction tr = new Transaction(Doc, "Разбить стену по несущему слою"))
            {
                tr.Start();
                MergeWalls(ParentWalls);
                var structure = ParentWall.WallType.GetCompoundStructure();

                double offset = CalculateLayerOffset(structure);
                offset = ParentWall.Flipped ? -offset : offset;
                MoveLayer(ParentWall, offset);

                foreach (Wall wall in ParentWalls)
                {
                    if (wall.Id.IntegerValue != ParentWall.Id.IntegerValue)
                    {
                        Doc.Delete(wall.Id);
                    }
                }                
                tr.Commit();
            }
            return Result.Succeeded;
        }
        
        public override void SplitStructure(CompoundStructure structure, out List<CompoundStructure> splittedStructures, out int bearingIndex)
        {
            throw new NotImplementedException();
        }
    }
}
