using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace TerrTools

{
    abstract class HorizontalFinishing
    {
        public abstract Document doc { get; set; }
        public abstract string finishIdParameterName { get; }
        public abstract BuiltInParameter roomFinishParameter { get; }
        public abstract BuiltInParameter finishingOffsetParameter { get; }
        public abstract List<ElementType> finishingTypes { get; }
        protected abstract Element CreateHostGeometry(HorizontalFinishingResult res);

        protected List<HorizontalFinishingResult> FormResult { get; set; }
        protected List<Room> rooms
        {
            get
            {
                return new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .Cast<Room>()
                .Where(x => x.LookupParameter("Площадь").HasValue)
                .ToList();
            }
        }

        protected void CheckDefaultSharedParameters()
        {
            bool done = true;
            done &= SharedParameterUtils.AddSharedParameter(doc, "ТеррНИИ_Идентификатор отделки пола",
                    new BuiltInCategory[] { BuiltInCategory.OST_Rooms }, BuiltInParameterGroup.PG_ANALYSIS_RESULTS);
            done &= SharedParameterUtils.AddSharedParameter(doc, "ТеррНИИ_Идентификатор потолка",
                    new BuiltInCategory[] { BuiltInCategory.OST_Rooms }, BuiltInParameterGroup.PG_ANALYSIS_RESULTS);
            done &= SharedParameterUtils.AddSharedParameter(doc, "ТеррНИИ_Номер помещения",
                    new BuiltInCategory[] { BuiltInCategory.OST_Floors, BuiltInCategory.OST_Ceilings }, BuiltInParameterGroup.PG_ANALYSIS_RESULTS);
            done &= SharedParameterUtils.AddSharedParameter(doc, "ТеррНИИ_Номера всех помещений",
                    new BuiltInCategory[] { BuiltInCategory.OST_Floors, BuiltInCategory.OST_Ceilings }, BuiltInParameterGroup.PG_ANALYSIS_RESULTS, isIntance: false);
        }

        protected void UpdateFinishingType()
        {
            /// Все помещения типоразмера
            IEnumerable<Element> allFloorTypes = new FilteredElementCollector(doc).OfClass(typeof(FloorType));
            IEnumerable<Floor> allFloors = new FilteredElementCollector(doc).OfClass(typeof(Floor)).Cast<Floor>();
            foreach (Element elType in allFloorTypes)
            {
                if (elType != null)
                {
                    string value = String.Join(", ",
                        (from fl in allFloors
                         where fl.FloorType.Id == elType.Id
                         select fl.LookupParameter("ТеррНИИ_Номер помещения").AsString()).Distinct().Where(x => x != "" && x != null).OrderBy(x => x));
                    Parameter pAllRooms = elType.LookupParameter("ТеррНИИ_Номера всех помещений");
                    pAllRooms?.Set(value);
                }
            }
        }
        protected void CreateOpening()
        {
            foreach (HorizontalFinishingResult res in FormResult)
            {
                foreach (CurveArray profile in res.OpeningProfiles)
                {
                    try
                    {
                        if (res.FinishingElement != null && res.FinishingElement.IsValidObject) doc.Create.NewOpening(res.FinishingElement as Element, profile, true);
                    }
                    catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                    {
                        continue;
                    }
                }
            }
        }

        protected void UpdateFinishingElement()
        {
            foreach (HorizontalFinishingResult result in FormResult)
            {
                Parameter pRoomFinishingName = result.Room.get_Parameter(roomFinishParameter);

                // Удаляем старый объект и замещаем новым
                Parameter pRoomFinishingId = result.Room.LookupParameter(finishIdParameterName);
                List<OldTag> oldTags = null;
                ElementId oldFloor = null;
                if (pRoomFinishingId.HasValue)
                    try
                    {
                        oldFloor = new ElementId(pRoomFinishingId.AsInteger());
                        oldTags = GetOldTags(oldFloor);
                        doc.Delete(oldFloor);
                    }
                    catch (Autodesk.Revit.Exceptions.ArgumentException) { }

                if (result.FinishingType != null)
                {
                    // создание геометрии
                    Element finishingElement = CreateHostGeometry(result);
                    if (finishingElement != null)
                    {
                        finishingElement.get_Parameter(finishingOffsetParameter).Set(result.Offset / 304.8);

                        /// заполнение данных
                        // Номер помещения конкретного элемента
                        string roomNumber = result.Room.get_Parameter(BuiltInParameter.ROOM_NUMBER).AsString();
                        Parameter pRoomNumber = finishingElement.LookupParameter("ТеррНИИ_Номер помещения");
                        pRoomNumber.Set(roomNumber);

                        // Название отделки для помещения
                        pRoomFinishingName.Set(finishingElement.Name);

                        // Идентификатор пола для помещения    
                        pRoomFinishingId.Set(finishingElement.Id.IntegerValue);
                        result.FinishingElement = finishingElement;

                        // Восстановление старой марки
                        if (oldTags != null)
                        {
                            foreach (OldTag oldTag in oldTags)
                            {
                                IndependentTag newTag = IndependentTag.Create(
                                    doc,
                                    oldTag.TypeId,
                                    oldTag.OwnerView.Id,
                                    new Reference(finishingElement),
                                    false,
                                    oldTag.TagOrientation,
                                    oldTag.TagHeadPosition);
                                newTag.ChangeTypeId(oldTag.TypeId);
                            }
                        }
                    }
                    else
                    {
                        // Идентификатор пола для помещения    
                        pRoomFinishingId.Set(-1);
                        // Название отделки для помещения
                        pRoomFinishingName.Set(result.FinishingType.Name);
                    }
                }
                else
                {
                    // Идентификатор пола для помещения    
                    pRoomFinishingId.Set(-1);
                    result.FinishingElement = null;
                    // Название отделки для помещения
                    pRoomFinishingName.Set("");
                }
            }
        }

        private List<OldTag> GetOldTags(ElementId oldFloor)
        {
            List<OldTag> tags = new List<OldTag>();
            foreach (var x in new FilteredElementCollector(doc)
                .OfClass(typeof(IndependentTag))
                .Cast<IndependentTag>()
                .Where(x => x.TaggedLocalElementId == oldFloor)) {
                tags.Add(new OldTag(x.TagOrientation, x.TagHeadPosition, doc.GetElement(x.OwnerViewId) as View, x.GetTypeId()));
            };
            return tags;
        }

        protected Result Generation()
        {
            using (Transaction tr = new Transaction(doc))
            {
                tr.Start("Генерация полов");
                CheckDefaultSharedParameters();
                var form = new HorizontalFinishingForm(rooms, finishingTypes, finishIdParameterName, roomFinishParameter, finishingOffsetParameter);
                if (form.DialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    this.FormResult = form.Result;
                    UpdateFinishingElement();
                    tr.Commit();
                    using (Transaction tr2 = new Transaction(doc))
                    {
                        tr2.Start("Генерация отверстий");
                        CreateOpening();
                        UpdateFinishingType();
                        tr2.Commit();
                    }
                    return Result.Succeeded;
                }
                else
                {
                    tr.RollBack();
                    return Result.Cancelled;
                }
            }
        }
    }

    class OldTag
    {
        public TagOrientation TagOrientation { get; set; }
        public XYZ TagHeadPosition { get; set; }
        public View OwnerView { get; set; }
        public ElementId TypeId { get; set; }

        public OldTag(TagOrientation to, XYZ thp, View v, ElementId type)
        {
            TagOrientation = to;
            TagHeadPosition = thp;
            OwnerView = v;
            TypeId = type;
        }
    }


    [Transaction(TransactionMode.Manual)]
    class FloorFinishing : HorizontalFinishing, IExternalCommand
    {
        public override Document doc { get; set; }
        public override string finishIdParameterName { get { return "ТеррНИИ_Идентификатор отделки пола"; } }
        public override BuiltInParameter roomFinishParameter { get { return BuiltInParameter.ROOM_FINISH_FLOOR; } }    
        public override BuiltInParameter finishingOffsetParameter { get { return BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM; } }
        public override List<ElementType> finishingTypes
        {
            get
            {
                return new FilteredElementCollector(doc)
                .OfClass(typeof(FloorType))
                .Cast<ElementType>()
                .ToList();
            }
        }
        protected override Element CreateHostGeometry(HorizontalFinishingResult res)
        {
            try {           
            
                /*CurveArray mainProfileWithDoors = res.MainProfile;
            foreach (FamilyInstance door in new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Doors))
            {
                if (FinishingData.IsElementCollideRoom(res.Room, door))
                {
                    Curve c = ModelCurveCreator.GetFamilyInstanceCutBaseLine(door);
                    XYZ ptSt = c.GetEndPoint(0); 
                    XYZ ptEnd = c.GetEndPoint(1);

                    mainProfileWithDoors.get_Item(0).Project
                }
            }*/


            return doc.Create.NewFloor(res.MainProfile, res.FinishingType as FloorType, res.Level, false); }
            catch (Autodesk.Revit.Exceptions.ArgumentException e)
            {
                ModelCurveCreator mmc = new ModelCurveCreator(doc);
                mmc.DrawGroup(res.MainProfile, "Debug");
                Debug.WriteLine(e.ToString());
                TaskDialog td = new TaskDialog("Предупреждение");
                td.MainIcon = TaskDialogIcon.TaskDialogIconWarning;
                td.MainInstruction = string.Format("Помещение {0} имеет незамкнутый внешний контур. создание отделки пола для него было пропущено", res.Room.Number);
                td.MainContent = "Проверьте контур помещения, а также же правильность соединения внешних стен";
                td.Show();
                return null;
            }
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            doc = commandData.Application.ActiveUIDocument.Document;
            return Generation();            
        }
    }

    [Transaction(TransactionMode.Manual)]
    class CeilingFinishing : HorizontalFinishing, IExternalCommand
    {
        public override Document doc { get; set; }
        public override string finishIdParameterName { get { return "ТеррНИИ_Идентификатор потолка"; } }
        public override BuiltInParameter roomFinishParameter { get { return BuiltInParameter.ROOM_FINISH_CEILING; } }
        public override BuiltInParameter finishingOffsetParameter { get { return BuiltInParameter.CEILING_HEIGHTABOVELEVEL_PARAM; } }
        public override List<ElementType> finishingTypes
        {
            get
            {
                return new FilteredElementCollector(doc)
                .OfClass(typeof(CeilingType))
                .Cast<ElementType>()
                .ToList();
            }
        }
        protected override Element CreateHostGeometry(HorizontalFinishingResult res)
        {
            return null;
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            doc = commandData.Application.ActiveUIDocument.Document;
            return Generation();
        }
    }
}
