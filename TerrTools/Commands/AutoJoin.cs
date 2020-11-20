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
            IList<Element> firstElementCollection;
            IList<Element> secondElementCollection;
            CollectElementsFromPairs(out firstElementCollection, out secondElementCollection);
            return Result.Succeeded;
        }
        // Возвращает собранные и отсортированные по двум категориям коллекции элементов.
        private void CollectElementsFromPairs(out IList<Element> firstElementCollection, 
            out IList<Element> secondElementCollection)
        {
            firstElementCollection = new FilteredElementCollector(Doc)
                .OfCategory(BuiltInCategory.OST_StructuralColumns)
                .WhereElementIsNotElementType()
                .ToElements();
            secondElementCollection = new FilteredElementCollector(Doc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsNotElementType()
                .ToElements();
        }
        private IList<Element> 
    }

}
