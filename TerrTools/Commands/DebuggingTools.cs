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
            string scriptString = @"
name = INPUT[0].Title
OUTPUT = name.upper()";

            Document doc = commandData.Application.ActiveUIDocument.Document;
            object[] input = new object[] { doc };

            var result = PythonExecuter.RunPythonScriptFromString(scriptString, input);
            TaskDialog.Show("WOW", result); 
            return Result.Succeeded;
        }
    }
}
