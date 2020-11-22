using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using TerrTools.UI;

namespace TerrTools
{
    [Transaction(TransactionMode.Manual)]
    class AutoJoin : IExternalCommand
    {
        public UIDocument UIDoc { get; set; }
        public Document Doc { get => UIDoc.Document; }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDoc = commandData.Application.ActiveUIDocument;
            List<Element> mostElementList;
            List<Element> leastElementList;
            CollectElementsFromPairs(out mostElementList, out leastElementList);
            ConnectElements(mostElementList, leastElementList);
            return Result.Succeeded;
        }
        // Возвращает собранные и отсортированные по двум категориям коллекции элементов,
        // учитывая - какая из них больше, а какая - меньше(в целях оптимизации).
        private void CollectElementsFromPairs(out List<Element> mostElementCollection,
            out List<Element> leastElementCollection)
        {
            List<Element> temporaryElementCollection;
            mostElementCollection = new FilteredElementCollector(Doc)
                .OfCategory(BuiltInCategory.OST_StructuralColumns)
                .WhereElementIsNotElementType()
                .ToList();
            leastElementCollection = new FilteredElementCollector(Doc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsNotElementType()
                .ToList();
            if (leastElementCollection.Count() > mostElementCollection.Count())
            {
                temporaryElementCollection = mostElementCollection;
                mostElementCollection = leastElementCollection;
                leastElementCollection = temporaryElementCollection;
            }
        }
        // Соединяет элементы двух категорий.
        private void ConnectElements(List<Element> mostElementsList,
            List<Element> leastElementsList)
        {
            using (Transaction trans = new Transaction(Doc))
            {
                trans.Start("Автоматическое присоединение элементов");
                foreach (var firstElementToJoin in leastElementsList)
                {
                    var boundingBox = firstElementToJoin.get_BoundingBox(null);
                    var outline = new Outline(boundingBox.Min, boundingBox.Max);
                    var filter = new BoundingBoxIntersectsFilter(outline);
                    foreach (var secondElementToJoin in mostElementsList)
                    {
                        if (filter.PassesFilter(secondElementToJoin))
                        {
                            try
                            {
                                JoinGeometryUtils.JoinGeometry(Doc, firstElementToJoin, secondElementToJoin);
                            }
                            // Исключение, которое происходит в том случае, когда элементы уже соединены.
                            // Не думаю, что имеет смысл уведомлять пользователей об этом.
                            catch (Autodesk.Revit.Exceptions.ArgumentException)
                            {
                            }
                        }
                    }
                }
                trans.Commit();
            }
        }
    }
}