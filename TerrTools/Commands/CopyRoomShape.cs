using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.Attributes;

namespace TerrTools
{
    class RoomFilter : ISelectionFilter
    {
        bool ISelectionFilter.AllowElement(Element elem)
        {
            return elem is SpatialElement;
        }

        bool ISelectionFilter.AllowReference(Reference reference, XYZ position)
        {
            throw new NotImplementedException();
        }
    }

    [Transaction(TransactionMode.Manual)]
    class CopyRoomShape: IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            Selection selection = commandData.Application.ActiveUIDocument.Selection;
            SpatialElementBoundaryOptions opt = new SpatialElementBoundaryOptions();
            ModelCurveCreator mmc = new ModelCurveCreator(doc);           

            IList<Reference> rooms = selection.PickObjects(ObjectType.Element, new RoomFilter(), "Выберите помещения");
            try
            {
                using (Transaction tr = new Transaction(doc, "Создание контуров"))
                {
                    tr.Start();                    
                    foreach (Reference roomref in rooms)
                    {
                        CurveArray curves = new CurveArray();
                        SpatialElement room = doc.GetElement(roomref.ElementId) as SpatialElement;
                        foreach (var boundary in GeometryUtils.GetCurvesListFromSpatialElement(room))
                        {
                            foreach (var curve in boundary)
                            {
                                curves.Append(curve);
                            }
                        }
                        mmc.DrawGroup(curves, string.Format("Контур {0} #{1}", room.Category.Name, room.Number));
                    }
                    TaskDialog.Show("Результат", "Создано групп контуров: " + rooms.Count.ToString());
                    tr.Commit();
                }
            }
            catch
            {
                return Result.Failed;
            }
            return Result.Succeeded;
        }
    }
}
