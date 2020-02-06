using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Diagnostics;

namespace TerrTools
{
    public class App : IExternalApplication
    {
        string tabName = "ТеррНИИ BIM";
        string releaseDLLPath = @"\\serverL\PSD\REVIT\Плагины\TerrTools";
        string releaseDLLName = "TerrTools.dll";
        string releaseDLLFullPath { get { return releaseDLLPath + @"\" + releaseDLLName; } }

        int btnCounter = 0;
        int pullBtnCounter = 0;

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
                Stream stream = this.GetType().Assembly.GetManifestResourceStream(embeddedPath);
                var decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                return decoder.Frames[0];
            }
            catch (System.ArgumentNullException) {
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

        public Result OnShutdown(UIControlledApplication application)
        {      
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
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
                    "Finishing",
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
            pbDict.Add("CopyRoomDataToSpace",
                MakePushButton(
                    "SpaceNaming",
                    "Переименовать\nпространства",
                    "Копирует номер и имя помещения из связанного файла",
                    "RoomToSpace.png"
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
            panelMEP.AddItem(pbDict["CopyRoomDataToSpace"]);


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
