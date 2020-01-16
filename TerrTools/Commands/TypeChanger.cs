using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Architecture;


namespace TerrTools
{
    [Transaction(TransactionMode.Manual)]
    class TypeChanger : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            FilteredElementCollector families = new FilteredElementCollector(doc).OfClass(typeof(Family));
            foreach (Family family in families)
            {
                if (family.IsEditable)
                {
                    Document famdoc = doc.EditFamily(family);
                }
            }

            UpdateType(doc);


            
            return Result.Succeeded;
        }

        private void UpdateType(Document doc)
        {
            ElementClassFilter filterTextType = new ElementClassFilter(typeof(TextElementType));
            ElementClassFilter filterTextNote = new ElementClassFilter(typeof(TextNoteType));
            LogicalOrFilter filter = new LogicalOrFilter(filterTextNote, filterTextType);
            Element[] textTypes = new FilteredElementCollector(doc).WherePasses(filter).ToArray();
            using (Transaction tr = new Transaction(doc, "Обновление шрифта"))
            {
                tr.Start();
                foreach (Element type in textTypes)
                {
                    type.get_Parameter(BuiltInParameter.TEXT_FONT).Set("GOST Common");
                    type.get_Parameter(BuiltInParameter.TEXT_STYLE_ITALIC).Set(1);
                    type.get_Parameter(BuiltInParameter.TEXT_WIDTH_SCALE).Set(0.8);

                }
                tr.Commit();
            }
        }
    }
}
