using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Forms = System.Windows.Forms;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System.Reflection;
using System.Collections.Generic;
using System.IO;

namespace TerrTools
{
    [Transaction(TransactionMode.Manual)]
    public class PythonExecuter : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication app = commandData.Application;
            Forms.OpenFileDialog dialog = new Forms.OpenFileDialog();
            dialog.Filter = "Python files (*.py)|*.py";
            dialog.Multiselect = false;
            if (dialog.ShowDialog() == Forms.DialogResult.OK)
            {
                try
                {
                    Dictionary<string, object> input = new Dictionary<string, object>();
                    input.Add("__revit__", app);
                    input.Add("uidoc", app.ActiveUIDocument);
                    input.Add("doc", app.ActiveUIDocument.Document);
                    RunPythonScriptFromFile(dialog.FileName);
                    TaskDialog.Show("Python execute", "Скрипт исполнен");
                    return Result.Succeeded;
                }
                catch (Exception e)
                {
                    var td = new TaskDialog("Python execute");
                    td.MainInstruction = "При исполнении скрипта произошла необработанная ошибка";
                    td.MainContent = e.ToString();
                    td.Show();
                    return Result.Failed;
                }
            }
            else return Result.Cancelled;
        }

        static dynamic ExecuteScript(string scriptString, object[] input)
        {
            ScriptEngine engine = Python.CreateEngine();
            dynamic scope = engine.CreateScope();
            if (input != null)
            {
                scope.SetVariable("INPUT", input);
            }
            engine.Execute(scriptString, scope);
            try
            {
                var result = scope.OUTPUT;
                return result;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Выполнение скрипта Python, который хранится в ресурсах сборки
        /// </summary>
        /// <param name="resourcePath">Путь к ресурсу</param>
        /// <param name="input">Переменные для инициализации в скрипте в виде словаря "название переменной" - "объект переменной". По умолчанию null</param>
        /// <returns>значение переменной OUTPUT в скрипте; если она отсутствует - null</returns>
        static public dynamic RunPythonScriptFromResource(string resourcePath, object[] input = null)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            StreamReader reader = new StreamReader(assembly.GetManifestResourceStream(resourcePath));
            string script = reader.ReadToEnd();
            return ExecuteScript(script, input);
        }

        /// <summary>
        /// Выполнение скрипта Python
        /// </summary>
        /// <param name="script">Текст скрипта</param>
        /// <param name="input">Переменные для инициализации в скрипте в виде словаря "название переменной" - "объект переменной". По умолчанию null</param>
        /// <returns>значение переменной OUTPUT в скрипте; если она отсутствует - null</returns>
        static public dynamic RunPythonScriptFromString(string script, object[] input = null)
        {
            return ExecuteScript(script, input);
        }

        /// <summary>
        /// Выполнение скрипта Python из файла
        /// </summary>
        /// <param name="filepath">Путь к файлу скрипта</param>
        /// <param name="input">Переменные для инициализации в скрипте в виде словаря "название переменной" - "объект переменной". По умолчанию null</param>
        /// <returns>значение переменной OUTPUT в скрипте; если она отсутствует - null</returns>
        static public dynamic RunPythonScriptFromFile(string filepath, object[] input = null)
        {
            string script = File.ReadAllText(filepath);
            return ExecuteScript(script, input);
        }
    }
}
