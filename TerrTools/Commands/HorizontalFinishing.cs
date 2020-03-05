using System;
using System.Collections.Generic;
using System.Linq;
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
            using (Transaction tr = new Transaction(doc, "Добавление общих параметров"))
            {
                tr.Start();
                bool done = true;
                done &= SharedParameterUtils.AddSharedParameter(doc, "ТеррНИИ_Идентификатор отделки пола",
                        new BuiltInCategory[] { BuiltInCategory.OST_Rooms }, BuiltInParameterGroup.PG_ANALYSIS_RESULTS);
                done &= SharedParameterUtils.AddSharedParameter(doc, "ТеррНИИ_Идентификатор потолка",
                        new BuiltInCategory[] { BuiltInCategory.OST_Rooms }, BuiltInParameterGroup.PG_ANALYSIS_RESULTS);
                done &= SharedParameterUtils.AddSharedParameter(doc, "ТеррНИИ_Номер помещения",
                        new BuiltInCategory[] { BuiltInCategory.OST_Rooms, BuiltInCategory.OST_Ceilings }, BuiltInParameterGroup.PG_ANALYSIS_RESULTS);
                done &= SharedParameterUtils.AddSharedParameter(doc, "ТеррНИИ_Номера всех помещений",
                        new BuiltInCategory[] { BuiltInCategory.OST_Rooms }, BuiltInParameterGroup.PG_ANALYSIS_RESULTS, isIntance: false);
                tr.Commit();
            }
        }
    

        protected void UpdateFinishingType()
        {
            using (Transaction tr = new Transaction(doc, "Обновление типоразмеров"))
            {
                tr.Start();
                /// Все помещения типоразмера
                List<Element> allFloorTypes =
                    (from db in FormResult
                     select db.FinishingType).Distinct().Cast<Element>().ToList();
                foreach (Element elType in allFloorTypes)
                {
                    if (elType != null)
                    {
                        string value = String.Join(", ",
                            (from db in FormResult
                             where db.FinishingType == elType
                             select db.Room.Number).OrderBy(x => x));
                        Parameter pAllRooms = elType.LookupParameter("ТеррНИИ_Номера всех помещений");
                        pAllRooms.Set(value);

                    }
                }
                tr.Commit();
            }
        }
        protected void CreateOpening()
        {
            using (Transaction tr = new Transaction(doc, "Вырезание отверстий"))
            {
                tr.Start();
                foreach (HorizontalFinishingResult res in FormResult)
                {
                    foreach (CurveArray profile in res.OpeningProfiles)
                    {
                        if (res.FinishingElement != null) doc.Create.NewOpening(res.FinishingElement as Element, profile, true);
                    }
                }
                tr.Commit();
            }
        }
        protected void UpdateFinishingElement()
        {
            using (Transaction tr = new Transaction(doc, "Создание геометрии"))
            {
                tr.Start();
                foreach (HorizontalFinishingResult result in FormResult)
                {
                    Parameter pRoomFinishingName = result.Room.get_Parameter(roomFinishParameter);

                    // Удаляем старый объект и замещаем новым
                    Parameter pRoomFinishingId = result.Room.LookupParameter(finishIdParameterName);
                    List<OldTag> oldTags = null;
                    ElementId oldFloor = null;
                    if (pRoomFinishingId.HasValue)
                        try {
                            oldFloor = new ElementId(pRoomFinishingId.AsInteger());
                            oldTags =  GetOldTags(oldFloor);
                            doc.Delete(oldFloor); 
                        }
                        catch (Autodesk.Revit.Exceptions.ArgumentException) { }

                    if (result.FinishingType != null)
                    {
                        // создание геометрии
                        Element finishingElement = CreateHostGeometry(result);
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
                        if (oldTags != null) {
                            foreach (OldTag oldTag in oldTags)
                            {
                                IndependentTag newTag = doc.Create.NewTag(
                                    oldTag.OwnerView,
                                    finishingElement,
                                    false,
                                    TagMode.TM_ADDBY_CATEGORY,
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
                        result.FinishingElement = null;
                        // Название отделки для помещения
                        pRoomFinishingName.Set("");
                    }
                }
                tr.Commit();
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
            using (TransactionGroup transGroup = new TransactionGroup(doc))
            {
                transGroup.Start("Генерация полов");
                CheckDefaultSharedParameters();
                var form = new HorizontalFinishingForm(rooms, finishingTypes, finishIdParameterName, roomFinishParameter, finishingOffsetParameter);
                if (form.DialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    this.FormResult = form.Result;
                    UpdateFinishingElement();
                    CreateOpening();
                    UpdateFinishingType();
                    transGroup.Assimilate();
                    return Result.Succeeded;
                }
                else
                {
                    transGroup.RollBack();
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
            CurveArray mainProfileWithDoors = null;
            return doc.Create.NewFloor(mainProfileWithDoors, res.FinishingType as FloorType, res.Level, false);
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
