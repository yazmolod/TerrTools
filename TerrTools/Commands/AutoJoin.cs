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
    abstract class AutoJoin : IExternalCommand
    {
        public UIDocument UIDoc { get; set; }
        public Document Doc { get => UIDoc.Document; }
        abstract public BuiltInCategory leftCategory { get; }
        abstract public BuiltInCategory rightCategory { get; }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDoc = commandData.Application.ActiveUIDocument;
            List<Element> largeList;
            List<Element> smallList;
            CollectElementsFromPairs(out largeList, out smallList);
            ConnectElements(largeList, smallList);
            return Result.Succeeded;
        }
        // Возвращает собранные и отсортированные по двум категориям коллекции элементов,
        // учитывая - какая из них больше, а какая - меньше(в целях оптимизации).
        private void CollectElementsFromPairs(out List<Element> largeList, out List<Element> smallList)
        {
            List<Element> leftCollection = new FilteredElementCollector(Doc)
                .OfCategory(leftCategory)
                .WhereElementIsNotElementType()
                .ToList();
            List<Element> rightCollection = new FilteredElementCollector(Doc)
                .OfCategory(rightCategory)
                .WhereElementIsNotElementType()
                .ToList();
            if (leftCollection.Count() > rightCollection.Count())
            {
                largeList = leftCollection;
                smallList = rightCollection;
            }
            else
            {
                largeList = rightCollection;
                smallList = leftCollection;
            }
        }
        // Соединяет элементы двух категорий.
        private void ConnectElements(List<Element> largeList, List<Element> smallList)
        {
            var largeListIds = largeList.Select(x => x.Id).ToList();
            using (Transaction trans = new Transaction(Doc))
            {
                trans.Start("Соединение элементов");
                foreach (var firstElementToJoin in smallList)
                {
                    var boundingBox = firstElementToJoin.get_BoundingBox(null);
                    var outline = new Outline(boundingBox.Min, boundingBox.Max);
                    var filter = new BoundingBoxIntersectsFilter(outline);
                    var intersectedElements = new FilteredElementCollector(Doc, largeListIds).WherePasses(filter).ToElements();
                    foreach (var secondElementToJoin in intersectedElements)
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
                trans.Commit();
            }
        }
    }

    [Transaction(TransactionMode.Manual)]
    class AutoJoin_WallFloor : AutoJoin
    {
        public override BuiltInCategory leftCategory => BuiltInCategory.OST_Walls;

        public override BuiltInCategory rightCategory => BuiltInCategory.OST_Floors;
    }

    [Transaction(TransactionMode.Manual)]
    class AutoJoin_WallColumn : AutoJoin
    {
        public override BuiltInCategory leftCategory => BuiltInCategory.OST_Walls;

        public override BuiltInCategory rightCategory => BuiltInCategory.OST_StructuralColumns;
    }
}