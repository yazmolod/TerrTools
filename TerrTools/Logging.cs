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
        public ElementProcessingLog(string operation, IEnumerable<ElementId> all, string type = "", string tip = "")
        {
            AllElementIds = all;
            Operation = operation;
            Tip = tip;
            ErrorType = type;
        }
        public ElementProcessingLog(string operation, IEnumerable<Element> all, string type = "", string tip = "")
        {
            AllElementIds = all.Select(x=>x.Id);
            Operation = operation;
            Tip = tip;
            ErrorType = type;
        }
        public IEnumerable<ElementId> AllElementIds { get; set; }
        public Stack<ElementId> FailedElementIds { get; set; } = new Stack<ElementId>();
        public string Operation { get; set; }
        public string ErrorType { get; set; }
        public string Tip { get; set; }
    }


    static class LoggingMachine
    {
        static private Stack<ElementProcessingLog> Stack { get; set; } = new Stack<ElementProcessingLog>();
        static public void Add(ElementProcessingLog error)
        {
            Stack.Push(error);
        }

        static public void Add(IEnumerable<ElementProcessingLog> errors)
        {
            foreach (var e in errors) Stack.Push(e);
        }

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
                string allErrorIds = String.Join(", ", error.FailedElementIds.Select(x => x.ToString()));
                TaskDialog dialog = new TaskDialog("Результат");
                dialog.MainInstruction = String.Format("Операция: {0}\nТип ошибки: {1}\nНеудачно: {2} из {3}", 
                    error.Operation, error.ErrorType, error.FailedElementIds.Count(), error.AllElementIds.Count());
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
        }
    }
}

