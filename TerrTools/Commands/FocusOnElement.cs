using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace TerrTools
{
    [Transaction(TransactionMode.Manual)]
    class FocusOnElement : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UI.ElementFilterForm form = new UI.ElementFilterForm(commandData);
            form.Show();
            return Result.Succeeded;
        }
    }
}
