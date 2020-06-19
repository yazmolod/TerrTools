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

    [Transaction(TransactionMode.Manual)]
    class DebuggingTools : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            BuiltInCategory[] allCats = new BuiltInCategory[]
            {
                BuiltInCategory.OST_DuctAccessory,
                BuiltInCategory.OST_PipeAccessory,
                BuiltInCategory.OST_DuctCurves,
                BuiltInCategory.OST_PlaceHolderDucts,
                BuiltInCategory.OST_DuctTerminal,
                BuiltInCategory.OST_FlexDuctCurves,
                BuiltInCategory.OST_FlexPipeCurves,
                BuiltInCategory.OST_DuctInsulations,
                BuiltInCategory.OST_PipeInsulations,
                BuiltInCategory.OST_PipeCurves,
                BuiltInCategory.OST_PlaceHolderPipes,
                BuiltInCategory.OST_DuctFitting,
                BuiltInCategory.OST_PipeFitting,
                BuiltInCategory.OST_MechanicalEquipment,
                BuiltInCategory.OST_Sprinklers,
                BuiltInCategory.OST_PlumbingFixtures,
                BuiltInCategory.OST_DuctSystem,
                BuiltInCategory.OST_PipingSystem,
            };

            string systemNameP = "ТеррНИИ_Наименование системы";
            var doc = commandData.Application.ActiveUIDocument.Document;
            using (Transaction tr = new Transaction(doc, "t"))
            {
                tr.Start();
                SharedParameterUtils.AddSharedParameter(doc,
                    systemNameP, allCats, BuiltInParameterGroup.PG_TEXT);
                tr.Commit();
            }
            return Result.Succeeded;
        }
    }
}
