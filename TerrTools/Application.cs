using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Diagnostics;

namespace TerrTools
{
    public class App : IExternalApplication
    {
        UIControlledApplication app;
        public static AddInId AddInId { get { return new AddInId(new Guid("4e6830af-73c4-45fa-aea0-82df352d5157")); } }
        string tabName = "ТеррНИИ BIM";
        string DLLPath { get { return @"\\serverL\PSD\REVIT\Плагины\TerrTools\TerrTools.dll"; } }
        string updaterPath { get { return @"\\serverL\PSD\REVIT\Плагины\TerrTools\TerrToolsUpdater.exe"; } }

        List<UpdaterId> updatersId = new List<UpdaterId>();
        int btnCounter = 0;
        int pullBtnCounter = 0;

        private bool CheckUpdates(out string currentVersion, out string lastReleaseVersion, out string patchNote)
        {
            currentVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;

            lastReleaseVersion = FileVersionInfo.GetVersionInfo(DLLPath).FileVersion;
            patchNote = FileVersionInfo.GetVersionInfo(DLLPath).Comments;
            return currentVersion != lastReleaseVersion;
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

            //updater = new MirroredInstancesUpdater();
            //updatersId.Add(updater.GetUpdaterId());
            //filter = new ElementCategoryFilter(BuiltInCategory.OST_Doors);
            //UpdaterRegistry.RegisterUpdater(updater);
            //UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filter, Element.GetChangeTypeGeometry());

            updater = new Updaters.SpaceUpdater();
            updatersId.Add(updater.GetUpdaterId());
            filter = new ElementCategoryFilter(BuiltInCategory.OST_MEPSpaces);
            UpdaterRegistry.RegisterUpdater(updater);
            UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filter, ChangeTypeAdditionAndModication);

            updater = new Updaters.DuctsUpdater();
            updatersId.Add(updater.GetUpdaterId());
            filter = new ElementCategoryFilter(BuiltInCategory.OST_DuctCurves);
            UpdaterRegistry.RegisterUpdater(updater);
            UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filter, ChangeTypeAdditionAndModication);

            //updater = new RoomUpdater();
            //updatersId.Add(updater.GetUpdaterId());
            //filter = new ElementCategoryFilter(BuiltInCategory.OST_Rooms);
            //UpdaterRegistry.RegisterUpdater(updater);
            //UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filter, Element.GetChangeTypeElementAddition());
            //UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filter, Element.GetChangeTypeAny());
        }

        private void CreateRibbon()
        {
            app.CreateRibbonTab(tabName);
            RibbonPanel panelArch = app.CreateRibbonPanel(tabName, "АР");
            RibbonPanel panelStruct = app.CreateRibbonPanel(tabName, "КР");
            RibbonPanel panelMEP = app.CreateRibbonPanel(tabName, "ОВиК");
            RibbonPanel panelGeneral = app.CreateRibbonPanel(tabName, "Общие");

            Dictionary<string, PushButtonData> pbDict = new Dictionary<string, PushButtonData>();
            Dictionary<string, PulldownButtonData> plDict = new Dictionary<string, PulldownButtonData>();

            PulldownButton tempBtn;

            ///
            /// Push buttons
            ///
            pbDict.Add(
                "RoomFinishingData",
                MakePushButton(
                    "FinishingData",
                    "Помещения: обновить\nпараметры отделки",
                    "Обновляет параметры в элементах категории \"Помещения\", требуемые для расчета отделки",
                    "Room.png"
                    ));
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
                "IntersectOpening",
                "В стенах",
                "Вставляет отверстия в местах пересечений с системами"
                ));
            pbDict.Add("FloorOpening",
            MakePushButton(
                "DummyClass",
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

            pbDict.Add("TEST",
    MakePushButton(
        "DummyClass",
        "ТЕСТ"
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
            panelArch.AddItem(pbDict["RoomFinishingData"]);
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
            tempBtn = panelGeneral.AddItem(plDict["UpdateType"]) as PulldownButton;
            tempBtn.AddPushButton(pbDict["UpdateTypeCurrent"]);
            tempBtn.AddPushButton(pbDict["UpdateTypeAll"]);
            tempBtn.AddPushButton(pbDict["TEST"]);
        }

        public Result OnShutdown(UIControlledApplication application)
        {   
            foreach (UpdaterId id in updatersId) UpdaterRegistry.UnregisterUpdater(id);
            return Result.Succeeded;
        }


        public Result OnStartup(UIControlledApplication app)
        {
            this.app = app;
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
                        break;

                    case TaskDialogResult.CommandLink2:
                        StartUpdaterService("-fromRevit");
                        RegisterUpdaters();
                        CreateRibbon();
                        break;

                    case TaskDialogResult.CommandLink3:
                        RegisterUpdaters();
                        CreateRibbon();
                        break;
                }
            }
            return Result.Succeeded;
        }


        private void StartUpdaterService(string argLine)
        {
            if (File.Exists(updaterPath)) Process.Start(updaterPath, argLine);
            else TaskDialog.Show("Ошибка", "Программа обновления отсутствует на сервере. Обновитесь самостоятельно");
        }
    }
}
