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
    class FamilyLoadOptions : IFamilyLoadOptions
    {
        public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
        {
            overwriteParameterValues = false;
            return true;
        }
        public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
        {
            source = FamilySource.Project;
            overwriteParameterValues = false;
            return true;
        }
    }


    [Transaction(TransactionMode.Manual)]
    class TypeChanger : IExternalCommand
    {
        virtual public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UpdateType(doc);
        
            return Result.Succeeded;
        }

        protected void UpdateType(Document doc)
        {
            ElementClassFilter filterTextType = new ElementClassFilter(typeof(TextElementType));
            ElementClassFilter filterTextNote = new ElementClassFilter(typeof(TextNoteType));
            ElementClassFilter filterDimension = new ElementClassFilter(typeof(DimensionType));
            LogicalOrFilter filter = new LogicalOrFilter(new ElementFilter[] { filterTextNote, filterTextType, filterDimension });
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

    [Transaction(TransactionMode.Manual)]
    class TypeChangerDeep : TypeChanger, IExternalCommand
    {
        override public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UpdateType(doc);

            Family[] families = new FilteredElementCollector(doc).OfClass(typeof(Family)).Cast<Family>().ToArray();
            foreach (Family family in families)
            {
                if (family.IsEditable && family.FamilyCategory.CategoryType == CategoryType.Annotation)
                {
                    Document famdoc = doc.EditFamily(family);
                    UpdateType(famdoc);
                    famdoc.LoadFamily(doc, new FamilyLoadOptions());
                }
            }
            return Result.Succeeded;
        }
    }
}
