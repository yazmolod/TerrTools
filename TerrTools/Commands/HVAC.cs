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
                    foreach (Element space in spaces) TransferData(space);
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
        static public void TransferData(Element space)
        {
            string name = space.get_Parameter(BuiltInParameter.SPACE_ASSOC_ROOM_NAME).AsString();
            string number = space.get_Parameter(BuiltInParameter.SPACE_ASSOC_ROOM_NUMBER).AsString();
            space.get_Parameter(BuiltInParameter.ROOM_NAME).Set(name);
            space.get_Parameter(BuiltInParameter.ROOM_NUMBER).Set(number);
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
        protected string spacePowerParameterName = "ТеррНИИ_Тепловая нагрузка";

        protected bool CheckDefaultSharedParameters()
        {
            bool done = true;
            using (Transaction tr = new Transaction(doc, "Добавление общих параметров"))
            {
                tr.Start();
                //Проверяем наличие необходимых параметров в проекте                
                done &= SharedParameterUtils.AddSharedParameter(doc, spaceNumberParameterName,
                        new BuiltInCategory[] { BuiltInCategory.OST_DuctTerminal, BuiltInCategory.OST_MechanicalEquipment }, BuiltInParameterGroup.PG_IDENTITY_DATA);

                done &= SharedParameterUtils.AddSharedParameter(doc, exhaustSystemParameterName,
                    new BuiltInCategory[] { BuiltInCategory.OST_MEPSpaces }, BuiltInParameterGroup.PG_MECHANICAL_AIRFLOW);

                done &= SharedParameterUtils.AddSharedParameter(doc, supplySystemParameterName,
                    new BuiltInCategory[] { BuiltInCategory.OST_MEPSpaces }, BuiltInParameterGroup.PG_MECHANICAL_AIRFLOW);

                done &= SharedParameterUtils.AddSharedParameter(doc, spaceTemperatureParameterName,
                    new BuiltInCategory[] { BuiltInCategory.OST_MEPSpaces }, BuiltInParameterGroup.PG_ENERGY_ANALYSIS);
                done &= SharedParameterUtils.AddSharedParameter(doc, spacePowerParameterName,
                    new BuiltInCategory[] { BuiltInCategory.OST_MechanicalEquipment }, BuiltInParameterGroup.PG_ENERGY_ANALYSIS);

                /* 
                  Эти параметры должны быть внутри семейства, нет смысла назначать их всей категории

                    done &= CustomSharedParameter.AddSharedParameter(doc, thermalPowerParameterName, "ADSK_Main_MEP", true,
                        new BuiltInCategory[] { BuiltInCategory.OST_MechanicalEquipment }, BuiltInParameterGroup.PG_MECHANICAL);

                    done &= CustomSharedParameter.AddSharedParameter(doc, airflowParameterName, "ADSK_Main_MEP", true,
                        new BuiltInCategory[] { BuiltInCategory.OST_DuctTerminal }, BuiltInParameterGroup.PG_MECHANICAL_AIRFLOW);
                */

                done &= SharedParameterUtils.AddSharedParameter(doc, skipParameterName,
                        new BuiltInCategory[] { BuiltInCategory.OST_DuctTerminal, BuiltInCategory.OST_MechanicalEquipment }, BuiltInParameterGroup.PG_ANALYSIS_RESULTS);
                tr.Commit();
            }
            return done;
        }

        protected void SetSpaces(List<FamilyInstance> allPlunts)
        {
            var log = LoggingMachine.NewLog("Назначение пространств", allPlunts, "Оборудование не находится в пространстве");
            UI.ProgressBar pBar = new UI.ProgressBar("Назначение пространств", allPlunts.Count);
            // Назначаем номера помещений диффузорам
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
                    log.FailedElementIds.Push(el.Id);
                }
                pBar.StepUp();
            }
        }

        protected void ShowResultDialog(string operationTitle, List<FamilyInstance> allPlunts, List<FamilyInstance> missingPlunts, string hint)
        {
            if (missingPlunts.Count > 0)
            {
                string allMissedIds = String.Join(", ", (from e in missingPlunts select e.Id.ToString()));
                TaskDialog dialog = new TaskDialog("Результат");
                dialog.MainInstruction = String.Format("Операция: {0}\nНеудачно: {1} из {2}", operationTitle, missingPlunts.Count, allPlunts.Count);
                dialog.MainContent = "Перечень id элементов, которые не удалось обновить:\n"
                    + allMissedIds;
                dialog.FooterText = hint;
                dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Скопировать ID элементов в буфер обмена");

                TaskDialogResult result = dialog.Show();
                if (result == TaskDialogResult.CommandLink1)
                {
                    System.Windows.Forms.Clipboard.SetText(allMissedIds);
                    TaskDialog.Show("Результат", "Данные успешно скопированы в буфер обмена");
                }
            }
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
    class DiffuserProcessing : PluntProcessing, IExternalCommand
    {
        protected override Document doc { get; set; }
        private string suplySystemTypeName;
        private string exhaustSystemTypeName;

        private void UpdatePluntGeometry(List<FamilyInstance> allPlunts, ref List<FamilyInstance> failedPlunts)
        {
            UI.ProgressBar pBar = new UI.ProgressBar("Назначение размера", allPlunts.Count);

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
                new int[] { 400, 400 },
                new int[] { 450, 450 },
                new int[] { 500, 500 },
                };
            int[] roundOpts = { 100, 125, 160, 200 };

            foreach (var plunt in allPlunts)
            {
                try
                {
                    // достаем назначенный расход
                    double airflow = plunt.LookupParameter("ADSK_Расход воздуха").AsDouble();
                    string pluntSystemType = plunt.get_Parameter(BuiltInParameter.RBS_DUCT_SYSTEM_TYPE_PARAM).AsValueString();
                    airflow = UnitUtils.ConvertFromInternalUnits(airflow, DisplayUnitType.DUT_CUBIC_METERS_PER_HOUR);
                    Connector connector = plunt.MEPModel.ConnectorManager.Connectors.Cast<Connector>().First();
                    switch (connector.Shape)
                    {
                        // Круглый диффузоры
                        case ConnectorProfileType.Round:
                            Parameter diamParam = plunt.LookupParameter("ADSK_Размер_Диаметр");
                            // Приточка
                            if (pluntSystemType == suplySystemTypeName)
                            {
                                if (airflow <= 50) diamParam.Set(UnitUtils.ConvertToInternalUnits(100, DisplayUnitType.DUT_MILLIMETERS));
                                else if (airflow <= 100) diamParam.Set(UnitUtils.ConvertToInternalUnits(125, DisplayUnitType.DUT_MILLIMETERS));
                                else if (airflow <= 160) diamParam.Set(UnitUtils.ConvertToInternalUnits(160, DisplayUnitType.DUT_MILLIMETERS));
                                else if (airflow <= 250) diamParam.Set(UnitUtils.ConvertToInternalUnits(200, DisplayUnitType.DUT_MILLIMETERS));
                                else failedPlunts.Add(plunt);
                            }
                            // Вытяжка
                            else if (pluntSystemType == exhaustSystemTypeName)
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
                            if (pluntSystemType == suplySystemTypeName || pluntSystemType == exhaustSystemTypeName)
                            {
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
                            }
                            break;

                        default:
                            failedPlunts.Add(plunt);
                            break;
                    }
                }
                catch (Autodesk.Revit.Exceptions.InvalidObjectException ex)
                {
                    failedPlunts.Add(plunt);                   
                }
                finally
                {
                    pBar.StepUp();
                }
            }
            pBar.Close();
        }
        
        private void UpdatePluntData(List<FamilyInstance> allPlunts, ref List<FamilyInstance> failedPlunts)
        {
            const int upperZoneHeight = 1800;
            UI.ProgressBar pBar = new UI.ProgressBar("Назначение расхода", allPlunts.Count);
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
                    int countUpperZone = (from d in allPlunts
                                 where d.LookupParameter(spaceNumberParameterName).AsString() == spaceNumberParam.AsString()
                                 && d.get_Parameter(sysTypeBuiltIn).AsValueString() == systemType
                                 && UnitUtils.ConvertFromInternalUnits(d.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM).AsDouble(), DisplayUnitType.DUT_MILLIMETERS) >= upperZoneHeight
                                          select d).Count();
                    int countBottomZone = (from d in allPlunts
                                 where d.LookupParameter(spaceNumberParameterName).AsString() == spaceNumberParam.AsString()
                                 && d.get_Parameter(sysTypeBuiltIn).AsValueString() == systemType
                                 && UnitUtils.ConvertFromInternalUnits(d.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM).AsDouble(), DisplayUnitType.DUT_MILLIMETERS) < upperZoneHeight
                                           select d).Count();
                    // Смещение
                    bool isInUpperZone = UnitUtils.ConvertFromInternalUnits(plunt.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM).AsDouble(), DisplayUnitType.DUT_MILLIMETERS) >= upperZoneHeight;

                    // Находим пространство, в котором находится диффузор и достаем нужные значения
                    Space space = GetSpaceOfPlant(plunt);
                    if (space != null)
                    {
                        // Задаем расход диффузорам
                        double value = 0;
                        if (systemType == suplySystemTypeName) value = space.get_Parameter(BuiltInParameter.ROOM_DESIGN_SUPPLY_AIRFLOW_PARAM).AsDouble();
                        else if (systemType == exhaustSystemTypeName) value = space.get_Parameter(BuiltInParameter.ROOM_DESIGN_EXHAUST_AIRFLOW_PARAM).AsDouble();
                        
                        // делим на количество диффузоров нужной системы, расположенных в одной вертикальной зоне
                        if (countUpperZone == 0 || countBottomZone == 0)
                        {
                            value = countUpperZone != 0 ? value / countUpperZone : value / countBottomZone;
                        }
                        else value = isInUpperZone ? value / countUpperZone : value / countBottomZone;
                        airflowParam.Set(value);                        
                    }
                }
                catch
                {
                    failedPlunts.Add(plunt);
                }
                finally
                {
                    pBar.StepUp();
                }
            }
            pBar.Close();
        }
        
        private void UpdatePluntSystem(List<FamilyInstance> allPlunts, ref List<FamilyInstance> failedPlunts)
        {
            UI.ProgressBar pBar = new UI.ProgressBar("Назначение систем", allPlunts.Count);
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
                    /*else if (systemTypeName == "Не определено")
                    {
                        if (systemClassName == "Приточный воздух") systemName = space.LookupParameter(supplySystemParameterName).AsString();
                        else if (systemClassName == "Отработанный воздух") systemName = space.LookupParameter(exhaustSystemParameterName).AsString();
                    }*/
                    if (systemName != null)
                    {
                        ConnectorSet connectors = plunt.MEPModel.ConnectorManager.Connectors;
                        Connector baseConnector = connectors.Cast<Connector>().FirstOrDefault();
                        MEPSystem fromSystem = baseConnector?.MEPSystem;
                        MEPSystem toSystem = new FilteredElementCollector(doc).OfClass(typeof(MEPSystem)).Cast<MEPSystem>().Where(x => x.Name == systemName).FirstOrDefault();
                        if (fromSystem?.Id.IntegerValue == toSystem?.Id.IntegerValue) continue;
                        if (fromSystem == null && toSystem == null)
                        {
                            DuctSystemType ductType = DuctSystemType.UndefinedSystemType;
                            if (systemTypeName == suplySystemTypeName) ductType = DuctSystemType.SupplyAir;
                            else if (systemTypeName == exhaustSystemTypeName) ductType = DuctSystemType.ExhaustAir;
                            toSystem = doc.Create.NewMechanicalSystem(null, connectors, ductType);
                            toSystem.Name = systemName;
                        }
                        else if (fromSystem == null && toSystem != null)
                        {
                            toSystem.Add(connectors);
                        }
                        else if (fromSystem != null && toSystem == null)
                        {
                            fromSystem.Name = systemName;
                        }
                        else if (fromSystem != null && toSystem != null)
                        {
                            try
                            {
                                if (fromSystem.ConnectorManager.Connectors.Size > 1)
                                {
                                    ConnectorSet smallSet = new ConnectorSet();
                                    smallSet.Insert(baseConnector);
                                    fromSystem.Remove(smallSet);
                                    toSystem.Add(smallSet);
                                }
                                else failedPlunts.Add(plunt);
                            }
                            catch (Autodesk.Revit.Exceptions.ArgumentException)
                            {
                                failedPlunts.Add(plunt);
                            }
                            finally { pBar.StepUp(); }
                        }
                    }
                    else failedPlunts.Add(plunt);                    
                }
                pBar.StepUp();
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

            if (suplySystemTypeName == null || suplySystemTypeName == "" || exhaustSystemTypeName == null || exhaustSystemTypeName == "")
            {
                TaskDialog.Show("Результат", "Операция отменена");
                return Result.Cancelled;
            }

            List<FamilyInstance> spaceMissing = new List<FamilyInstance>();
            List<FamilyInstance> systemMissing = new List<FamilyInstance>();
            List<FamilyInstance> dataMissing = new List<FamilyInstance>();
            List<FamilyInstance> geometryMissing = new List<FamilyInstance>();
            using (Transaction tr = new Transaction(doc, "Обновление воздухораспределителей"))
            {
                tr.Start();
                SetSpaces(allPlunts);
                tr.Commit();
                tr.Start();                
                UpdatePluntSystem(allPlunts, ref systemMissing);
                tr.Commit();
                tr.Start();
                UpdatePluntData(allPlunts, ref dataMissing);
                tr.Commit();
                tr.Start();
                UpdatePluntGeometry(allPlunts, ref geometryMissing);
                tr.Commit();
            }

            ShowResultDialog("Назначение пространства", allPlunts, spaceMissing, "Если экземпляр все же находится в пространстве, проверьте настройки точки расчета площади в семействе");
            ShowResultDialog("Назначение системы", allPlunts, systemMissing, "");
            ShowResultDialog("Назначение расхода", allPlunts, dataMissing, "");
            ShowResultDialog("Подбор типоразмера", allPlunts, geometryMissing, "Вероятно, расход выше заданного максимума воздухораспределителя. Добавьте новые в пространство.");
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
            LoggingMachine.Reset();

            List<FamilyInstance> allPlunts = GetPlunts();
            if (allPlunts.Count < 1)
            {
                TaskDialog.Show("Ошибка", "Не найден ни один радиатор");
                return Result.Failed;
            }
            using (Transaction tr = new Transaction(doc, "Обновление радиаторов"))
            {
                tr.Start();
                SetSpaces(allPlunts);
                tr.Commit();
                tr.Start();
                UpdatePluntData(allPlunts);                
                tr.Commit();
                tr.Start();
                UpdatePluntGeometry(allPlunts);
                tr.Commit();
            }

            LoggingMachine.Show();

            return Result.Succeeded;
        }

        private void UpdatePluntData(List<FamilyInstance> allPlunts)
        {
            ElementProcessingLog paramLog = LoggingMachine.NewLog(
                "Обновление данных радиаторов", allPlunts, "Не удалось обновить значение параметров", "Проверьте семейство на их наличие");
            ElementProcessingLog tempLog = LoggingMachine.NewLog(
                "Обновление данных радиаторов", allPlunts, "Не удалось определить температуру в системе приточки или обратки", "Проверьте, правильно ли подключен радиатор");

            foreach (FamilyInstance plunt in allPlunts)
            {
                // находим параметры для переопределения
                Parameter pluntTi = plunt.LookupParameter("ADSK_Температура в помещении");
                Parameter pluntTp = plunt.LookupParameter("ТеррНИИ_Температура обратки");
                Parameter pluntTz = plunt.LookupParameter("ТеррНИИ_Температура подачи");

                // Температуру помещения определяем из свойств пространства
                Space space = GetSpaceOfPlant(plunt);
                double spaceTi = space != null ? space.LookupParameter(spaceTemperatureParameterName).AsDouble() : 0;
                // Температуру подачи и обратки определяем из свойств подключенных систем
                ConnectorManager connMng = plunt.MEPModel.ConnectorManager;
                if (connMng == null)
                {
                    tempLog.FailedElementIds.Push(plunt.Id);
                    continue;
                }
                Connector connTz = connMng.Connectors.Cast<Connector>().Where(x => x.PipeSystemType == PipeSystemType.SupplyHydronic && x.MEPSystem != null).FirstOrDefault();                
                Connector connTp = connMng.Connectors.Cast<Connector>().Where(x => x.PipeSystemType == PipeSystemType.ReturnHydronic && x.MEPSystem != null).FirstOrDefault();

                // Назначаем температуру
                if (pluntTi != null && pluntTp != null && pluntTz != null && connTp != null && connTz != null)
                {
                    double systemTz = ((PipingSystemType)doc.GetElement(connTz.MEPSystem.GetTypeId())).FluidTemperature;
                    double systemTp = ((PipingSystemType)doc.GetElement(connTp.MEPSystem.GetTypeId())).FluidTemperature;
                    pluntTi.Set(spaceTi);
                    pluntTz.Set(systemTz);
                    pluntTp.Set(systemTp);
                }
                else paramLog.FailedElementIds.Push(plunt.Id);
            }
        }

        protected void UpdatePluntGeometry(List<FamilyInstance> allPlunts)
        {
            // logging            
            ElementProcessingLog lostParameterLog = LoggingMachine.NewLog("Обновление геометрии радиаторов", allPlunts, "В семействе отсутствуют необходимые параметры");
            ElementProcessingLog highWattLog = LoggingMachine.NewLog("Обновление геометрии радиаторов", allPlunts, "Ни один из вариантов не подходит под такую нагрузку");
            ElementProcessingLog invalidTableLog = LoggingMachine.NewLog(
                "Обновление геометрии радиаторов", allPlunts, "В таблица выбора отсутствуют подходящие значения", "Проверьте соответствии таблицы выбора и настроек плагина");

            foreach (FamilyInstance plunt in allPlunts)
            {
                Space space = GetSpaceOfPlant(plunt);
                if (space == null)
                {
                    continue;
                }
                int count = allPlunts.Where(x => x.LookupParameter("ТеррНИИ_Номер помещения").AsString() == space.get_Parameter(BuiltInParameter.ROOM_NUMBER).AsString()).Count();
                double requiredValue = space.get_Parameter(BuiltInParameter.ROOM_DESIGN_HEATING_LOAD_PARAM).AsDouble() / count;
                plunt.LookupParameter(spacePowerParameterName).Set(requiredValue); // для проверки найденных типораразмеров
                requiredValue = UnitUtils.ConvertFromInternalUnits(requiredValue, DisplayUnitType.DUT_WATTS);

                FamilySizeTableManager sizeMng = FamilySizeTableManager.GetFamilySizeTableManager(doc, plunt.Symbol.Family.Id);
                Parameter tableNameParam = plunt.Symbol.LookupParameter("Таблица выбора");
                Parameter TzParam = plunt.LookupParameter("ТеррНИИ_Температура подачи");
                Parameter TpParam = plunt.LookupParameter("ТеррНИИ_Температура обратки");
                Parameter TiParam = plunt.LookupParameter("ADSK_Температура в помещении");
                Parameter hParam = plunt.LookupParameter("ADSK_Размер_Высота");
                Parameter lParam = plunt.LookupParameter("ADSK_Размер_Длина");
                Parameter radTypeParam = plunt.LookupParameter("ТеррНИИ_Тип радиатора");                

                if (hParam == null || lParam == null || radTypeParam == null || TzParam == null ||
                    TpParam == null || TiParam == null || sizeMng == null || tableNameParam == null)
                {
                    lostParameterLog.FailedElementIds.Push(plunt.Id);
                    continue;
                }

                double Tz = UnitUtils.ConvertFromInternalUnits(TzParam.AsDouble(), DisplayUnitType.DUT_CELSIUS);
                double Tp = UnitUtils.ConvertFromInternalUnits(TpParam.AsDouble(), DisplayUnitType.DUT_CELSIUS);
                double Ti = UnitUtils.ConvertFromInternalUnits(TiParam.AsDouble(), DisplayUnitType.DUT_CELSIUS);
                FamilySizeTable sizeTable = sizeMng.GetSizeTable(plunt.Symbol.LookupParameter("Таблица выбора").AsString());

                string sN, sQn, sTzn, sTpn, sTin;
                double N, Qn, Tzn, Tpn, Tin, powerValue;
                bool wattFinded = false;
                bool rowFinded = false;

                Parameter keepHeightParam = plunt.LookupParameter("Не изменять высоту");
                Parameter keepLengthParam = plunt.LookupParameter("Не изменять длину");
                Parameter keepTypeParam = plunt.LookupParameter("Не изменять тип");
                var iteratedHeights = keepHeightParam != null && keepHeightParam.AsInteger() == 1
                    ?
                    new List<int>() { Convert.ToInt32(UnitUtils.ConvertFromInternalUnits(hParam.AsDouble(), DisplayUnitType.DUT_MILLIMETERS)) } 
                    : 
                    TerrSettings.RadiatorHeights;
                var iteratedLengths = keepLengthParam != null && keepLengthParam.AsInteger() == 1
                    ?
                    new List<int>() { Convert.ToInt32(UnitUtils.ConvertFromInternalUnits(lParam.AsDouble(), DisplayUnitType.DUT_MILLIMETERS)) }
                    :
                    TerrSettings.RadiatorLengths;
                var iteratedTypes = keepTypeParam != null && keepTypeParam.AsInteger() == 1
                    ?
                    new List<int>() { radTypeParam.AsInteger() }
                    :
                    TerrSettings.RadiatorTypes;

                for (int t = 0; t < iteratedTypes.Count() && !wattFinded; t++)
                {
                    int type = iteratedTypes[t];
                    for (int h = 0; h < iteratedHeights.Count() && !wattFinded; h++)
                    {
                        int height = iteratedHeights[h];
                        sN = FamilyInstanceUtils.SizeLookup(sizeTable, "N", new string[] { type.ToString(), height.ToString() });
                        sQn = FamilyInstanceUtils.SizeLookup(sizeTable, "Qn", new string[] { type.ToString(), height.ToString() });
                        sTzn = FamilyInstanceUtils.SizeLookup(sizeTable, "Tz", new string[] { type.ToString(), height.ToString() });
                        sTpn = FamilyInstanceUtils.SizeLookup(sizeTable, "Tp", new string[] { type.ToString(), height.ToString() });
                        sTin = FamilyInstanceUtils.SizeLookup(sizeTable, "Ti", new string[] { type.ToString(), height.ToString() });
                        if (sN == null || sQn == null || sTzn == null || sTpn == null || sTin == null) continue;
                        rowFinded = true;

                        N = double.Parse(sN, System.Globalization.CultureInfo.InvariantCulture);
                        Qn = double.Parse(sQn, System.Globalization.CultureInfo.InvariantCulture);
                        Tzn = double.Parse(sTzn, System.Globalization.CultureInfo.InvariantCulture);
                        Tpn = double.Parse(sTpn, System.Globalization.CultureInfo.InvariantCulture);
                        Tin = double.Parse(sTin, System.Globalization.CultureInfo.InvariantCulture);

                        for (int l = 0; l < iteratedLengths.Count() && !wattFinded; l++)
                        {
                            int length = iteratedLengths[l];
                            powerValue = (length / 1000.0) * Qn * Math.Pow(
                                (
                                    (Tz - Tp)
                                        /
                                    Math.Log((Tz - Ti) / (Tp - Ti))
                                ) / (
                                    (Tzn - Tpn)
                                        /
                                    Math.Log((Tzn - Tin) / (Tpn - Tin))
                                ),
                                N
                                );

                            if (powerValue >= requiredValue)
                            {
                                lParam.Set(UnitUtils.ConvertToInternalUnits(length, DisplayUnitType.DUT_MILLIMETERS));
                                hParam.Set(UnitUtils.ConvertToInternalUnits(height, DisplayUnitType.DUT_MILLIMETERS));
                                radTypeParam.Set(type);
                                wattFinded = true;
                            }
                        }
                    }
                }
                if (!rowFinded) invalidTableLog.FailedElementIds.Push(plunt.Id);
                else if (!wattFinded) highWattLog.FailedElementIds.Push(plunt.Id);
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
                .Where(x => x.LookupParameter(skipParameterName).AsInteger() != 1)
                .ToList();
            return plunts;
        }
    }
}

