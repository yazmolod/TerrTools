using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace TerrTools
{
    [Transaction(TransactionMode.Manual)]
    class Marking : IExternalCommand
    {
        public UIDocument UIDoc { get; set; }
        public Document Doc { get => UIDoc.Document; }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIDoc = commandData.Application.ActiveUIDocument;
                var selectedMarkReference = UIDoc.Selection.PickObject(ObjectType.Element, new TagsSelectionFilter(), "Выберите марку");
                var selectedMark = Doc.GetElement(selectedMarkReference.ElementId);
                MarkingElements(selectedMark);
                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
        }
        // Возвращает все экземпляры, присущие категории выбранной
        // марки на виде для их последующей маркировки.
        private IEnumerable<ElementId> GetAllViewInstancesByCategory(ElementId selectedItem)
        {
           var excessItem =  Doc.GetElement(selectedItem);
           var allViewInstanceIds = new FilteredElementCollector(Doc, UIDoc.ActiveView.Id)
                         .OfCategoryId(excessItem.Category.Id)
                         .WhereElementIsNotElementType()
                         .ToElementIds()
                         .Where(x => excessItem.Id != x);
           return allViewInstanceIds;
        }
        // Маркиру ет элементы с учетом свойств выбранной пользователем марки.
        // нужно BIC а не mark
        private void MarkingElements(Reference selectedMarkReference, Element selectedTag)
        {
            using (Transaction trans = new Transaction(Doc))
            {
                trans.Start("Создание марок для элемента");
                // Случай, когда пользователь выбрал марку помещения.
                if ((BuiltInCategory)tag.Category.Id.IntegerValue == BuiltInCategory.OST_RoomTags)
                {
                    var selectedRoomTag = (RoomTag)tag;   
                    var instancesIdsForTagging = GetAllViewInstancesByCategory(selectedRoomTag.Room.Id);
                    var tagPosition = selectedRoomTag.TagHeadPosition;
                    var TagHostCenterPoint = GetCenterFromBoundingBox(Doc.GetElement(selectedRoomTag.Room.Id));
                    foreach (var instanceIdForTagging in instancesIdsForTagging)
                    {
                        var instanceForTagging = Doc.GetElement(instanceIdForTagging);
                        var tagEndPoint = GetCenterFromBoundingBox(Doc.GetElement(instanceIdForTagging));
                        UV uvPoint = new UV(tagEndPoint.X, tagEndPoint.Y);
                        var createdRoomTag = Doc.Create.NewRoomTag(new LinkElementId(instanceIdForTagging), uvPoint, UIDoc.ActiveView.Id);
                        createdRoomTag.RoomTagType = selectedRoomTag.RoomTagType;
                        createdRoomTag.HasLeader = selectedRoomTag.HasLeader;
                        createdRoomTag.TagOrientation = selectedRoomTag.TagOrientation;
                        // В случае, когда марка имеет выноску,
                        // производится настройка выноски для созданной марки.
                        if (selectedRoomTag.HasLeader)
                        {
                            var hostElementLocation = selectedRoomTag.Room.Location as LocationPoint;
                            var hostLP = hostElementLocation.Point;
                            var locationInstance = instanceForTagging.Location as LocationPoint;
                            var instanceLP = locationInstance.Point;
                            createdRoomTag.LeaderEnd = GetPointForOffset(hostLP, selectedRoomTag.LeaderEnd, instanceLP);
                            createdRoomTag.TagHeadPosition = GetPointForOffset(selectedRoomTag.LeaderEnd, selectedRoomTag.TagHeadPosition, createdRoomTag.LeaderEnd);
                            // Если выноска загибается.
                            if (selectedRoomTag.HasElbow)
                            {
                                createdRoomTag.LeaderElbow = GetPointForOffset(selectedRoomTag.TagHeadPosition, selectedRoomTag.LeaderElbow, createdRoomTag.TagHeadPosition);
                            }
                        }
                    }
                }
                // Случай, когда пользователь выбрал марку пространства.
                else if ((BuiltInCategory)tag.Category.Id.IntegerValue == BuiltInCategory.OST_MEPSpaceTags)
                {
                    var selectedSpaceTag = (SpaceTag)tag;
                    var instancesIdsForTagging = GetAllViewInstancesByCategory(selectedSpaceTag.Space.Id);
                    foreach (var instanceIdForTagging in instancesIdsForTagging)
                    {
                        var instanceForTagging = Doc.GetElement(instanceIdForTagging);
                        var tagEndPoint = GetCenterFromBoundingBox(Doc.GetElement(instanceIdForTagging));
                        UV uvPoint = new UV(tagEndPoint.X, tagEndPoint.Y);
                        var createdSpaceTag = Doc.Create.NewSpaceTag((Space)Doc.GetElement(instanceIdForTagging), uvPoint, UIDoc.ActiveView);
                        createdSpaceTag.SpaceTagType = selectedSpaceTag.SpaceTagType;
                        createdSpaceTag.TagOrientation = selectedSpaceTag.TagOrientation;
                        createdSpaceTag.HasLeader = selectedSpaceTag.HasLeader;
                        // В случае, когда марка имеет выноску,
                        // производится настройка выноски для созданной марки.
                        if (selectedSpaceTag.HasLeader)
                        {
                            var hostElementLocation = selectedSpaceTag.Space.Location as LocationPoint;
                            var hostLP = hostElementLocation.Point;
                            var locationInstance = instanceForTagging.Location as LocationPoint;
                            var instanceLP = locationInstance.Point;
                            createdSpaceTag.LeaderEnd = GetPointForOffset(hostLP, selectedSpaceTag.LeaderEnd, instanceLP);
                            createdSpaceTag.TagHeadPosition = GetPointForOffset(selectedSpaceTag.LeaderEnd, selectedSpaceTag.TagHeadPosition, createdSpaceTag.LeaderEnd);
                            // Если выноска загибается.
                            if (selectedSpaceTag.HasElbow)
                            {
                                createdSpaceTag.LeaderElbow = GetPointForOffset(selectedSpaceTag.TagHeadPosition, selectedSpaceTag.LeaderElbow, createdSpaceTag.TagHeadPosition);
                            }
                        }
                    }
                }
                else
                {
                    var selectedTag = (IndependentTag)tag;
                    var instancesIdsForTagging = GetAllViewInstancesByCategory(selectedTag.TaggedLocalElementId);
                    var tagPosition= selectedTag.TagHeadPosition;
                    var TagHostCenterPoint = GetCenterFromBoundingBox(Doc.GetElement(selectedTag.TaggedLocalElementId));
                    foreach (var instanceIdForTagging in instancesIdsForTagging)
                    {
                        var instanceForTagging = Doc.GetElement(instanceIdForTagging);
                        var creatingMarkHeaderPoint = GetPointForOffset(TagHostCenterPoint,
                            tagPosition, GetCenterFromBoundingBox(instanceForTagging));
                        Reference tagHostReference = new Reference(instanceForTagging);
                        // Создается новый экземпляр марки.
                        var createdTag = IndependentTag.Create(Doc, selectedTag.GetTypeId(), UIDoc.ActiveView.Id,
                        tagHostReference, selectedTag.HasLeader, selectedTag.TagOrientation, creatingMarkHeaderPoint);
                        createdTag.LeaderEndCondition = selectedTag.LeaderEndCondition;
                        // В случае, когда марка имеет выноску,
                        // производится настройка выноски для созданной марки.
                        if (selectedTag.HasLeader)
                        {
                            var hostElementLocation = selectedTag.GetTaggedLocalElement().Location as LocationPoint;
                            var hostLP = hostElementLocation.Point;
                            var locationInstance = instanceForTagging.Location as LocationPoint;
                            var instanceLP = locationInstance.Point;
                            // Если выноска имеет закрепленный конец.
                            if (selectedTag.LeaderEndCondition == LeaderEndCondition.Attached)
                            {
                                createdTag.TagHeadPosition = GetPointForOffset(hostLP, selectedTag.TagHeadPosition, instanceLP);
                            }
                            // Если выноска имеет свободный конец.
                            else
                            {
                                createdTag.LeaderEnd = GetPointForOffset(hostLP, selectedTag.LeaderEnd, instanceLP);
                                createdTag.TagHeadPosition = GetPointForOffset(selectedTag.LeaderEnd, selectedTag.TagHeadPosition, createdTag.LeaderEnd);
                            }
                            // Если выноска загибается.
                            if (selectedTag.HasElbow)
                            {
                                createdTag.LeaderElbow = GetPointForOffset(selectedTag.TagHeadPosition, selectedTag.LeaderElbow, createdTag.TagHeadPosition);
                            }
                        }
                    }
                }
                trans.Commit();
            }
        }
        /// <summary>
        /// Возвращает координаты точки, в которую нужно поместить
        /// ту или иную часть выноски(end, header, elbow).
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
