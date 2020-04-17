using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Diagnostics;

namespace TerrTools
{
    public class TerrToolsApp : IExternalApplication
    {
        public static UIControlledApplication Application;
        public static List<IUpdater> Updaters = new List<IUpdater>();
        public static AddInId AddInId { get { return new AddInId(new Guid("4e6830af-73c4-45fa-aea0-82df352d5157")); } }
        string tabName = "ТеррНИИ BIM";
        string DLLPath { get { return @"\\serverL\PSD\REVIT\Плагины\TerrTools\TerrTools.dll"; } }
        string updaterPath { get { return @"\\serverL\PSD\REVIT\Плагины\TerrTools\TerrToolsUpdater.exe"; } }

        
        int btnCounter = 0;
        int pullBtnCounter = 0;

        private bool CheckUpdates(out string currentVersion, out string lastReleaseVersion, out string patchNote)
        {
            currentVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;

            try
            {
                lastReleaseVersion = FileVersionInfo.GetVersionInfo(DLLPath).FileVersion;
                patchNote = FileVersionInfo.GetVersionInfo(DLLPath).Comments;
                return currentVersion != lastReleaseVersion;
            }
            catch
            {
                lastReleaseVersion = patchNote = "0";
                return false;
            }
        }

        private ImageSource BitmapToImageSource(string embeddedPath)
        {
            try
            {
                Stream stream = GetType().Assembly.GetManifestResourceStream(embeddedPath);
                var decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                return decoder.Frames[0];
            }
            catch (ArgumentNullException)
            {
                return null;
            }
        }

        private PushButtonData MakePushButton
    (string className, string btnText, string toolTip = null, string iconName = null)
        {
            btnCounter++;
            PushButtonData btnData = new PushButtonData(
                "PushButton" + btnCounter.ToString(),
                btnText,
                Assembly.GetExecutingAssembly().Location,
                "TerrTools." + className);
            btnData.ToolTip = toolTip ?? "";
            if (iconName != null) btnData.LargeImage = BitmapToImageSource("TerrTools.Resources.Icons." + iconName);
            return btnData;
        }

        private PulldownButtonData MakePulldownButton
            (string btnText, string toolTip = null, string iconName = null)
        {
            pullBtnCounter++;
            PulldownButtonData btnData = new PulldownButtonData("PulldownButton" + pullBtnCounter.ToString(), btnText);
            btnData.ToolTip = toolTip ?? "";
            if (iconName != null) btnData.LargeImage = BitmapToImageSource("TerrTools.Resources.Icons." + iconName);
            return btnData;
        }

        private void RegisterUpdaters()
        {            
            IUpdater updater;
            ElementFilter filter;

            ChangeType ChangeTypeAdditionAndModication = ChangeType.ConcatenateChangeTypes(Element.GetChangeTypeAny(), Element.GetChangeTypeElementAddition());
            ChangeType allChangeTypes = ChangeType.ConcatenateChangeTypes(ChangeTypeAdditionAndModication, Element.GetChangeTypeElementDeletion());

            //updater = new MirroredInstancesUpdater();
            //updatersId.Add(updater.GetUpdaterId());
            //filter = new ElementCategoryFilter(BuiltInCategory.OST_Doors);
            //UpdaterRegistry.RegisterUpdater(updater);
            //UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filter, Element.GetChangeTypeGeometry());

            updater = new Updaters.SpaceUpdater("SpaceUpdater", "b49432e1-c88d-4020-973d-1464f2d7b121", "", ChangePriority.RoomsSpacesZones);
            Updaters.Add(updater);
            filter = new ElementCategoryFilter(BuiltInCategory.OST_MEPSpaces);
            UpdaterRegistry.RegisterUpdater(updater);
            UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filter, ChangeTypeAdditionAndModication);

            updater = new Updaters.DuctsUpdater("DuctUpdater", "93dc3d80-0c29-4af5-a509-c36dfd497d66", "", ChangePriority.MEPAccessoriesFittingsSegmentsWires);
            Updaters.Add(updater);
            filter = new LogicalAndFilter(new ElementCategoryFilter(BuiltInCategory.OST_DuctCurves), new ElementIsElementTypeFilter(true));
            UpdaterRegistry.RegisterUpdater(updater);
            UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filter, ChangeTypeAdditionAndModication);

