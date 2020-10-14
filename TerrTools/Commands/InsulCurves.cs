using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.Attributes;

namespace TerrTools
{
    [Transaction(TransactionMode.Manual)]
    class InsulCurves : IExternalCommand
    {
        Document doc;
        UIDocument uidoc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            // собираем типы линий
            var lineStyles = new FilteredElementCollector(doc).OfClass(typeof(GraphicsStyle)).Cast<GraphicsStyle>()
                .Where(x => x.GraphicsStyleCategory.Parent?.Id.IntegerValue == (int)BuiltInCategory.OST_Lines).ToList();
            // диалоговое окно
            var form = new UI.InsulCurvesForm(lineStyles);
            if (form.DialogResult == System.Windows.Forms.DialogResult.OK)
            {                
                // получаем объекты для штриховки и задаем параметры
                var elems = GetElements(form.ResultScope);
                var step = UnitUtils.ConvertToInternalUnits(form.ResultStep, DisplayUnitType.DUT_MILLIMETERS);
                var height = UnitUtils.ConvertToInternalUnits(form.ResultHeight, DisplayUnitType.DUT_MILLIMETERS);
                var style = form.ResultStyle;
                using (Transaction tr = new Transaction(doc, "Создание 3D штриховок"))
                {
                    tr.Start();
                    // проверяем какие штриховки уже есть в проекте и удаляем если есть обновляемые 
                    string dimension = form.ResultLine == UI.InsulCurvesForm.LineType.ModelLine ? "3D" : "2D";
                    IEnumerable<string> elemsIds = elems.Select(x => $"Зигзаг для id{x.Id.IntegerValue} {dimension}");
                    ICollection<ElementId> existedGroups = new FilteredElementCollector(doc).OfClass(typeof(Group))
                        .Where(x => elemsIds.Contains(x.Name)).Select(x => x.Id).ToList();
                    doc.Delete(existedGroups);

                    foreach (var e in elems)
                    {
                        var groupName = $"Зигзаг для id{e.Id.IntegerValue}";
                        // рисуем линии
                        var modelArray = DrawInsulLine(e, step, height, style);
                        // преобразуем в линии аннотации, если была выбрана такая опция
                        DetailCurveArray detailArray = null;
                        if (form.ResultLine == UI.InsulCurvesForm.LineType.DetailLine)
                        {
                            try
                            {
                                ModelCurveArray curvesArray = new ModelCurveArray();
                                foreach (var curve in modelArray)
                                {
                                    curvesArray.Append(curve);
                                }                                
                                detailArray = doc.ConvertModelToDetailCurves(doc.ActiveView, curvesArray);
                            }
                            catch (System.ArgumentException)
                            {
                                TaskDialog.Show("Ошибка", "На данном виде невозможно создать линии аннотации");
                                return Result.Failed;

                            }
                        }
                        // группируем линии для объекта
                        List<ElementId> curvesIds = new List<ElementId>();
                        if (detailArray != null)
                        {
                            groupName = groupName + " 2D";
                            foreach (DetailCurve item in detailArray)
                            {
                                curvesIds.Add(item.Id);
                            }
                        }
                        else if (modelArray != null)
                        {
                            groupName = groupName + " 3D";
                            curvesIds = modelArray.Select(x => x.Id).ToList();
                        }
                        Group group = doc.Create.NewGroup(curvesIds);
                        group.GroupType.Name = groupName;
                    }
                    tr.Commit();
                }
                return Result.Succeeded;
            }
            else
            {
                return Result.Cancelled;
            }            
        }

        private List<MEPCurve> GetElements(UI.InsulCurvesForm.ScopeType scope)
        {
            List<MEPCurve> elems = new List<MEPCurve>();
            switch (scope)
            {
                case UI.InsulCurvesForm.ScopeType.Document:
                    elems = new FilteredElementCollector(doc).OfClass(typeof(MEPCurve)).WhereElementIsNotElementType()
                        .Cast<MEPCurve>().Where(x => x.get_Parameter(BuiltInParameter.RBS_REFERENCE_INSULATION_THICKNESS).AsDouble() > 0)
                        .ToList();
                    break;
                case UI.InsulCurvesForm.ScopeType.Selection:
                    try
                    {
                        elems = uidoc.Selection
                            .PickObjects(ObjectType.Element, new InsulMepFilter(), "Выберите воздуховоды или трубопроводы для штриховки")
                            .Select(x=>doc.GetElement(x.ElementId)).Cast<MEPCurve>().ToList();
                    }
                    catch (Autodesk.Revit.Exceptions.OperationCanceledException) { }
                    break;
                case UI.InsulCurvesForm.ScopeType.View:
                    elems = new FilteredElementCollector(doc, doc.ActiveView.Id).OfClass(typeof(MEPCurve)).WhereElementIsNotElementType()
                        .Cast<MEPCurve>().Where(x => x.get_Parameter(BuiltInParameter.RBS_REFERENCE_INSULATION_THICKNESS).AsDouble() > 0)
                        .ToList();
                    break;
            }
            return elems;
        }

        private List<ModelCurve> DrawInsulLine(MEPCurve e, double stepLength, double height, GraphicsStyle style)
        {
            Curve ductCurve = GeometryUtils.FindDuctCurve(e.ConnectorManager);
            XYZ pt1 = ductCurve.GetEndPoint(0); XYZ pt2 = ductCurve.GetEndPoint(1);

            // создаем плоскость, на которой будет рисоваться штриховка
            XYZ VOffset;
            XYZ ductVector = GeometryUtils.GetDuctDirection(e.ConnectorManager);
            // Вертикальная линии
            if (ductVector.IsAlmostEqualTo(XYZ.BasisZ) | ductVector.IsAlmostEqualTo(XYZ.BasisZ.Negate())) 
            {
                // находим перпендикулярный вектор, нормализуем его и умножаем до нужной длины
                VOffset = (pt2 - pt1).CrossProduct(XYZ.BasisY).Normalize() * (height / 2);
            }
            else
            {
                VOffset = (pt2 - pt1).CrossProduct(XYZ.BasisZ).Normalize() * (height / 2);
            }
            
            XYZ UOffset = (pt2 - pt1).Normalize() * (stepLength / 2);
            int steps = (int)Math.Floor((pt2 - pt1).GetLength() / UOffset.GetLength());
            List<XYZ> points = new List<XYZ>();
            points.Add(pt1);
            for (int i = 0; i < steps; i++)
            {
                pt1 = pt1 + UOffset;
                XYZ VOffsetedPt = pt1 + VOffset;
                points.Add(VOffsetedPt);
                VOffset = VOffset.Negate();
            }
            points.Add(pt2);
            ModelCurveCreator mcc = new ModelCurveCreator(doc);
            mcc.LineStyle = style;
            if (points.Count() > 2)
            {
                var array = mcc.MakeModelCurve(points, false);
                return array;
            }
            else
            {
                return null;
            }
        }

        private class InsulMepFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                var p = elem.get_Parameter(BuiltInParameter.RBS_REFERENCE_INSULATION_THICKNESS);
                return elem is MEPCurve && p != null && p.AsDouble() > 0;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                throw new NotImplementedException();
            }
        }
    }
}
