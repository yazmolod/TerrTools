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
    class DummyClass : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            message = @"Это сообщение отладки. Если вы его видите, значит функция еще не реализована ¯\_(ツ)_/¯";
            return Result.Failed;
        }
    }
}
