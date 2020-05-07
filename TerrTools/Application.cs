using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Diagnostics;
using TerrTools.Updaters;

namespace TerrTools
{
    public class App : IExternalApplication
    {
        public static UIControlledApplication Application { get; set; }
        public static List<TerrUpdater> Updaters = new List<TerrUpdater>();
        public static AddInId AddInId { get => new AddInId(new Guid("4e6830af-73c4-45fa-aea0-82df352d5157")); }
        public static string AppName { get => "ТеррНИИ BIM"; }
        public static string DLLPath { get => @"\\serverL\PSD\REVIT\Плагины\TerrTools\TerrTools.dll";  }
        public static string UpdaterPath { get => @"\\serverL\PSD\REVIT\Плагины\TerrTools\TerrToolsUpdater.exe";  }
        public static string Version { get => FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion; }
        public static string PatchNote { get => FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).Comments; }
        
        int btnCounter = 0;
        int pullBtnCounter = 0;

        static public bool CheckUpdates(out string lastReleaseVersion, out string patchNote)
        {
            try
            {
                lastReleaseVersion = FileVersionInfo.GetVersionInfo(DLLPath).FileVersion;
                patchNote = FileVersionInfo.GetVersionInfo(DLLPath).Comments;
                return App.Version != lastReleaseVersion;
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
            ChangeType ChangeTypeAdditionAndModication = ChangeType.ConcatenateChangeTypes(Element.GetChangeTypeAny(), Element.GetChangeTypeElementAddition());
            ChangeType allChangeTypes = ChangeType.ConcatenateChangeTypes(ChangeTypeAdditionAndModication, Element.GetChangeTypeElementDeletion());

            Updaters.Add(new SpaceUpdater(                 
                new ElementCategoryFilter(BuiltInCategory.OST_MEPSpaces),
                ChangeTypeAdditionAndModication));

            Updaters.Add(new DuctsUpdater(                
                new LogicalAndFilter(new ElementCategoryFilter(BuiltInCategory.OST_DuctCurves), new ElementIsElementTypeFilter(true)),
                ChangeTypeAdditionAndModication));

            Updaters.Add(new DuctsAccessoryUpdater(                
                new LogicalAndFilter(new ElementCategoryFilter(BuiltInCategory.OST_DuctAccessory), new ElementIsElementTypeFilter(true)),
                ChangeTypeAdditionAndModication));

            Updaters.Add(new PartUpdater(                
                new ElementCategoryFilter(BuiltInCategory.OST_Parts),
                Element.GetChangeTypeAny()));

            BuiltInCategory[] elemCats = new BuiltInCategory[]
            {
                BuiltInCategory.OST_DuctAccessory,
                BuiltInCategory.OST_PipeAccessory,
                BuiltInCategory.OST_DuctCurves,
                BuiltInCategory.OST_PlaceHolderDucts,
                BuiltInCategory.OST_DuctTerminal,
                BuiltInCategory.OST_FlexDuctCurves,
                BuiltInCategory.OST_FlexPipeCurves,
                BuiltInCategory.OST_DuctInsulations,
                BuiltInCategory.OST_PipeInsulations,
                BuiltInCategory.OST_PipeCurves,
                BuiltInCategory.OST_DuctSystem,
                BuiltInCategory.OST_PipingSystem,
                BuiltInCategory.OST_PlaceHolderPipes,
                BuiltInCategory.OST_DuctFitting,
                BuiltInCategory.OST_PipeFitting,
                BuiltInCategory.OST_MechanicalEquipment,
                BuiltInCategory.OST_Sprinklers
            };
            var filter = new ElementMulticategoryFilter(elemCats);
            Updaters.Add(new SystemNamingUpdater(filter, ChangeTypeAdditionAndModication));
            Updaters.Last().AddTriggerPair(
                new ElementMulticategoryFilter(new BuiltInCategory[] { BuiltInCategory.OST_DuctSystem, BuiltInCategory.OST_PipingSystem }),
                Element.GetChangeTypeAny()
                );

            var filterRDW = new LogicalOrFilter(new List<ElementFilter>() 
            { new ElementCategoryFilter(BuiltInCategory.OST_Rooms), 
                new ElementCategoryFilter(BuiltInCategory.OST_Doors), 
                new ElementCategoryFilter(BuiltInCategory.OST_Windows)});
            var filterDW = new LogicalOrFilter(new List<ElementFilter>()
            {  new ElementCategoryFilter(BuiltInCategory.OST_Doors),
                new ElementCategoryFilter(BuiltInCategory.OST_Windows) });
            Updaters.Add(new RoomUpdater(                
                filterRDW,
                ChangeTypeAdditionAndModication));
            Updaters.Last().AddTriggerPair(filterDW, Element.GetChangeTypeElementDeletion());


            foreach (var upd in Updaters)
            {
                UpdaterRegistry.RegisterUpdater(upd);
                foreach (var trigger in upd.TriggerPairs)
                {
                    UpdaterRegistry.AddTrigger(upd.GetUpdaterId(), trigger.Filter, trigger.ChangeType);
                }
            }
        }

        private void CreateRibbon()
        {
            Application.CreateRibbonTab(AppName);
            RibbonPanel panelInfo = Application.CreateRibbonPanel(AppName, "ТеррНИИ BIM");
            RibbonPanel panelArch = Application.CreateRibbonPanel(AppName, "АР");
            RibbonPanel panelStruct = Application.CreateRibbonPanel(AppName, "КР");
            RibbonPanel panelMEP = Application.CreateRibbonPanel(AppName, "ОВиК");
            RibbonPanel panelGeneral = Application.CreateRibbonPanel(AppName, "Общие");

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
            App.Application = app;
            CheckUpdateDialog();
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

        static public void CheckUpdateDialog()
        {
            if (CheckUpdates(out string lastReleaseVersion, out string patchNote))
            {
                TaskDialog td = new TaskDialog("Доступно обновление");
                td.MainInstruction = "На сервере доступна новая версия плагина. Обновить прямо сейчас?";
                td.MainContent = string.Format("Текущая версия: {0}\nДоступная версия: {1}\n\nЧто нового: \n{2}", App.Version, lastReleaseVersion, patchNote);
                td.FooterText = "Да - Revit перезапустится и плагин обновится прямо сейчас. Нет - плагин обновится сразу после того, как вы закроете программу";
                td.AllowCancellation = false;
                td.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;                

                TaskDialogResult tdResult = td.Show();
                switch (tdResult)
                {
                    case TaskDialogResult.Yes:
                        StartUpdaterService("-fromRevit -restart");
                        break;

                    case TaskDialogResult.No:
                        StartUpdaterService("-fromRevit");
                        break;
                }
            }
        }

        public static void StartUpdaterService(string argLine)
        {
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
            psi.FileName = @"cmd";
            psi.Arguments = "/C start " + UpdaterPath + " " + argLine;
            psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            if (File.Exists(UpdaterPath)) Process.Start(psi);
            else TaskDialog.Show("Ошибка", "Программа обновления отсутствует на сервере. Обновитесь самостоятельно");
        }
    }


    [Transaction(TransactionMode.Manual)]
    public class SettingsWindow : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var form = new UI.SettingsForm(commandData.Application.ActiveUIDocument.Document);
            return Result.Succeeded;
        }
    }


    static public class TerrSettings
    {
        // Настройки для поиска радиатора
        public static List<int> RadiatorLengths { get; set; } = new List<int> { 400,500,600,700,800,900,1000,1100,1200,1400,1600,1800,2000,2300,2600,3000 };
        public static List<int> RadiatorHeights { get; set; } = new List<int> { 300,400,450,500,550,600,900 };
        public static List<int> RadiatorTypes { get; set; } = new List<int> { 10, 11, 20, 22, 30, 33 };
    }
}
