using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Diagnostics;
using TerrTools.Updaters;

namespace TerrTools
{
    public class App : IExternalApplication
    {
        public static AddInId AddInId { get { return new AddInId(new Guid("4e6830af-73c4-45fa-aea0-82df352d5157")); } }
        string tabName = "ТеррНИИ BIM";
        string releaseDLLPath = @"\\serverL\PSD\REVIT\Плагины\TerrTools";
        string releaseDLLName = "TerrTools.dll";
        string releaseDLLFullPath { get { return releaseDLLPath + @"\" + releaseDLLName; } }

        int btnCounter = 0;
        int pullBtnCounter = 0;

        UIControlledApplication application;

        private bool CheckUpdates(out string currentVersion, out string lastReleaseVersion, out string patchNote)
        {
            currentVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;

            lastReleaseVersion = FileVersionInfo.GetVersionInfo(releaseDLLFullPath).FileVersion;
            patchNote = FileVersionInfo.GetVersionInfo(releaseDLLFullPath).Comments;
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

            //updater = new MirroredInstancesUpdater();
            //filter = new ElementCategoryFilter(BuiltInCategory.OST_Doors);
            //UpdaterRegistry.RegisterUpdater(updater);
            //UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filter, Element.GetChangeTypeGeometry());

            updater = new SpaceUpdater();
            filter = new ElementCategoryFilter(BuiltInCategory.OST_MEPSpaces);
            UpdaterRegistry.RegisterUpdater(updater);
            UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filter, Element.GetChangeTypeElementAddition());
            UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filter, Element.GetChangeTypeAny());

            //updater = new RoomUpdater();
            //filter = new ElementCategoryFilter(BuiltInCategory.OST_Rooms);
            //UpdaterRegistry.RegisterUpdater(updater);
            //UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filter, Element.GetChangeTypeElementAddition());
            //UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filter, Element.GetChangeTypeAny());
        }

        public Result OnShutdown(UIControlledApplication application)
        {   
            //UpdaterRegistry.UnregisterUpdater(MirroredInstancesUpdater.UpdaterId);
            UpdaterRegistry.UnregisterUpdater(SpaceUpdater.UpdaterId);
            //UpdaterRegistry.UnregisterUpdater(RoomUpdater.UpdaterId);
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication app)
        {
            application = app;
            RegisterUpdaters();
            //string currentVersion, lastReleaseVersion, patchNote;
            if (CheckUpdates(out string currentVersion, out string lastReleaseVersion, out string patchNote))
            {
                TaskDialog td = new TaskDialog("Доступно обновление");
                td.MainInstruction = "На сервере доступна новая версия плагина. Рекомендуем закрыть программу, обновить плагин и возобновить работу";
                td.MainContent = string.Format("Текущая версия: {0}\nДоступная версия: {1}\n\nЧто нового: \n{2}", currentVersion, lastReleaseVersion, patchNote);
                td.FooterText = "Обновление плагина доступно здесь: " + releaseDLLPath;
                td.Show();
            }
            application.CreateRibbonTab(tabName);
            RibbonPanel panelArch = application.CreateRibbonPanel(tabName, "АР");
            RibbonPanel panelStruct = application.CreateRibbonPanel(tabName, "КР");
            RibbonPanel panelMEP = application.CreateRibbonPanel(tabName, "ОВиК");
            RibbonPanel panelGeneral = application.CreateRibbonPanel(tabName, "Общие");

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

            return Result.Succeeded;
        }
    }
}
