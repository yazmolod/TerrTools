using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace TerrTools
{
    class ElementProcessingLog
    {
        public ElementProcessingLog(string operation, IEnumerable<ElementId> all, string errorType = "", string tip = "")
        {
            AllElementIds = all.Select(x => x.ToString()) ;
            Operation = operation;
            Tip = tip;
            ErrorType = errorType;
        }

        public ElementProcessingLog(string operation, string errorType = "", string tip = "")
        {
            Operation = operation;
            Tip = tip;
            ErrorType = errorType;
        }

        public ElementProcessingLog(string operation, IEnumerable<int> all, string errorType = "", string tip = "")
        {
            AllElementIds = all.Select(x => x.ToString());
            Operation = operation;
            Tip = tip;
            ErrorType = errorType;
        }

        public ElementProcessingLog(string operation, IEnumerable<Element> all, string errorType = "", string tip = "")
        {
            AllElementIds = all.Select(x=>x.Id.ToString());
            Operation = operation;
            Tip = tip;
            ErrorType = errorType;
        }

        public ElementProcessingLog(string operation, IEnumerable<string> all, string errorType = "", string tip = "")
        {
            AllElementIds = all;
            Operation = operation;
            Tip = tip;
            ErrorType = errorType;
        }

        public void AddError(string item)
        {
            FailedElementIds.Push(item);
        }

        public void AddError(int item)
        {
            FailedElementIds.Push(item.ToString());
        }
        public void AddError(ElementId item)
        {
            FailedElementIds.Push(item.IntegerValue.ToString());
        }

        public IEnumerable<string> AllElementIds { get; set; }
        public Stack<string> FailedElementIds { get; set; } = new Stack<string>();
        public string Operation { get; set; }
        public string ErrorType { get; set; }
        public string Tip { get; set; }
    }


    static class LoggingMachine
    {
        static private Stack<ElementProcessingLog> Stack { get; set; } = new Stack<ElementProcessingLog>();

        static public void Reset()
        {
            Stack = new Stack<ElementProcessingLog>();
        }

        static public ElementProcessingLog NewLog(string operation, IEnumerable<ElementId> all, string type = "", string tip = "")
        {
            ElementProcessingLog log = new ElementProcessingLog(operation, all, type, tip);
            Stack.Push(log);
            return log;
        }

        static public ElementProcessingLog NewLog(string operation, string type = "", string tip = "")
        {
            ElementProcessingLog log = new ElementProcessingLog(operation, type, tip);
            Stack.Push(log);
            return log;
        }

        static public ElementProcessingLog NewLog(string operation, IEnumerable<int> all, string type = "", string tip = "")
        {
            ElementProcessingLog log = new ElementProcessingLog(operation, all, type, tip);
            Stack.Push(log);
            return log;
        }

        static public ElementProcessingLog NewLog(string operation, IEnumerable<string> all, string type = "", string tip = "")
        {
            ElementProcessingLog log = new ElementProcessingLog(operation, all, type, tip);
            Stack.Push(log);
            return log;
        }


        static public ElementProcessingLog NewLog(string operation, IEnumerable<Element> all, string type = "", string tip = "")
        {
            ElementProcessingLog log = new ElementProcessingLog(operation, all, type, tip);
            Stack.Push(log);
            return log;
        }

        static public void Show(bool showEmpty = false)
        {
            foreach (var error in Stack)
            {
                if (!showEmpty && error.FailedElementIds.Count() == 0) continue;
                string allErrorIds = String.Join(", ", error.FailedElementIds);
                TaskDialog dialog = new TaskDialog("Результат");
                if (error.AllElementIds != null)
                {
                    dialog.MainInstruction = String.Format("Операция: {0}\nТип ошибки: {1}\nНеудачно: {2} из {3}",
                        error.Operation, error.ErrorType, error.FailedElementIds.Count(), error.AllElementIds.Count());
                }
                else
                {
                    dialog.MainInstruction = String.Format("Операция: {0}\nТип ошибки: {1}\nНеудачно: {2}",
                        error.Operation, error.ErrorType, error.FailedElementIds.Count());
                }
                dialog.MainContent = "Перечень id элементов:\n"
                    + allErrorIds;
                dialog.FooterText = error.Tip;
                dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Скопировать ID элементов в буфер обмена");
                TaskDialogResult result = dialog.Show();
                if (result == TaskDialogResult.CommandLink1)
                {
                    System.Windows.Forms.Clipboard.SetText(allErrorIds);
                    TaskDialog.Show("Результат", "Данные успешно скопированы в буфер обмена");
                }
            }
            LoggingMachine.Reset();
        }
    }
}

