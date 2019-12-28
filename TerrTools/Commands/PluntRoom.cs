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
    class LinkedDocFilter : Autodesk.Revit.UI.Selection.ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            BuiltInCategory cat = (BuiltInCategory)elem.Category.Id.IntegerValue;
            if (cat == BuiltInCategory.OST_RvtLinks) return true;
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;            
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class PluntRoom : IExternalCommand    
    {               
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            Document linkedDoc = null;
            XYZ linkedOrigin = null;

            TwoStringsForm inputForm = new TwoStringsForm();
            BuiltInCategory category = inputForm.Category;
            string parameterName = inputForm.ParameterName;   
            if (category == BuiltInCategory.INVALID)
            {
                TaskDialog.Show("Ошибка", "Не найдена категория");
                return Result.Failed;
            }

            // Выбор связанного файла, в котором содержаися помещения с забитыми номерами
            try
            {
                Reference linkInstanceRef = uidoc.Selection.PickObject(
                    Autodesk.Revit.UI.Selection.ObjectType.Element,
                    new LinkedDocFilter(),
                    "Выберите документ, в котором содержатся помещения");
                RevitLinkInstance linkInstance = doc.GetElement(linkInstanceRef) as RevitLinkInstance;
                linkedDoc = linkInstance.GetLinkDocument();
                linkedOrigin = linkInstance.GetTransform().Origin;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }        

            // Выбираем все элементы трубуемой категории
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            List<Element> ductTerminals = collector.OfCategory(category).WhereElementIsNotElementType().ToList();
            if (ductTerminals.Count < 1)
            {
                TaskDialog.Show("Ошибка", "Не найден ни один элемент данной категории");
                return Result.Failed;
            }

            //Проверяем, существует ли он в проекте
            Parameter checkingParameter = ductTerminals[0].LookupParameter(parameterName);
            if (checkingParameter == null)
            {
                TaskDialog.Show("Ошибка", String.Format("Не найден параметер {0} для требуемой категории", parameterName));
                return Result.Failed;
            }

            using (Transaction tr = new Transaction(doc, "Назначить воздухораспределителям номера помещений"))
            {
                tr.Start();
                List<string> missingDucts = new List<string>();
                foreach (Element el in ductTerminals)
                {
                    LocationPoint el_origin = el.Location as LocationPoint;
                    XYZ el_originXYZ = el_origin.Point;
                    Room room = linkedDoc.GetRoomAtPoint(el_originXYZ - linkedOrigin);
                    if (room != null)
                    {
                        string roomNumber = room.LookupParameter("Номер").AsString();
                        el.LookupParameter(parameterName).Set(roomNumber);
                    }
                    else
                    {
                        el.LookupParameter(parameterName).Set("<Помещение не найдено!>");
                        missingDucts.Add(el.Id.ToString());
                    }
                }
                TaskDialog.Show(String.Format("Не определено элементов: {0}", missingDucts.Count), String.Join("\n", missingDucts));
                tr.Commit();
            }
            return Result.Succeeded;
        }
    }
}
