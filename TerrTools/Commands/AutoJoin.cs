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
            FilteredElementCollector mostElementCollection;
            FilteredElementCollector leastElementCollection;
            CollectElementsFromPairs(out mostElementCollection, out leastElementCollection);
            ConnectElements(mostElementCollection, leastElementCollection);
            return Result.Succeeded;
        }
        // Возвращает собранные и отсортированные по двум категориям коллекции элементов,
        // учитывая - какая из них больше, а какая - меньше(в целях оптимизации).
        private void CollectElementsFromPairs(out FilteredElementCollector mostElementCollection,
            out FilteredElementCollector leastElementCollection)
        {

            FilteredElementCollector temporaryElementCollection;
            mostElementCollection = new FilteredElementCollector(Doc)
                .OfCategory(BuiltInCategory.OST_StructuralColumns)
                .WhereElementIsNotElementType();
            leastElementCollection = new FilteredElementCollector(Doc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsNotElementType();
            if (leastElementCollection.Count() > mostElementCollection.Count())
            {
                temporaryElementCollection = mostElementCollection;
                mostElementCollection = leastElementCollection;
                leastElementCollection = temporaryElementCollection;
            }
        }
        // Соединяет элементы двух категорий.
        private void ConnectElements(FilteredElementCollector mostElementCollection,
            FilteredElementCollector leastElementCollection)
        {
            Transaction trans = new Transaction(Doc);
            trans.Start("Автоматическое присоединение элементов");
            
            for (int i = 0; i < leastElementCollection.ToList().Count; i++)
            {
                var firstElementToJoin = leastElementCollection.ToElements()[i];
                var boundingBox = firstElementToJoin.get_BoundingBox(null);
                var outline = new Outline(boundingBox.Min, boundingBox.Max);
                var filter = new BoundingBoxIntersectsFilter(outline);
                var elementsToJoin = mostElementCollection.WherePasses(filter).ToElements();
                for (int t = 0; t < elementsToJoin.Count; t++)
                {
                    var secondElementToJoin = elementsToJoin[t];
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
            trans.Commit();
            /*foreach (var firstElementToJoinId in leastElementCollection.ToElementIds())
            {
                var firstElementToJoin = Doc.GetElement(firstElementToJoinId);
                var boundingBox = firstElementToJoin.get_BoundingBox(null);
                //
                var outline = new Outline(boundingBox.Min, boundingBox.Max);
                //
                var filter = new BoundingBoxIntersectsFilter(outline);
                // элементы из наибольшей коллекции для присоединения, которые проходят фильтр
                var elementsToJoin = mostElementCollection.WherePasses(filter);
                // Соединение элементов
                foreach (var secondElementToJoinId in elementsToJoin.ToElementIds())
                {
                    var secondElementToJoin = Doc.GetElement(secondElementToJoinId);
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
            }*/
            /*trans.Commit();*/
        }
    }

}