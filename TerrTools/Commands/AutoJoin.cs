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

    class AutoJoin : IExternalCommand
    {

        public UIDocument UIDoc { get; set; }
        public Document Doc { get => UIDoc.Document; }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDoc = commandData.Application.ActiveUIDocument;
            IList<Element> mostElementCollection;
            IList<Element> leastElementCollection;
            CollectElementsFromPairs(out mostElementCollection, out leastElementCollection);
            ConnectElements(mostElementCollection, leastElementCollection);
            return Result.Succeeded;
        }
        // Возвращает собранные и отсортированные по двум категориям коллекции элементов,
        // учитывая - какая из них больше, а какая - меньше(в целях оптимизации).
        private void CollectElementsFromPairs(out IList<Element> mostElementCollection, 
            out IList<Element> leastElementCollection)
        {
            IList<Element> temporaryElementCollection;
            mostElementCollection = new FilteredElementCollector(Doc)
                .OfCategory(BuiltInCategory.OST_StructuralColumns)
                .WhereElementIsNotElementType()
                .ToElements();
            leastElementCollection = new FilteredElementCollector(Doc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsNotElementType()
                .ToElements();
            if (leastElementCollection.Count > mostElementCollection.Count)
            {
                temporaryElementCollection = mostElementCollection;
                mostElementCollection = leastElementCollection;
                leastElementCollection = temporaryElementCollection;
            }
        }
        // Соединяет элементы двух категорий.
        private void ConnectElements(IList<Element> mostElementCollection,
            IList<Element> leastElementCollection)
        {
            foreach (var firstElementToJoin in leastElementCollection)
            {
                var boundingBox = firstElementToJoin.get_BoundingBox(null);
                var outline = new Outline(boundingBox.Min, boundingBox.Max);
                var filter = new BoundingBoxIntersectsFilter(outline);
                var collectorWithLeastElements = (FilteredElementCollector)leastElementCollection;
                var elementsToJoin = collectorWithLeastElements.WherePasses(filter);
                // Соединение элементов
                foreach (var secondElementToJoin in elementsToJoin)
                {
                    JoinGeometryUtils.JoinGeometry(Doc, firstElementToJoin, secondElementToJoin);
                }
            }
        }
    }

}
