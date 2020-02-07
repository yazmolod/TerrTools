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
            return elem.Category.Name == "Помещения";
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

            List<Curve> curves = new List<Curve>();

            selection.PickObjects(ObjectType.Element, new RoomFilter(), "Выберите помещения");            
            foreach (ElementId elementId in selection.GetElementIds())
            {
                Room room = doc.GetElement(elementId) as Room;
                foreach (var boundary in GeometryUtils.GetCurvesListFromRoom(room))
                {
                    foreach (var curve in boundary)
                    {
                        curves.Add(curve);
                    }
                }
            }


            System.Windows.Forms.Clipboard.SetDataObject(curves);
            return Result.Succeeded;
        }
    }
}
