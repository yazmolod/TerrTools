using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
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
        protected string spaceTemperatureParameterName = "ADSK_Температура в помещении";

        protected bool CheckDefaultSharedParameters()
        {
            //Проверяем наличие необходимых параметров в проекте
            bool done = true;        
            done &= SharedParameterUtils.AddSharedParameter(doc, spaceNumberParameterName, "TerrTools_General", true,
                    new BuiltInCategory[] { BuiltInCategory.OST_DuctTerminal, BuiltInCategory.OST_MechanicalEquipment }, BuiltInParameterGroup.PG_IDENTITY_DATA);

                done &= SharedParameterUtils.AddSharedParameter(doc, exhaustSystemParameterName, "ADSK_Secondary_MEP", true,
                    new BuiltInCategory[] { BuiltInCategory.OST_MEPSpaces }, BuiltInParameterGroup.PG_MECHANICAL_AIRFLOW);

                done &= SharedParameterUtils.AddSharedParameter(doc, supplySystemParameterName, "ADSK_Secondary_MEP", true,
                    new BuiltInCategory[] { BuiltInCategory.OST_MEPSpaces }, BuiltInParameterGroup.PG_MECHANICAL_AIRFLOW);

                done &= SharedParameterUtils.AddSharedParameter(doc, spaceTemperatureParameterName, "ADSK_Secondary_MEP", true,
                    new BuiltInCategory[] { BuiltInCategory.OST_MEPSpaces }, BuiltInParameterGroup.PG_ENERGY_ANALYSIS);

            /* 
             * Эти параметры должны быть внутри семейства, нет смысла назначать их всей категории

                done &= CustomSharedParameter.AddSharedParameter(doc, thermalPowerParameterName, "ADSK_Main_MEP", true,
                    new BuiltInCategory[] { BuiltInCategory.OST_MechanicalEquipment }, BuiltInParameterGroup.PG_MECHANICAL);

                done &= CustomSharedParameter.AddSharedParameter(doc, airflowParameterName, "ADSK_Main_MEP", true,
                    new BuiltInCategory[] { BuiltInCategory.OST_DuctTerminal }, BuiltInParameterGroup.PG_MECHANICAL_AIRFLOW);

            */

            done &= SharedParameterUtils.AddSharedParameter(doc, skipParameterName, "TerrTools_General", true,
                    new BuiltInCategory[] { BuiltInCategory.OST_DuctTerminal, BuiltInCategory.OST_MechanicalEquipment }, BuiltInParameterGroup.PG_ANALYSIS_RESULTS);

            return done;
        }

        protected void SetSpaces(List<FamilyInstance> allPlunts, ref List<FamilyInstance> failedPlunts)
        {
            // Назначаем номера помещений диффузорам
            failedPlunts = new List<FamilyInstance>();
            using (Transaction tr = new Transaction(doc, "Назначить воздухораспределителям номера помещений"))
            {
                tr.Start();
                foreach (FamilyInstance el in allPlunts)
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
                        failedPlunts.Add(el);
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
        private string suplySystemTypeName;
        private string exhaustSystemTypeName;

        private void UpdatePluntGeometry(List<FamilyInstance> allPlunts, ref List<FamilyInstance> failedPlunts)
        {
            /* Подбор размеров, скорее всего, выглядит крайне ручным и нелогичным,
             * но на этом настоял Игорь. Возможно, имеет смысл переписать эту функцию в будущем
             */
            using (Transaction tr = new Transaction(doc, "Геометрия воздухораспределителей"))
            {
                tr.Start();

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

                foreach (var plunt in allPlunts)
                {
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
                                else if (airflow <= 250) diamParam.Set(UnitUtils.ConvertToInternalUnits(200, DisplayUnitType.DUT_MILLIMETERS));
                                else failedPlunts.Add(plunt);
                            }
                            // Вытяжка
                            else if (connector.DuctSystemType == DuctSystemType.ExhaustAir)
                            {
                                if (airflow <= 80) diamParam.Set(UnitUtils.ConvertToInternalUnits(100, DisplayUnitType.DUT_MILLIMETERS));
                                else if (airflow <= 125) diamParam.Set(UnitUtils.ConvertToInternalUnits(125, DisplayUnitType.DUT_MILLIMETERS));
                                else if (airflow <= 160) diamParam.Set(UnitUtils.ConvertToInternalUnits(160, DisplayUnitType.DUT_MILLIMETERS));
                                else if (airflow <= 300) diamParam.Set(UnitUtils.ConvertToInternalUnits(200, DisplayUnitType.DUT_MILLIMETERS));
                                else failedPlunts.Add(plunt);
                            }
                            break;

                        // Прямоугольная решетка
                        case ConnectorProfileType.Rectangular:
                            Parameter lengthParam = plunt.LookupParameter("ADSK_Размер_Ширина");
                            Parameter heightParam = plunt.LookupParameter("ADSK_Размер_Высота");
                            bool found = false;
                            foreach (int[] size in rectOpts)
                            {
                                double F = size[0] * size[1] * 0.68 / 1000000;
                                double V = airflow / (3600 * F);
                                if (V < 3)
                                {
                                    lengthParam.Set(UnitUtils.ConvertToInternalUnits(size[0], DisplayUnitType.DUT_MILLIMETERS));
                                    heightParam.Set(UnitUtils.ConvertToInternalUnits(size[1], DisplayUnitType.DUT_MILLIMETERS));
                                    found = true;
                                    break;
                                }
                            }
                            if (!found) failedPlunts.Add(plunt);
                            break;

                        default: 
                            failedPlunts.Add(plunt); 
                            break;
                    }
                }
                tr.Commit();
            }
        }

        private void UpdatePluntData(List<FamilyInstance> allPlunts, ref List<FamilyInstance> failedPlunts)
        {
            using (Transaction tr = new Transaction(doc, "Данные воздухораспределителей"))
            {
                tr.Start();
                foreach (FamilyInstance plunt in allPlunts)
                {
                    try
                    {
                        // Параметр номера помещения
                        Parameter spaceNumberParam = plunt.LookupParameter(spaceNumberParameterName);
                        // Параметр Расход воздуха
                        Parameter airflowParam = plunt.LookupParameter(airflowParameterName);
                        // Параметр "Классификация системы"
                        BuiltInParameter sysTypeBuiltIn = BuiltInParameter.RBS_DUCT_SYSTEM_TYPE_PARAM;
                        Parameter systemTypeParam = plunt.get_Parameter(sysTypeBuiltIn);
                        string systemType = systemTypeParam.AsValueString();
                        // Считаем количество диффузоров одной системы на пространство
                        int count = (from d in allPlunts
                                     where d.LookupParameter(spaceNumberParameterName).AsString() == spaceNumberParam.AsString()
                                     && d.get_Parameter(sysTypeBuiltIn).AsValueString() == systemType
                                     select d).Count();

                        // Находим пространство, в котором находится диффузор и достаем нужные значения
                        Space space = GetSpaceOfPlant(plunt);
                        if (space != null)
                        {
                            // Задаем расход диффузорам
                            double value;
                            if (systemType == suplySystemTypeName)
                            {
                                value = space.get_Parameter(BuiltInParameter.ROOM_DESIGN_SUPPLY_AIRFLOW_PARAM).AsDouble();
                                value /= count;
                                airflowParam.Set(value);
                            }
                            else if (systemType == exhaustSystemTypeName)
                            {
                                value = space.get_Parameter(BuiltInParameter.ROOM_DESIGN_EXHAUST_AIRFLOW_PARAM).AsDouble();
                                value /= count;
                                airflowParam.Set(value);
                            }
                        }
                        else failedPlunts.Add(plunt);                        
                    }
                    catch
                    {
                        failedPlunts.Add(plunt);
                    }
                }
                tr.Commit();
            }
        }

        private void UpdatePluntSystem(List<FamilyInstance> allPlunts, ref List<FamilyInstance> failedPlunts)
        {
            using (Transaction tr = new Transaction(doc, "Система воздухораспределителей"))
            {
                tr.Start();
                foreach (FamilyInstance plunt in allPlunts)
                {
                    Space space = GetSpaceOfPlant(plunt);
                    string systemName = null;
                    if (space != null)
                    {
                        string systemTypeName = plunt.get_Parameter(BuiltInParameter.RBS_DUCT_SYSTEM_TYPE_PARAM).AsValueString();
                        string systemClassName = plunt.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM).AsString();
                        if (systemTypeName == suplySystemTypeName) systemName = space.LookupParameter(supplySystemParameterName).AsString();
                        else if (systemTypeName == exhaustSystemTypeName) systemName = space.LookupParameter(exhaustSystemParameterName).AsString();
                        else if (systemTypeName == "Не определено")
                        {
                            if (systemClassName == "Приточный воздух") systemName = space.LookupParameter(supplySystemParameterName).AsString();                            
                            else if (systemClassName == "Отработанный воздух")systemName = space.LookupParameter(exhaustSystemParameterName).AsString();                            
                        }
                        if (systemName != null)
                        {
                            ConnectorSet connectors = plunt.MEPModel.ConnectorManager.Connectors;
                            Connector baseConnector = connectors.Cast<Connector>().FirstOrDefault();
                            MEPSystem fromSystem = baseConnector.MEPSystem;
                            MEPSystem toSystem = new FilteredElementCollector(doc).OfClass(typeof(MEPSystem)).Cast<MEPSystem>().Where(x => x.Name == systemName).FirstOrDefault();                           
                            if (fromSystem != null) fromSystem.Remove(connectors);
                            if (toSystem == null)
                            {
                                DuctSystemType ductType = DuctSystemType.UndefinedSystemType;
                                if (systemTypeName == suplySystemTypeName) ductType = DuctSystemType.SupplyAir;
                                else if (systemTypeName == exhaustSystemTypeName) ductType = DuctSystemType.ExhaustAir;
                                toSystem = doc.Create.NewMechanicalSystem(null, connectors, ductType);
                                toSystem.Name = systemName;
                            }
                            else toSystem.Add(connectors);
                        }
                        else failedPlunts.Add(plunt);
                    }
                    else failedPlunts.Add(plunt);
                }
                tr.Commit();
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
            // Обозначаем, где какая система
            string[] systemTypes = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_DuctSystem).WhereElementIsElementType().Select(x => x.Name).ToArray();
            suplySystemTypeName = new UI.OneComboboxForm("Выберите систему приточного воздуха", systemTypes).SelectedItem;
            exhaustSystemTypeName = new UI.OneComboboxForm("Выберите систему вытяжного воздуха", systemTypes).SelectedItem;


            List<FamilyInstance> missingPlunts = new List<FamilyInstance>();
            SetSpaces(allPlunts, ref missingPlunts);
            UpdatePluntSystem(allPlunts, ref missingPlunts);
            UpdatePluntData(allPlunts, ref missingPlunts);           
            UpdatePluntGeometry(allPlunts, ref missingPlunts);

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
            SetSpaces(allPlunts, ref missingPlunts);
            foreach (FamilyInstance el in allPlunts) FamilyInstanceUtils.MirroredIndicator(el);

            // Назначаем расход диффузорам            
            using (Transaction tr = new Transaction(doc, "Обновление данных радиатора"))
            {
                tr.Start();
                foreach (FamilyInstance el in allPlunts)
                {
                    UpdatePluntData(el);
                }
                tr.Commit();
            }

            using (Transaction tr = new Transaction(doc, "Подбор размера"))
            {
                tr.Start();
                foreach (FamilyInstance el in allPlunts)
                {
                    UpdatePluntGeometry(el, allPlunts);
                }
                tr.Commit();
            }

            ShowResultDialog(allPlunts, missingPlunts);
            return Result.Succeeded;
        }

        private bool UpdatePluntData(FamilyInstance el)
        {
            // находим параметры для переопределения
            Parameter pluntTi = el.LookupParameter("ADSK_Температура в помещении");
            Parameter pluntTp = el.LookupParameter("ТеррНИИ_Температура обратки");
            Parameter pluntTz = el.LookupParameter("ТеррНИИ_Температура подачи");

            // Температуру помещения определяем из свойств пространства
            Space space = GetSpaceOfPlant(el);
            double spaceTi = space != null ? space.LookupParameter(spaceTemperatureParameterName).AsDouble() : 0;

            // Температуру подачи и обратки определяем из свойств подключенных систем
            ConnectorManager connMng = el.MEPModel.ConnectorManager;
            if (connMng == null) return false;
            Connector connTz = connMng.Connectors.Cast<Connector>().Where(x => x.PipeSystemType == PipeSystemType.SupplyHydronic && x.MEPSystem != null).FirstOrDefault();
            double systemTz = connTz != null ? ((PipingSystemType)doc.GetElement(connTz.MEPSystem.GetTypeId())).FluidTemperature : 10 + 273.15;
            Connector connTp = connMng.Connectors.Cast<Connector>().Where(x => x.PipeSystemType == PipeSystemType.ReturnHydronic && x.MEPSystem != null).FirstOrDefault();
            double systemTp = connTp != null ? ((PipingSystemType)doc.GetElement(connTp.MEPSystem.GetTypeId())).FluidTemperature : 5 + 273.15;

            // Назначаем температуру
            if (pluntTi != null && pluntTp != null && pluntTz != null)
            {
                pluntTi.Set(spaceTi);
                pluntTz.Set(systemTz);
                pluntTp.Set(systemTp);
                return true;
            }
            else return false;
        }

        protected bool UpdatePluntGeometry(FamilyInstance el, List<FamilyInstance> allPlunts)
        {
            // создаем массив со всеми размерами
            int[] lengths = { 400, 500, 600, 700, 800, 900, 1000, 1100, 1200, 1400, 1600, 1800, 2000, 2300, 2600, 3000 };
            int[] heights = { 300, 400, 450, 500, 550, 600, 900 };            

            Space space = GetSpaceOfPlant(el);
            if (space == null) return false;
            int count = allPlunts.Where(x => x.LookupParameter("ТеррНИИ_Номер помещения").AsString() == space.get_Parameter(BuiltInParameter.ROOM_NUMBER).AsString()).Count();
            double requiredValue = space.get_Parameter(BuiltInParameter.ROOM_DESIGN_HEATING_LOAD_PARAM).AsDouble() / count;
            requiredValue = UnitUtils.ConvertFromInternalUnits(requiredValue, DisplayUnitType.DUT_WATTS);

            FamilySizeTableManager sizeMng = FamilySizeTableManager.GetFamilySizeTableManager(doc, el.Symbol.Family.Id);
            Parameter tableNameParam = el.Symbol.LookupParameter("Таблица выбора");
            Parameter TzParam = el.LookupParameter("ТеррНИИ_Температура подачи");
            Parameter TpParam = el.LookupParameter("ТеррНИИ_Температура обратки");
            Parameter TiParam = el.LookupParameter("ADSK_Температура в помещении");
            Parameter hParam = el.LookupParameter("ADSK_Размер_Высота");
            Parameter lParam = el.LookupParameter("ADSK_Размер_Длина");
            Parameter radType = el.LookupParameter("Тип радиатора");
            if (hParam == null || lParam == null || radType == null || TzParam == null || 
                TpParam == null || TiParam == null || sizeMng == null || tableNameParam == null) return false;                       
            double Tz = UnitUtils.ConvertFromInternalUnits(TzParam.AsDouble(), DisplayUnitType.DUT_CELSIUS);
            double Tp = UnitUtils.ConvertFromInternalUnits(TpParam.AsDouble(), DisplayUnitType.DUT_CELSIUS);
            double Ti = UnitUtils.ConvertFromInternalUnits(TiParam.AsDouble(), DisplayUnitType.DUT_CELSIUS);
            FamilySizeTable sizeTable = sizeMng.GetSizeTable(el.Symbol.LookupParameter("Таблица выбора").AsString());

            double N, Qn, Tzn, Tpn, Tin, powerValue;

            foreach (int h in heights)
            {
                foreach (int l in lengths)
                {
                    N = double.Parse( FamilyInstanceUtils.SizeLookup(sizeTable, "N", new string[] { radType.AsValueString(), h.ToString() }), 
                        System.Globalization.CultureInfo.InvariantCulture);
                    Qn = double.Parse( FamilyInstanceUtils.SizeLookup(sizeTable, "Qn", new string[] { radType.AsValueString(), h.ToString() }),
                        System.Globalization.CultureInfo.InvariantCulture);
                    Tzn = double.Parse( FamilyInstanceUtils.SizeLookup(sizeTable, "Tz", new string[] { radType.AsValueString(), h.ToString() }),
                        System.Globalization.CultureInfo.InvariantCulture);
                    Tpn = double.Parse( FamilyInstanceUtils.SizeLookup(sizeTable, "Tp", new string[] { radType.AsValueString(), h.ToString() }),
                        System.Globalization.CultureInfo.InvariantCulture);
                    Tin = double.Parse( FamilyInstanceUtils.SizeLookup(sizeTable, "Ti", new string[] { radType.AsValueString(), h.ToString() }),
                        System.Globalization.CultureInfo.InvariantCulture);

                    powerValue = (l / 1000.0) * Qn * Math.Pow(
                        (
                            (Tz - Tp) 
                                / 
                            Math.Log((Tz - Ti) / (Tp - Ti))
                        )/(
                            (Tzn - Tpn) 
                                / 
                            Math.Log((Tzn - Tin) / (Tpn - Tin))
                        ), 
                        N
                        );                                        

                    if (powerValue >= requiredValue)
                    {
                        lParam.Set(UnitUtils.ConvertToInternalUnits(l, DisplayUnitType.DUT_MILLIMETERS));
                        hParam.Set(UnitUtils.ConvertToInternalUnits(h, DisplayUnitType.DUT_MILLIMETERS));
                        return true;
                    }
                }
            }
            return false;        
        }

        protected override List<FamilyInstance> GetPlunts()
        {
            // Выбираем все элементы трубуемой категории
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            List<FamilyInstance> plunts = collector
                .OfCategory(BuiltInCategory.OST_MechanicalEquipment)
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>()
                .Where(x => x.LookupParameter(skipParameterName).AsInteger() != 1)
                .ToList();
            return plunts;
        }
    }
}
