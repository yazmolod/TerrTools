using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;


namespace TerrTools
{
    [Transaction(TransactionMode.Manual)]
    public class SpaceNaming : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            FilteredElementCollector collector = new FilteredElementCollector(doc).OfClass(typeof(SpatialElement));
            Element[] spaces = (from e in collector.ToElements()
                                where e is Space
                                select e).ToArray();
            if (spaces.Length > 0)
            {
                using (Transaction tr = new Transaction(doc, "Перенос информации из помещений в пространства"))
                {
                    tr.Start();
                    foreach (Element space in spaces)
                    {
                        string name = space.get_Parameter(BuiltInParameter.SPACE_ASSOC_ROOM_NAME).AsString();
                        string number = space.get_Parameter(BuiltInParameter.SPACE_ASSOC_ROOM_NUMBER).AsString();
                        space.get_Parameter(BuiltInParameter.ROOM_NAME).Set(name);
                        space.get_Parameter(BuiltInParameter.ROOM_NUMBER).Set(number);
                    }
                    tr.Commit();
                }
                TaskDialog.Show("Результат", "Данные успешно скопированы");
                return Result.Succeeded;
            }
            else
            {
                message = "Не найдено ни одно пространство в текущем проекте";
                return Result.Failed;
            }            
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class PluntRoom : IExternalCommand    
    {
        Document doc { get; set; }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Стандарт
            doc = commandData.Application.ActiveUIDocument.Document;
            // Параметры для обновления
            string airflowParameterName = "ADSK_Расход воздуха";
            string spaceNumberParameterName = "ТеррНИИ_Номер помещения";            

            // Выбираем все элементы трубуемой категории
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            List<Element> ductTerminals = collector.OfCategory(BuiltInCategory.OST_DuctTerminal).WhereElementIsNotElementType().ToList();
            if (ductTerminals.Count < 1)
            {
                TaskDialog.Show("Ошибка", "Не найден ни один воздухораспределитель");
                return Result.Failed;
            }

            //Проверяем наличие необходимых параметров в проекте
            Parameter checkingParameter = ductTerminals[0].LookupParameter(spaceNumberParameterName);
            if (checkingParameter == null)
            {
                bool result = Static.AddSharedParameter(doc,
                    spaceNumberParameterName,
                    "TerrTools_General",
                    true,
                    new BuiltInCategory[] { BuiltInCategory.OST_DuctTerminal },
                    BuiltInParameterGroup.PG_IDENTITY_DATA);
                if (!result) return Result.Failed;
            }


            checkingParameter = ductTerminals[0].LookupParameter(airflowParameterName);
            if (checkingParameter == null)
            {
                bool result = Static.AddSharedParameter(doc,
                    airflowParameterName,
                    "ADSK_Main_MEP",
                    true,
                    new BuiltInCategory[] { BuiltInCategory.OST_DuctTerminal },
                    BuiltInParameterGroup.PG_MECHANICAL);
                if (!result) return Result.Failed;
            }

            // Назначаем номера помещений диффузорам
            List<Element> missingDucts = new List<Element>();
            using (Transaction tr = new Transaction(doc, "Назначить воздухораспределителям номера помещений"))
            {
                tr.Start();
                foreach (Element el in ductTerminals)
                {                    
                    Parameter p = el.LookupParameter(spaceNumberParameterName);
                    Space space = GetSpaceOfDuct(el);
                    if (space != null)
                    {
                        string roomNumber = space.LookupParameter("Номер").AsString();
                        p.Set(roomNumber);
                    }
                    else
                    {
                        p.Set("<Помещение не найдено!>");
                        missingDucts.Add(el);
                    }
                }
                tr.Commit();
            }

            // Назначаем расход диффузорам            
            using (Transaction tr = new Transaction(doc, "Задать расход диффузорам"))
            {
                tr.Start();
                foreach (Element el in ductTerminals)
                {
                    try
                    {
                        // Параметр номера помещения
                        Parameter spaceNumberParam = el.LookupParameter(spaceNumberParameterName);
                        // Параметр Расход воздуха
                        Parameter airflowParam = el.LookupParameter(airflowParameterName);
                        // Параметр "Классификация системы"
                        BuiltInParameter sysClassBuiltIn = BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM;
                        Parameter systemClassParam = el.get_Parameter(sysClassBuiltIn);
                        string systemClass = systemClassParam.AsString();
                        // Считаем количество диффузоров одной системы на пространство
                        int count = (from d in ductTerminals
                                     where d.LookupParameter(spaceNumberParameterName).AsString() == spaceNumberParam.AsString()
                                     && d.get_Parameter(sysClassBuiltIn).AsString() == systemClassParam.AsString()
                                     select d).Count();

                        // Находим пространство, в котором находится диффузор и достаем нужные значения
                        Space space = GetSpaceOfDuct(el);
                        if (space != null)
                        {
                            double value;
                            switch (systemClass)
                            {
                                case "Приточный воздух":
                                    value = space.get_Parameter(BuiltInParameter.ROOM_DESIGN_SUPPLY_AIRFLOW_PARAM).AsDouble();
                                    value /= count;
                                    airflowParam.Set(value);
                                    break;
                                case "Отработанный воздух":
                                    value = space.get_Parameter(BuiltInParameter.ROOM_DESIGN_EXHAUST_AIRFLOW_PARAM).AsDouble();
                                    value /= count;
                                    airflowParam.Set(value);
                                    break;
                                default:
                                    break;
                            }
                        }
                        else airflowParam.Set(0);
                    }
                    catch
                    {
                        TaskDialog.Show("Ошибка", string.Format("При попытке назначить расход элементу {0} возникла ошибка. Данный элемент пропущен", el.Id.ToString()));
                    }
                }
                tr.Commit();
            }            
            TaskDialog dialog = new TaskDialog("Результат");
            dialog.MainInstruction = String.Format("Количество воздухораспределителей, для которых найдено пространство: {0}\nНе найдено: {1}",ductTerminals.Count - missingDucts.Count, missingDucts.Count);
            dialog.MainContent = "Не найдены пространства для следующих воздухораспределителей:\n" + String.Join(", ", (from e in missingDucts select e.Id.ToString()));
            dialog.Show();
            return Result.Succeeded;
        }

        private Space GetSpaceOfDuct(Element el)
        {
            LocationPoint el_origin = el.Location as LocationPoint;
            XYZ el_originXYZ = el_origin.Point;
            Space space = doc.GetSpaceAtPoint(el_originXYZ);
            return space;
        }
    }
}
