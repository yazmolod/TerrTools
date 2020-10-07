using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.Windows.Forms;

namespace TerrTools
{
    [Transaction(TransactionMode.Manual)]
    class CollisionViewer : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Отчет о коллизиях (*.html)|*.html";
            dialog.Multiselect = false;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string path = dialog.FileName;
                var table = new CollisionReportTable(path);
                return Result.Succeeded;
            }
            else
            {
                return Result.Cancelled;
            }            
        }
    }
}