            updater = new Updaters.DuctsAccessoryUpdater("DuctAccessoryUpdater", "79e309d3-bd2d-4255-84b8-2133c88b695d", "", ChangePriority.MEPAccessoriesFittingsSegmentsWires);
            Updaters.Add(updater);
            filter = new LogicalAndFilter(new ElementCategoryFilter(BuiltInCategory.OST_DuctAccessory), new ElementIsElementTypeFilter(true));
            UpdaterRegistry.RegisterUpdater(updater);
            UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filter, ChangeTypeAdditionAndModication);

            updater = new Updaters.RoomUpdater("RoomUpdater", "a82a5ae5-9c21-4645-b029-d3d0b67312f1", "", ChangePriority.RoomsSpacesZones);
            Updaters.Add(updater);
            var filter1 = new ElementCategoryFilter(BuiltInCategory.OST_Rooms);
            var filter2 = new ElementCategoryFilter(BuiltInCategory.OST_Doors);
            var filter3 = new ElementCategoryFilter(BuiltInCategory.OST_Windows);
            var filterRDW = new LogicalOrFilter(new List<ElementFilter>() { filter1, filter2, filter3 });
            var filterDW = new LogicalOrFilter(filter2, filter3);
            UpdaterRegistry.RegisterUpdater(updater);
            UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filterRDW, ChangeTypeAdditionAndModication);
            UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filterDW, Element.GetChangeTypeElementDeletion());

            updater = new Updaters.PartUpdater("PartUpdater", "79ef66cc-2d1a-4bdd-9bae-dae5aa8501f0", "", ChangePriority.FloorsRoofsStructuralWalls);
            Updaters.Add(updater);
            filter = new ElementCategoryFilter(BuiltInCategory.OST_Parts);
            UpdaterRegistry.RegisterUpdater(updater);
            UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filter, Element.GetChangeTypeAny());
        }

        private void CreateRibbon()
        {
            Application.CreateRibbonTab(tabName);
            RibbonPanel panelInfo = Application.CreateRibbonPanel(tabName, "ТеррНИИ BIM");
            RibbonPanel panelArch = Application.CreateRibbonPanel(tabName, "АР");
            RibbonPanel panelStruct = Application.CreateRibbonPanel(tabName, "КР");
            RibbonPanel panelMEP = Application.CreateRibbonPanel(tabName, "ОВиК");
            RibbonPanel panelGeneral = Application.CreateRibbonPanel(tabName, "Общие");

            Dictionary<string, PushButtonData> pbDict = new Dictionary<string, PushButtonData>();
            Dictionary<string, PulldownButtonData> plDict = new Dictionary<string, PulldownButtonData>();

            PulldownButton tempBtn;

            ///
            /// Push buttons
            ///            
            pbDict.Add("DiffuserProcessing",
            MakePushButton(
                "DiffuserProcessing",
                "Обновить\nдиффузоры",
                "Копирует номер пространства и расход воздуха для всех диффузоров в проекте",
                "Diffuser.png"
                ));
            pbDict.Add("RadiatorProcessing",
                MakePushButton(
                "RadiatorProcessing",
                "Обновить\nрадиаторы",
                "Копирует номер пространства и подбирает радиатор исходя из заданной в пространстве тепловой мощности",
                "Radiator.png"));
            pbDict.Add("WallOpening",
           MakePushButton(
                "WallOpeningHandler",
                "В стенах",
                "Вставляет отверстия в местах пересечений с системами"
                ));
            pbDict.Add("FloorOpening",
            MakePushButton(
                "FloorOpeningHandler",
                "В перекрытиях"
                ));
            pbDict.Add("GenerateFloor",
           MakePushButton(
                "FloorFinishing",
                "Отделка пола",
                "Создает элемент \"Перекрытие\" нужного типоразмера в указанных помещениях",
                "Brush.png"
                ));
            pbDict.Add("FocusOnElement",
                MakePushButton(
                    "FocusOnElement",
                    "Сфокусироваться\nна элементе",
                    "Выберите из списка необходимый элемент и окно сфокусируется на нем",
                    "Zoom.png"
                    ));
            pbDict.Add("UpdateTypeCurrent",
                MakePushButton(
                    "TypeChanger",
                    "В текущем проекте"
                    ));
            pbDict.Add("UpdateTypeAll",
                MakePushButton(
                    "TypeChangerDeep",
                    "В проекте и семействах"
                    ));
            pbDict.Add("CopyRoomShape",
                MakePushButton(
                    "CopyRoomShape",
                    "Создать контур\nпомещения",
                    iconName:"Shape.png"
                    ));
            pbDict.Add("SystemScheduleExporter",
                MakePushButton(
                    "ScheduleExporter",
                    "Экспорт\nспецификаций",
                    iconName: "Tables.png"
                    ));

            ///
            /// Pulldown buttons
            ///
            plDict.Add("GenerateOpenings",
                MakePulldownButton(
                    "Генерация отверстий",
                    "Быстрая генерация отверстий на пересечении конструктивных элементов с инженерными системами",
                    "Openings.png"
                    ));
            plDict.Add("UpdateType",
                MakePulldownButton(
                    "Обновить шрифт",
                    "Обновление всех шрифтов в проекте под стандарты предприятия",
                    "Type.png"
                    ));

            ///
            /// Архитектурная панель
            ///
            panelArch.AddItem(pbDict["GenerateFloor"]);

            ///
            /// Конструкторская панель
            ///
            tempBtn = panelStruct.AddItem(plDict["GenerateOpenings"]) as PulldownButton;
            tempBtn.AddPushButton(pbDict["WallOpening"]);
            tempBtn.AddPushButton(pbDict["FloorOpening"]);


            ///
            /// ОВиК панель
            ///
            panelMEP.AddItem(pbDict["DiffuserProcessing"]);
            panelMEP.AddItem(pbDict["RadiatorProcessing"]);


            ///
            /// Общая панель
            ///
            panelGeneral.AddItem(pbDict["FocusOnElement"]);
            panelGeneral.AddItem(pbDict["CopyRoomShape"]);
            tempBtn = panelGeneral.AddItem(plDict["UpdateType"]) as PulldownButton;
            tempBtn.AddPushButton(pbDict["UpdateTypeCurrent"]);
            tempBtn.AddPushButton(pbDict["UpdateTypeAll"]);
            panelGeneral.AddItem(pbDict["SystemScheduleExporter"]);

            ///
            /// Настройки
            ///
            panelInfo.AddItem(MakePushButton("SettingsWindow", "Настройки", iconName: "Settings.png"));
        }

        public Result OnShutdown(UIControlledApplication application)
        {   
            foreach (IUpdater upd in Updaters) UpdaterRegistry.UnregisterUpdater(upd.GetUpdaterId());
            return Result.Succeeded;
        }


        public Result OnStartup(UIControlledApplication app)
        {
            TerrToolsApp.Application = app;
            app.Idling += OverrideCommands;


            if (CheckUpdates(out string currentVersion, out string lastReleaseVersion, out string patchNote))
            {
                TaskDialog td = new TaskDialog("Доступно обновление");
                td.MainInstruction = "На сервере доступна новая версия плагина. Рекомендуем закрыть программу, обновить плагин и возобновить работу";
                td.MainContent = string.Format("Текущая версия: {0}\nДоступная версия: {1}\n\nЧто нового: \n{2}", currentVersion, lastReleaseVersion, patchNote);
                td.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Обновить сейчас (перезагрузка программы)");
                td.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Обновить позже (автоматически, после закрытия программы)");
                td.AddCommandLink(TaskDialogCommandLinkId.CommandLink3, "Не обновлять");
                TaskDialogResult tdResult = td.Show();
                switch (tdResult)
                {
                    case TaskDialogResult.CommandLink1:
                        StartUpdaterService("-fromRevit -restart");
                        Process.GetCurrentProcess().CloseMainWindow();
                        return Result.Cancelled;

                    case TaskDialogResult.CommandLink2:
                        StartUpdaterService("-fromRevit");
                        break;

                    case TaskDialogResult.CommandLink3:
                        break;
                }
            }
            RegisterUpdaters();
            CreateRibbon();
            return Result.Succeeded;
        }

        // В этом методе можно перезаписать поведение стандартных комманд в Revit
        // Список команд можно найти в файле commandids.txt в папке проекта
        private void OverrideCommands(object sender, Autodesk.Revit.UI.Events.IdlingEventArgs e)
        {
            Application.Idling -= OverrideCommands;
            UIApplication uiapp = sender as UIApplication;
            if (uiapp != null)
            {
                /*
                RevitCommandId commandId = RevitCommandId.LookupCommandId("ID_BUTTON_DELETE");
                try
                {
                    AddInCommandBinding deleteBinding = uiapp.CreateAddInCommandBinding(commandId);
                    deleteBinding.Executed += new EventHandler<Autodesk.Revit.UI.Events.ExecutedEventArgs>(DeleteBinding_Executed);
                }
                catch
                {
                }
                */
            }
        }

        private void StartUpdaterService(string argLine)
        {
            if (File.Exists(updaterPath)) Process.Start(updaterPath, argLine);
            else TaskDialog.Show("Ошибка", "Программа обновления отсутствует на сервере. Обновитесь самостоятельно");
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class SettingsWindow : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var form = new UI.SettingsForm();
            return Result.Succeeded;
        }
    }
}
