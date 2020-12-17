using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using TerrTools.UI;

namespace TerrTools
{
    [Transaction(TransactionMode.Manual)]
    class Marking : IExternalCommand
    {
        // Хранит в себе информацию о варианте смещения марки
        // относительно bounding box.
        public static string MarkOffset { get; set; }
        public UIDocument UIDoc { get; set; }
        public Document Doc { get => UIDoc.Document; }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var form = new UI.MarkingForm();
            try
            {
                UIDoc = commandData.Application.ActiveUIDocument;
                var selectedMarkReference = UIDoc.Selection.PickObject(ObjectType.Element, new TagsSelectionFilter(), "Выберите марку");
                form.Close();
                var selectedMark = Doc.GetElement(selectedMarkReference.ElementId);
                MarkingElements(selectedMark);
                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                form.Close();
                return Result.Cancelled;
            }
        }
        // Возвращает все экземпляры, присущие категории выбранной
        // марки на виде для их последующей маркировки.
        private IEnumerable<ElementId> GetAllViewInstancesByCategory(ElementId selectedItem)
        {
           var excessItem = Doc.GetElement(selectedItem);
           var allViewInstanceIds = new FilteredElementCollector(Doc, UIDoc.ActiveView.Id)
                         .OfCategoryId(excessItem.Category.Id)
                         .WhereElementIsNotElementType()
                         .ToElementIds()
                         .Where(x => excessItem.Id != x);
           return allViewInstanceIds;
        }
        /// <summary>
        /// Маркирует элементы с учетом свойств выбранной пользователем марки.
        /// </summary>
        /// <param name="tag">Выбранная пользователем марка</param>
        /// <param name="typeOffset">Тип смещения марки относительно центра bounding box маркируемого элемента</param>
        private void MarkingElements(Element tag)
        {
            using (Transaction trans = new Transaction(Doc))
            {
                trans.Start("Создание марок для элемента");
                // Случай, когда пользователь выбрал марку помещения.
                if ((BuiltInCategory)tag.Category.Id.IntegerValue == BuiltInCategory.OST_RoomTags)
                {
                    var selectedRoomTag = (RoomTag)tag;
                    var instancesIdsForTagging = GetAllViewInstancesByCategory(selectedRoomTag.Room.Id);
                    var selectedTagHeadPosition = selectedRoomTag.TagHeadPosition;
                    var oldRoomBBPoint = GetPointFromBoundingBox(selectedRoomTag.Room, MarkOffset);
                    foreach (var instanceIdForTagging in instancesIdsForTagging)
                    {
                        var newRoom = Doc.GetElement(instanceIdForTagging);
                        var newRoomBBPoint = GetPointFromBoundingBox(newRoom, MarkOffset);
                        var newHeadPoint = GetPointForOffset(oldRoomBBPoint, selectedTagHeadPosition, newRoomBBPoint);
                        UV uvPoint = new UV(newHeadPoint.X, newHeadPoint.Y);
                        var createdRoomTag = Doc.Create.NewRoomTag(new LinkElementId(instanceIdForTagging), uvPoint, UIDoc.ActiveView.Id);
                        createdRoomTag.RoomTagType = selectedRoomTag.RoomTagType;
                        createdRoomTag.HasLeader = selectedRoomTag.HasLeader;
                        createdRoomTag.TagOrientation = selectedRoomTag.TagOrientation;
                        if (selectedRoomTag.HasLeader)
                        {
                            try
                            {
                                createdRoomTag.LeaderEnd = GetPointForOffset(selectedRoomTag.TagHeadPosition,
                                    selectedRoomTag.LeaderEnd, createdRoomTag.TagHeadPosition);
                            }
                            // В случае, если конец выноски будет выходить за пределы помещения,
                            // он просто останется в дефолтном при создании месте.
                            catch (Autodesk.Revit.Exceptions.ArgumentException)
                            {
                            }
                            if (selectedRoomTag.HasElbow)
                            {
                                createdRoomTag.LeaderElbow = GetPointForOffset(oldRoomBBPoint,
                                    selectedRoomTag.LeaderElbow, newRoomBBPoint);
                            }
                        }
                    }
                }
                else if ((BuiltInCategory)tag.Category.Id.IntegerValue == BuiltInCategory.OST_MEPSpaceTags)
                {
                    var selectedSpaceTag = (SpaceTag)tag;
                    var instancesIdsForTagging = GetAllViewInstancesByCategory(selectedSpaceTag.Space.Id);
                    var selectedTagHeadPosition = selectedSpaceTag.TagHeadPosition;
                    var oldSpaceBBPoint = GetPointFromBoundingBox(selectedSpaceTag.Space, MarkOffset);
                    foreach (var instanceIdForTagging in instancesIdsForTagging)
                    {
                        var newSpace = Doc.GetElement(instanceIdForTagging);
                        var newSpaceBBPoint = GetPointFromBoundingBox(newSpace, MarkOffset);
                        var newHeadPoint = GetPointForOffset(oldSpaceBBPoint, selectedTagHeadPosition, newSpaceBBPoint);
                        UV uvPoint = new UV(newHeadPoint.X, newHeadPoint.Y);
                        var createdSpaceTag = Doc.Create.NewSpaceTag((Space)newSpace, uvPoint, UIDoc.ActiveView);
                        createdSpaceTag.SpaceTagType = selectedSpaceTag.SpaceTagType;
                        createdSpaceTag.HasLeader = selectedSpaceTag.HasLeader;
                        createdSpaceTag.TagOrientation = selectedSpaceTag.TagOrientation;
                        if (selectedSpaceTag.HasLeader)
                        {
                            try
                            {
                                createdSpaceTag.LeaderEnd = GetPointForOffset(selectedSpaceTag.TagHeadPosition,
                                    selectedSpaceTag.LeaderEnd, createdSpaceTag.TagHeadPosition);
                            }
                            // В случае, если конец выноски будет выходить за пределы помещения,
                            // он просто останется в дефолтном при создании месте.
                            catch (Autodesk.Revit.Exceptions.ArgumentException)
                            {
                            }
                            if (selectedSpaceTag.HasElbow)
                            {
                                createdSpaceTag.LeaderElbow = GetPointForOffset(oldSpaceBBPoint,
                                    selectedSpaceTag.LeaderElbow, newSpaceBBPoint);
                            }
                        }
                    }
                }
                else
                {
                    var selectedTag = (IndependentTag)tag;
                    var instancesIdsForTagging = GetAllViewInstancesByCategory(selectedTag.TaggedLocalElementId);
                    var selectedTagHeadPosition = selectedTag.TagHeadPosition;
                    var oldElementBBPoint = GetPointFromBoundingBox(selectedTag.GetTaggedLocalElement(), MarkOffset);
                    foreach (var instanceIdForTagging in instancesIdsForTagging)
                    {
                        var newElementToTag = Doc.GetElement(instanceIdForTagging);
                        var newElementBBPoint = GetPointFromBoundingBox(newElementToTag, MarkOffset);
                        var newHeadPoint = GetPointForOffset(oldElementBBPoint, selectedTagHeadPosition, newElementBBPoint);
                        var createdTag = IndependentTag.Create(Doc, selectedTag.GetTypeId(),
                            UIDoc.ActiveView.Id, new Reference(newElementToTag), 
                            selectedTag.HasLeader, selectedTag.TagOrientation, 
                            newHeadPoint);
                        createdTag.LeaderEndCondition = selectedTag.LeaderEndCondition;
                        createdTag.TagOrientation = selectedTag.TagOrientation;
                        createdTag.HasLeader = selectedTag.HasLeader;
                        if (selectedTag.HasLeader)
                        {
                            createdTag.TagHeadPosition = newHeadPoint;
                            // Если выноска имеет свободный конец.
                            if (selectedTag.LeaderEndCondition == LeaderEndCondition.Free)
                            {
                                createdTag.LeaderEnd = GetPointForOffset(selectedTag.TagHeadPosition,
                                    selectedTag.LeaderEnd, createdTag.TagHeadPosition);
                            }
                            // Если выноска загибается.
                            if (selectedTag.HasElbow)
                            {
                                createdTag.LeaderElbow = GetPointForOffset(oldElementBBPoint,
                                        selectedTag.LeaderElbow, newElementBBPoint);
                            }
                        }
                    }
                }
                trans.Commit();
            }
        }
        /// <summary>
        /// Возвращает координаты точки.
        /// </summary>
        /// <param name="pt1">Координаты начала вектора</param>
        /// <param name="pt2">Координаты конца вектора</param>
        /// <param name="pt3">Координаты начала другого вектора</param>
        private XYZ GetPointForOffset(XYZ pt1, XYZ pt2, XYZ pt3)
        {
            var vector = pt2 - pt1;
            return pt3 + vector;
        }
        // Возвращает центр из BoundingBox элемента.
        private XYZ GetCenterFromBoundingBox(Element el)
        {
            var boundingBox = el.get_BoundingBox(null);
            return (boundingBox.Max + boundingBox.Min) / 2;
        }
        // Возвращает центр из BoundingBox элемента с учетом смещения.
        private XYZ GetPointFromBoundingBox(Element el, string typeOffset)
        {
            var boundingBox = el.get_BoundingBox(null);
            var p1 = boundingBox.Min;
            var p2 = boundingBox.Max;
            XYZ pt1;
            XYZ pt2;
            switch (typeOffset)
            {
                // Центр
                case "radioButton1":
                    return (p1 + p2) / 2;
                // Верх
                case "radioButton2":
                    pt1 = new XYZ(p1.X, p2.Y, p2.Z);
                    return (p2 + pt1) / 2;
                // Низ
                case "radioButton3":
                    pt1 = new XYZ(p1.X, p1.Y, p2.Z);
                    pt2 = new XYZ(p2.X, p1.Y, p2.Z);
                    return (pt1 + pt2) / 2;
                // Лево
                case "radioButton4":
                    pt1 = new XYZ(p1.X, p1.Y, p2.Z);
                    pt2 = new XYZ(p1.X, p2.Y, p2.Z);
                    return (pt1 + pt2) / 2;
                // Право
                case "radioButton5":
                    pt1 = new XYZ(p2.X, p1.Y, p2.Z);
                    return (p2 + pt1) / 2;
                // Лево, верх
                case "radioButton6":
                    pt1 = new XYZ(p1.X, p2.Y, p2.Z);
                    return pt1;
                // Право, верх
                case "radioButton7":
                    return p2;
                // Лево, низ
                case "radioButton8":
                    pt1 = new XYZ(p1.X, p1.Y, p2.Z);
                    return pt1;
                // Право, низ
                case "radioButton9":
                    pt1 = new XYZ(p2.X, p1.Y, p2.Z);
                    return pt1;
            }
            var centerFromBB = (boundingBox.Max + boundingBox.Min) / 2;
            return centerFromBB;
        }
        // Фильтр для выбора элементов. 
        // Позволит пользователю выбрать только из марок.
        private class TagsSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element element)
            {
                if (element != null)
                {
                    if (element.Category.IsTagCategory)
                    {
                        return true;
                    }
                }
                return false;
            }
            public bool AllowReference(Reference refer, XYZ point)
            {
                return false;
            }
        }
    }
}
