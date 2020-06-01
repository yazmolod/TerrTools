using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Forms = System.Windows.Forms;
using IronPython.Hosting;

namespace TerrTools
{
    [Transaction(TransactionMode.Manual)]
    class PythonExecuter : IExternalCommand
    {
        private UIApplication __revit__;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            __revit__ = commandData.Application;

            Forms.OpenFileDialog dialog = new Forms.OpenFileDialog();
            dialog.Filter = "Python files (*.py)|*.py";
            dialog.Multiselect = false;
            if (dialog.ShowDialog() == Forms.DialogResult.OK)
            {
                try
                {
                    RunPythonScript(dialog.FileName);
                    TaskDialog.Show("Python execute", "Скрипт успешно исполнен");
                    return Result.Succeeded;
                }
                catch (Exception e)
                {
                    var td = new TaskDialog("Python execute");
                    td.MainInstruction = "При исполнении скрипта произошла ошибка";
                    td.MainContent = e.ToString();
                    td.Show();
                    return Result.Failed;
                }
            }
            else return Result.Cancelled;
        }

        private void RunPythonScript(string filepath)
        {
            var engine = Python.CreateEngine();
            var source = engine.CreateScriptSourceFromFile(filepath);

            //Execute
            var scope = engine.CreateScope();
            scope.SetVariable("__revit__", __revit__);

            source.Execute(scope);
        }
    }
}
