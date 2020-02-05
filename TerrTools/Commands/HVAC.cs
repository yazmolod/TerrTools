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
   
    abstract class PluntProcessing  
    {
        protected abstract Document doc { get; set; }
        //protected abstract void UpdateSize(FamilyInstance plunt);
        protected abstract List<FamilyInstance> GetPlunts();

        // Параметры для обновления
        protected string airflowParameterName = "ADSK_Расход воздуха";
        protected string exhaustSystemParameterName = "ADSK_Наименование вытяжной системы";
        protected string supplySystemParameterName = "ADSK_Наименование приточной системы";
        protected string spaceNumberParameterName = "ТеррНИИ_Номер помещения";
        protected string skipParameterName = "ТеррНИИ_Пропустить";
        protected string thermalPowerParameterName = "ADSK_Тепловая мощность";

        protected bool CheckDefaultSharedParameters()
        {
            //Проверяем наличие необходимых параметров в проекте
            bool done = true;        
            done &= CustomSharedParameter.AddSharedParameter(doc, spaceNumberParameterName, "TerrTools_General", true,
                    new BuiltInCategory[] { BuiltInCategory.OST_DuctTerminal, BuiltInCategory.OST_MechanicalEquipment }, BuiltInParameterGroup.PG_IDENTITY_DATA);
                done &= CustomSharedParameter.AddSharedParameter(doc, airflowParameterName, "ADSK_Main_MEP", true,
                    new BuiltInCategory[] { BuiltInCategory.OST_DuctTerminal }, BuiltInParameterGroup.PG_MECHANICAL_AIRFLOW);

                done &= CustomSharedParameter.AddSharedParameter(doc, exhaustSystemParameterName, "ADSK_Secondary_MEP", true,
                    new BuiltInCategory[] { BuiltInCategory.OST_MEPSpaces }, BuiltInParameterGroup.PG_MECHANICAL_AIRFLOW);

                done &= CustomSharedParameter.AddSharedParameter(doc, supplySystemParameterName, "ADSK_Secondary_MEP", true,
                    new BuiltInCategory[] { BuiltInCategory.OST_MEPSpaces }, BuiltInParameterGroup.PG_MECHANICAL_AIRFLOW);

                done &= CustomSharedParameter.AddSharedParameter(doc, thermalPowerParameterName, "ADSK_Main_MEP", true,
                    new BuiltInCategory[] { BuiltInCategory.OST_MechanicalEquipment }, BuiltInParameterGroup.PG_MECHANICAL);

                done &= CustomSharedParameter.AddSharedParameter(doc, skipParameterName, "TerrTools_General", true,
                    new BuiltInCategory[] { BuiltInCategory.OST_DuctTerminal, BuiltInCategory.OST_MechanicalEquipment }, BuiltInParameterGroup.PG_ANALYSIS_RESULTS);

            return done;
        }

        protected void SetSpaces(List<FamilyInstance> plunts, out List<FamilyInstance> missingPlunts)
        {
            // Назначаем номера помещений диффузорам
            missingPlunts = new List<FamilyInstance>();
            using (Transaction tr = new Transaction(doc, "Назначить воздухораспределителям номера помещений"))
            {
                tr.Start();
                foreach (FamilyInstance el in plunts)
                {
                    Parameter p = el.LookupParameter(spaceNumberParameterName);
                    Space space = GetSpaceOfPlant(el);
                    if (space != null)
                    {
                        string roomNumber = space.LookupParameter("Номер").AsString();
                        p.Set(roomNumber);
                    }
                    else
                    {
                        p.Set("<Помещение не найдено!>");
                        missingPlunts.Add(el);
                    }
                }
                tr.Commit();
            }
        }

        protected void ShowResultDialog(List<FamilyInstance> allPlunts, List<FamilyInstance> missingPlunts)
        {
            TaskDialog dialog = new TaskDialog("Результат");
            dialog.MainInstruction = String.Format("Количество элементов, для которых найдено пространство: {0}\nНе найдено: {1}", allPlunts.Count - missingPlunts.Count, missingPlunts.Count);
            dialog.MainContent = "Не найдены пространства для следующих элементов:\n"
                + String.Join(", ", (from e in missingPlunts select e.Id.ToString()).Take(20))
                + "...\n\nЕсли элемент находится в пространстве, но не определяется, проверьте точку расчета площади в семействе";
            dialog.Show();
        }

        protected Space GetSpaceOfPlant(FamilyInstance el)
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

    [Transaction(TransactionMode.Manual)]
    class DiffuserProcessing: PluntProcessing, IExternalCommand
    {
        protected override Document doc { get; set; }
        private string suplySystemName;
        private string exhaustSystemName;

        private void UpdateSize(FamilyInstance plunt)
        {
            /* Подбор размеров, скорее всего, выглядит крайне ручным и нелогичным,
             * но на этом настоял Игорь. Возможно, имеет смысл переписать эту функцию в будущем
             */

            // Перечень допустимых размеров
            int[][] rectOpts =
                {
                new int[] { 150, 100 },
                new int[] { 200, 100 },
                new int[] { 150, 150 },
                new int[] { 200, 150 },
                new int[] { 250, 150 },
                new int[] { 300, 150 },
                new int[] { 250, 200 },
                new int[] { 250, 250 },
                new int[] { 300, 250 },
                new int[] { 300, 300 },
                new int[] { 350, 300 },
                new int[] { 350, 350 }
                };
            int[] roundOpts = { 100, 125, 160, 200 };
            // достаем назначенный расход
            double airflow = plunt.LookupParameter("ADSK_Расход воздуха").AsDouble();
            airflow = UnitUtils.ConvertFromInternalUnits(airflow, DisplayUnitType.DUT_CUBIC_METERS_PER_HOUR);
            Connector connector = plunt.MEPModel.ConnectorManager.Connectors.Cast<Connector>().First();
            switch (connector.Shape)
            {
                // Круглый диффузоры
                case ConnectorProfileType.Round:
                    Parameter diamParam = plunt.LookupParameter("ADSK_Размер_Диаметр");
                    // Приточка
                    if (connector.DuctSystemType == DuctSystemType.SupplyAir)
                    {
                        if (airflow <= 50) diamParam.Set(UnitUtils.ConvertToInternalUnits(100, DisplayUnitType.DUT_MILLIMETERS));
                        else if (airflow <= 100) diamParam.Set(UnitUtils.ConvertToInternalUnits(125, DisplayUnitType.DUT_MILLIMETERS));
                        else if (airflow <= 160) diamParam.Set(UnitUtils.ConvertToInternalUnits(160, DisplayUnitType.DUT_MILLIMETERS));
                        else diamParam.Set(UnitUtils.ConvertToInternalUnits(200, DisplayUnitType.DUT_MILLIMETERS));
                    }
                    // Вытяжка
                    else if (connector.DuctSystemType == DuctSystemType.ExhaustAir)
                    {                        
                        if (airflow <= 80) diamParam.Set(UnitUtils.ConvertToInternalUnits(100, DisplayUnitType.DUT_MILLIMETERS));
                        else if (airflow <= 125) diamParam.Set(UnitUtils.ConvertToInternalUnits(125, DisplayUnitType.DUT_MILLIMETERS));
                        else if (airflow <= 160) diamParam.Set(UnitUtils.ConvertToInternalUnits(160, DisplayUnitType.DUT_MILLIMETERS));
                        else diamParam.Set(UnitUtils.ConvertToInternalUnits(200, DisplayUnitType.DUT_MILLIMETERS));
                    }
                    break;

                // Прямоугольная решетка
                case ConnectorProfileType.Rectangular:
                    Parameter lengthParam = plunt.LookupParameter("ADSK_Размер_Ширина");
                    Parameter heightParam = plunt.LookupParameter("ADSK_Размер_Высота");
                    foreach (int[]size in rectOpts)
                    {
                        double F = size[0] * size[1] * 0.68 / 1000000;
                        double V = airflow / (3600 * F);
                        if (V < 3)
                        {
                            lengthParam.Set(UnitUtils.ConvertToInternalUnits(size[0], DisplayUnitType.DUT_MILLIMETERS));
                            heightParam.Set(UnitUtils.ConvertToInternalUnits(size[1], DisplayUnitType.DUT_MILLIMETERS));
                            break;
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        private void UpdatePluntData(FamilyInstance currentPlunt, List<FamilyInstance> allPlunts)
        {
            try
            {
                // Параметр номера помещения
                Parameter spaceNumberParam = currentPlunt.LookupParameter(spaceNumberParameterName);
                // Параметр Расход воздуха
                Parameter airflowParam = currentPlunt.LookupParameter(airflowParameterName);
                // Параметр "Классификация системы"
                BuiltInParameter sysTypeBuiltIn = BuiltInParameter.RBS_DUCT_SYSTEM_TYPE_PARAM;
                Parameter systemTypeParam = currentPlunt.get_Parameter(sysTypeBuiltIn);
                string systemType = systemTypeParam.AsValueString();
                // Считаем количество диффузоров одной системы на пространство
                int count = (from d in allPlunts
                             where d.LookupParameter(spaceNumberParameterName).AsString() == spaceNumberParam.AsString()
                             && d.get_Parameter(sysTypeBuiltIn).AsValueString() == systemType
                             select d).Count();

                // Находим пространство, в котором находится диффузор и достаем нужные значения
                Space space = GetSpaceOfPlant(currentPlunt);
                if (space != null)
                {
                    // Задаем расход диффузорам
                    double value;
                    if (systemType == suplySystemName)
                    {
                        value = space.get_Parameter(BuiltInParameter.ROOM_DESIGN_SUPPLY_AIRFLOW_PARAM).AsDouble();
                        value /= count;
                        airflowParam.Set(value);
                        space.LookupParameter(supplySystemParameterName).Set(currentPlunt.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM).AsString());
                    }
                    else if (systemType == exhaustSystemName)
                    {
                        value = space.get_Parameter(BuiltInParameter.ROOM_DESIGN_EXHAUST_AIRFLOW_PARAM).AsDouble();
                        value /= count;
                        airflowParam.Set(value);
                        space.LookupParameter(exhaustSystemParameterName).Set(currentPlunt.get_Parameter(BuiltInParameter.RBS_SYSTEM_NAME_PARAM).AsString());
                    }
                }
                else airflowParam.Set(0);
            }
            catch
            {
                TaskDialog.Show("Ошибка", string.Format("При попытке назначить расход элементу {0} возникла ошибка. Данный элемент пропущен", currentPlunt.Id.ToString()));
            }
        }

        protected override List<FamilyInstance> GetPlunts()
        {
            // Выбираем все элементы трубуемой категории
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            List<FamilyInstance> plunts = collector
                .OfCategory(BuiltInCategory.OST_DuctTerminal)
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>()
                .Where(x => x.LookupParameter(skipParameterName).AsInteger() != 1)
                .ToList();
            return plunts;
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Стандарт
            doc = commandData.Application.ActiveUIDocument.Document;
            if (!CheckDefaultSharedParameters()) return Result.Failed;

            List<FamilyInstance> allPlunts = GetPlunts();
            if (allPlunts.Count < 1)
            {
                TaskDialog.Show("Ошибка", "Не найден ни один воздухораспределитель");
                return Result.Failed;
            }

            List<FamilyInstance> missingPlunts = new List<FamilyInstance>();
            SetSpaces(allPlunts, out missingPlunts);

            // Обозначаем, где какая система
            string[] systemTypes = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_DuctSystem).WhereElementIsElementType().Select(x => x.Name).ToArray();
            suplySystemName = new UI.OneComboboxForm("Выберите систему приточного воздуха", systemTypes).SelectedItem;
            exhaustSystemName = new UI.OneComboboxForm("Выберите систему вытяжного воздуха", systemTypes).SelectedItem;

            // Назначаем расход диффузорам            
            using (Transaction tr = new Transaction(doc, "Обновление данных решеток"))
            {
                tr.Start();
                foreach (FamilyInstance el in allPlunts)
                {
                    UpdatePluntData(el, allPlunts);
                }
                tr.Commit();
            }

            // Задаем размер диффузорам            
            using (Transaction tr = new Transaction(doc, "Обновление геометрии решеток"))
            {
                tr.Start();
                foreach (FamilyInstance el in allPlunts)
                {
                    UpdateSize(el);
                }
                tr.Commit();
            }

            ShowResultDialog(allPlunts, missingPlunts);
            return Result.Succeeded;
        }        
    }

    [Transaction(TransactionMode.Manual)]
    class RadiatorProcessing : PluntProcessing, IExternalCommand
    {
        protected override Document doc { get; set; }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Стандарт
            doc = commandData.Application.ActiveUIDocument.Document;
            if (!CheckDefaultSharedParameters()) return Result.Failed;

            List<FamilyInstance> allPlunts = GetPlunts();
            if (allPlunts.Count < 1)
            {
                TaskDialog.Show("Ошибка", "Не найден ни один радиатор");
                return Result.Failed;
            }

            List<FamilyInstance> missingPlunts = new List<FamilyInstance>();
            SetSpaces(allPlunts, out missingPlunts);

            using (Transaction tr = new Transaction(doc, "Подбор размера"))
            {
                tr.Start();
                foreach (FamilyInstance el in allPlunts)
                {
                    UpdateSize(el);
                }
                tr.Commit();
            }

            ShowResultDialog(allPlunts, missingPlunts);
            return Result.Succeeded;
        }

        protected bool UpdateSize(FamilyInstance el)
        {
            // создаем массив со всеми размерами
            int[] lengths = { 400, 500, 600, 700, 800, 900, 1000, 1100, 1200, 1400, 1600, 1800, 2000, 2300, 2600, 3000 };
            int[] heights = { 300, 400, 450, 500, 550, 600, 900 };
            int[][] sizeOpts = new int[lengths.Length * heights.Length][];
            for (int i = 0; i < heights.Length; i++)
            {
                for (int j = 0; j < lengths.Length; j++)
                {
                    sizeOpts[i*lengths.Length + j] = new int[] { lengths[j], heights[i] };
                }
            }

            Space space = GetSpaceOfPlant(el);
            double requiredValue = space.get_Parameter(BuiltInParameter.ROOM_DESIGN_HEATING_LOAD_PARAM).AsDouble();

            Parameter valueParameter = el.LookupParameter("ADSK_Тепловая мощность");
            Parameter lengthParameter = el.LookupParameter("Длина радиатора");
            Parameter heightParameter = el.LookupParameter("Высота радиатора");

            using (Transaction tr = new Transaction(doc))
            {
                tr.Start();
                foreach (int[] size in sizeOpts)
                {
                    lengthParameter.Set(size[0] / 304.8);
                    heightParameter.Set(size[1] / 304.8);
                    if (valueParameter.AsDouble() > requiredValue) break;
                }
                if (valueParameter.AsDouble() < requiredValue)
                {
                    tr.RollBack();
                    TaskDialog.Show("Debug", "Не найден подходящий размер для");
                    return false;
                }
                else
                {
                    tr.Commit();
                    return true;
                }
            }
             
        }

        protected override List<FamilyInstance> GetPlunts()
        {
            // Выбираем все элементы трубуемой категории
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            List<FamilyInstance> plunts = collector
                .OfCategory(BuiltInCategory.OST_MechanicalEquipment)
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>()
                .Where(x => x.Symbol.FamilyName == "ТеррНИИ_Оборудование_Радиатор универсальный")
                .Where(x => x.LookupParameter(skipParameterName).AsInteger() != 1)
                .ToList();
            return plunts;
        }
    }
}
