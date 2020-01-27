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
            string exhaustSystemParameterName = "ADSK_Наименование вытяжной системы";
            string supplySystemParameterName = "ADSK_Наименование приточной системы";
            string spaceNumberParameterName = "ТеррНИИ_Номер помещения";
            string skipParameterName = "ТеррНИИ_Пропустить";           

            //Проверяем наличие необходимых параметров в проекте
            bool exist;
            exist = CustomSharedParameter.IsParameterInProject(doc, spaceNumberParameterName, BuiltInCategory.OST_DuctTerminal);
            if (!exist)
            {
                bool result = CustomSharedParameter.AddSharedParameter(doc, spaceNumberParameterName, "TerrTools_General", true,
                    new BuiltInCategory[] { BuiltInCategory.OST_DuctTerminal }, BuiltInParameterGroup.PG_IDENTITY_DATA);
                if (!result) return Result.Failed;
            }

            exist = CustomSharedParameter.IsParameterInProject(doc, airflowParameterName, BuiltInCategory.OST_DuctTerminal);
            if (!exist)
            {
                bool result = CustomSharedParameter.AddSharedParameter(doc, airflowParameterName, "ADSK_Main_MEP", true,
                    new BuiltInCategory[] { BuiltInCategory.OST_DuctTerminal }, BuiltInParameterGroup.PG_MECHANICAL_AIRFLOW);
                if (!result) return Result.Failed;
            }

            exist = CustomSharedParameter.IsParameterInProject(doc, exhaustSystemParameterName, BuiltInCategory.OST_MEPSpaces);
            if (!exist)
            {
                bool result = CustomSharedParameter.AddSharedParameter(doc, exhaustSystemParameterName, "ADSK_Secondary_MEP", true,
                    new BuiltInCategory[] { BuiltInCategory.OST_MEPSpaces}, BuiltInParameterGroup.PG_MECHANICAL_AIRFLOW);
                if (!result) return Result.Failed;
            }

            exist = CustomSharedParameter.IsParameterInProject(doc, supplySystemParameterName, BuiltInCategory.OST_MEPSpaces);
            if (!exist)
            {
                bool result = CustomSharedParameter.AddSharedParameter(doc, supplySystemParameterName, "ADSK_Secondary_MEP", true,
                    new BuiltInCategory[] { BuiltInCategory.OST_MEPSpaces}, BuiltInParameterGroup.PG_MECHANICAL_AIRFLOW);
                if (!result) return Result.Failed;
            }

            exist = CustomSharedParameter.IsParameterInProject(doc, skipParameterName,BuiltInCategory.OST_DuctTerminal);
            if (!exist)
            {
                bool result = CustomSharedParameter.AddSharedParameter(doc, skipParameterName, "TerrTools_General", true,
                    new BuiltInCategory[] { BuiltInCategory.OST_DuctTerminal }, BuiltInParameterGroup.PG_ANALYSIS_RESULTS);
                if (!result) return Result.Failed;
            }

            // Выбираем все элементы трубуемой категории
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            List<FamilyInstance> ductTerminals = collector
                .OfCategory(BuiltInCategory.OST_DuctTerminal)
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>()
                .Where(x => x.LookupParameter(skipParameterName).AsInteger() != 1)
                .ToList();
            if (ductTerminals.Count < 1)
            {
                TaskDialog.Show("Ошибка", "Не найден ни один воздухораспределитель");
                return Result.Failed;
            }

            // Назначаем номера помещений диффузорам
            List<Element> missingDucts = new List<Element>();
            using (Transaction tr = new Transaction(doc, "Назначить воздухораспределителям номера помещений"))
            {
                tr.Start();
                foreach (FamilyInstance el in ductTerminals)
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
                string[] systemTypes = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_DuctSystem).WhereElementIsElementType().Select(x => x.Name).ToArray();
                string suplySystemName = new UI.OneComboboxForm("Выберите систему приточного воздуха", systemTypes).SelectedItem;
                string exhaustSystemName = new UI.OneComboboxForm("Выберите систему вытяжного воздуха", systemTypes).SelectedItem;

                tr.Start();
                foreach (FamilyInstance el in ductTerminals)
                {
                    try
                    {
                        // Параметр номера помещения
                        Parameter spaceNumberParam = el.LookupParameter(spaceNumberParameterName);
                        // Параметр Расход воздуха
                        Parameter airflowParam = el.LookupParameter(airflowParameterName);
                        // Параметр "Классификация системы"
                        BuiltInParameter sysTypeBuiltIn = BuiltInParameter.RBS_DUCT_SYSTEM_TYPE_PARAM;
                        Parameter systemTypeParam = el.get_Parameter(sysTypeBuiltIn);
                        string systemType = systemTypeParam.AsValueString();
                        // Считаем количество диффузоров одной системы на пространство
                        int count = (from d in ductTerminals
                                     where d.LookupParameter(spaceNumberParameterName).AsString() == spaceNumberParam.AsString()
                                     && d.get_Parameter(sysTypeBuiltIn).AsValueString() == systemType
                                     select d).Count();

                        // Находим пространство, в котором находится диффузор и достаем нужные значения
                        Space space = GetSpaceOfDuct(el);
                        if (space != null)
                        {
                            // Задаем расход диффузорам
                            double value;
                            if (systemType == suplySystemName) {
                                value = space.get_Parameter(BuiltInParameter.ROOM_DESIGN_SUPPLY_AIRFLOW_PARAM).AsDouble();
                                value /= count;
                                airflowParam.Set(value);
                                space.LookupParameter(supplySystemParameterName).Set(el.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM).AsString());
                            }
                            else if (systemType == exhaustSystemName) {
                                value = space.get_Parameter(BuiltInParameter.ROOM_DESIGN_EXHAUST_AIRFLOW_PARAM).AsDouble();
                                value /= count;
                                airflowParam.Set(value);
                                space.LookupParameter(exhaustSystemParameterName).Set(el.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM).AsString());
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
            dialog.MainContent = "Не найдены пространства для следующих воздухораспределителей:\n" 
                + String.Join(", ", (from e in missingDucts select e.Id.ToString()).Take(20)) 
                + "...\n\nЕсли диффузор находится в пространстве, но не определяется, проверьте точку расчета площади в семействе";
            dialog.Show();
            return Result.Succeeded;
        }

        private Space GetSpaceOfDuct(FamilyInstance el)
        {
            //По свойству пространства диффузора
            Space space = el.Space;
            if (space != null) return space;

            //По точке расчета площади
            if (el.HasSpatialElementCalculationPoint)
            {
                XYZ point = el.GetSpatialElementCalculationPoint();
                space = doc.GetSpaceAtPoint(point);
                if (space != null) return space;
            }
            //По точке вставки экземпляра семества
            LocationPoint el_origin = el.Location as LocationPoint;
            XYZ el_originXYZ = el_origin.Point;
            space = doc.GetSpaceAtPoint(el_originXYZ);
            if (space != null) return space;

            //Ничего не прошло - возвращаем null
            return null;
        }
    }
}
