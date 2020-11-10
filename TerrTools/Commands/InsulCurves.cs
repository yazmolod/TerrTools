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
    abstract class InsulCurves : IExternalCommand
    {
        protected Document doc { get; set; }
        protected UIDocument uidoc { get; set; }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
            string horFamilyName = "ТеррНИИ_УГО_3D_Теплоизоляция зигзаг";
            string verFamilyName = "ТеррНИИ_УГО_3D_Теплоизоляция зигзаг_Вертикальный";
            string familyFolder = @"L:\REVIT\Семейства\ТеррНИИ\Условники\3D";
            string familyType = "-";

            // получаем объекты для штриховки и задаем параметры
            var elems = GetElements();
            if (elems == null) return Result.Cancelled;
            using (Transaction tr = new Transaction(doc))
            {
                tr.Start("Загрузка семейства");
                FamilySymbol horZigzagSymbol = FamilyInstanceUtils.FindOpeningFamily(doc, horFamilyName, familyFolder, familyType);
                FamilySymbol verZigzagSymbol = FamilyInstanceUtils.FindOpeningFamily(doc, verFamilyName, familyFolder, familyType);
                tr.Commit();

                tr.Start("Добавление общих параметров");
                var systemParamName = "ТеррНИИ_Наименование системы";
                var idParamName = "ТеррНИИ_Идентификатор";
                // добавляем общие параметры, в который будем копировать марку и систему

                SharedParameterUtils.AddSharedParameter(doc, systemParamName,
                                                        new BuiltInCategory[] { BuiltInCategory.OST_GenericModel },
                                                        BuiltInParameterGroup.PG_TEXT);
                SharedParameterUtils.AddSharedParameter(doc, idParamName,
                                                        new BuiltInCategory[] { BuiltInCategory.OST_GenericModel },
                                                        BuiltInParameterGroup.PG_ADSK_MODEL_PROPERTIES);

                tr.Commit();

                tr.Start("Рисование штриховок");
                // проверяем какие штриховки уже есть в проекте и удаляем если есть обновляемые 
                List<ElementId> toDelete = new List<ElementId>();
                List<string> elemsIds = elems.Select(x => x.Id.IntegerValue.ToString()).ToList();
                List<FamilyInstance> fills = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance))
                                                    .Cast<FamilyInstance>().Where(x => x.Symbol.FamilyName == horFamilyName || x.Symbol.FamilyName == verFamilyName).ToList();
                foreach (FamilyInstance i in fills)
                {
                    var param = i.LookupParameter(idParamName);
                    if (param != null && elemsIds.Contains(param.AsString()))
                    {
                        toDelete.Add(i.Id);
                    }
                }
                doc.Delete(toDelete);

                foreach (MEPCurve e in elems)
                {
                    Curve ductCurve = (e.Location as LocationCurve).Curve;
                    Level level = e.ReferenceLevel;

                    var dir = GeometryUtils.GetDuctOrientation(e);

                    XYZ pt1 = ductCurve.GetEndPoint(0);
                    XYZ pt2 = ductCurve.GetEndPoint(1);
                    XYZ project_pt1 = new XYZ(pt1.X, pt1.Y, level.Elevation);
                    XYZ project_pt2 = new XYZ(pt2.X, pt2.Y, level.Elevation);

                    FamilyInstance fi = null;
                    try
                    {
                        if (dir == GeometryUtils.DuctOrientation.Horizontal)
                        {
                            Line line = Line.CreateBound(project_pt1, project_pt2);
                            fi = doc.Create.NewFamilyInstance(line, horZigzagSymbol, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                            fi.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM).Set(e.LevelOffset);
                        }
                        else if (dir == GeometryUtils.DuctOrientation.StraightVertical)
                        {
                            fi = doc.Create.NewFamilyInstance(project_pt1, verZigzagSymbol, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                            fi.LookupParameter("Длина").Set(
                                pt1.DistanceTo(pt2)
                                );                            
                            var offset = e.LookupParameter("Нижняя отметка").AsDouble();
                            fi.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM).Set(offset);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    catch (Autodesk.Revit.Exceptions.ArgumentsInconsistentException)
                    {
                        continue;
                    }

                    // Вычисляем марку
                    string insul_type = e.get_Parameter(BuiltInParameter.RBS_REFERENCE_INSULATION_TYPE).AsString();
                    string insul_thickness = e.get_Parameter(BuiltInParameter.RBS_REFERENCE_INSULATION_THICKNESS).AsValueString();

                    // вычисляем наименование системы
                    var p = e.LookupParameter(systemParamName);
                    string element_system = p != null ? p.AsString() : "";

                    // вычисляем смещение
                    Parameter ductWidth = e.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM);
                    Parameter ductHeight = e.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM);
                    Parameter ductDiameter = e.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM);
                    Parameter pipeDiameter = e.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM);

                    double fillOffset = 0;
                    if (ductDiameter != null) fillOffset = ductDiameter.AsDouble() / 2;
                    else if (pipeDiameter != null) fillOffset = pipeDiameter.AsDouble() / 2;
                    else
                    {
                        fillOffset = ductWidth.AsDouble() > ductHeight.AsDouble() ? ductHeight.AsDouble() / 2 : ductWidth.AsDouble() / 2;
                    }

                    // Назначаем параметры
                    fi.LookupParameter("Марка").Set(insul_type + " " + insul_thickness);
                    fi.LookupParameter(systemParamName).Set(element_system);
                    //fi.LookupParameter("Смещение по Z").Set(fillOffset);
                    fi.LookupParameter(idParamName).Set(e.Id.IntegerValue.ToString());
                }
                tr.Commit();

            }
            return Result.Succeeded;
        }

        abstract public List<MEPCurve> GetElements();

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
    }


    [Transaction(TransactionMode.Manual)]
    class InsulCurvesDocument : InsulCurves, IExternalCommand
    {
        public override List<MEPCurve> GetElements()
        {
            var elems = new FilteredElementCollector(doc).OfClass(typeof(MEPCurve)).WhereElementIsNotElementType()
                        .Cast<MEPCurve>().Where(x => x.get_Parameter(BuiltInParameter.RBS_REFERENCE_INSULATION_THICKNESS).AsDouble() > 0)
                        .ToList();
            return elems;
        }
    }


    [Transaction(TransactionMode.Manual)]
    class InsulCurvesView : InsulCurves, IExternalCommand
    {
        public override List<MEPCurve> GetElements()
        {
            var elems = new FilteredElementCollector(doc, doc.ActiveView.Id).OfClass(typeof(MEPCurve)).WhereElementIsNotElementType()
                        .Cast<MEPCurve>().Where(x => x.get_Parameter(BuiltInParameter.RBS_REFERENCE_INSULATION_THICKNESS).AsDouble() > 0)
                        .ToList();
            return elems;
        }
    }


    [Transaction(TransactionMode.Manual)]
    class InsulCurvesSelection : InsulCurves, IExternalCommand
    {
        public override List<MEPCurve> GetElements()
        {
            try
            {
                var elems = uidoc.Selection
                    .PickObjects(ObjectType.Element, new InsulMepFilter(), "Выберите воздуховоды или трубопроводы для штриховки")
                    .Select(x => doc.GetElement(x.ElementId)).Cast<MEPCurve>().ToList();
                return elems;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException) { return null; }
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
