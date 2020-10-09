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
    class InsulCurves : IExternalCommand
    {
        Document doc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            doc = commandData.Application.ActiveUIDocument.Document;
            var elems = new FilteredElementCollector(doc).OfClass(typeof(MEPCurve)).WhereElementIsNotElementType().ToElements().Cast<MEPCurve>();
            using (Transaction tr = new Transaction(doc, "test"))
            {
                tr.Start();
                foreach (var e in elems)
                {
                    DrawInsulLine(e);
                }
                tr.Commit();
            }
            return Result.Succeeded;
        }

        private void DrawInsulLine(MEPCurve e)
        {
            Curve curve = GeometryUtils.FindDuctCurve(e.ConnectorManager);
            XYZ pt1 = curve.GetEndPoint(0); XYZ pt2 = curve.GetEndPoint(1);
            XYZ offset = (pt2 - pt1) / 10;
            List<XYZ> points = new List<XYZ>();
            points.Add(pt1);
            double y = 1;
            while (pt1.X < pt2.X)
            {
                pt1 = pt1 + offset;
                points.Add(new XYZ(pt1.X, pt1.Y + y, pt1.Z));
                y = -y;
            }
            ModelCurveCreator mcc = new ModelCurveCreator(doc);            
            mcc.MakeModelCurve(points, false);
        }
    }
}
