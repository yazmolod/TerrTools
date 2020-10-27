using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using TerrTools.UI;

namespace TerrTools
{
    [Transaction(TransactionMode.Manual)]
    class IzometryGenerator : IExternalCommand
    {
        /// >> Саша: используй свойства (или переменные класса, но с ними сложнее дебажить)
        /// потому что ты эти переменные используешь постоянно в разных функциях.
        /// Вместо того, чтобы передавать их из функции в функции, просто обращаемся к свойству
        Document Doc { get => UIDoc.Document; }
        UIDocument UIDoc { get; set; }
        List<View3D> Existing3DViews { get; set; }
        /// >> Саша: а если тебе нужно какое то свойство элементов из списка, 
        /// можно вот так быстро реализовать это через свойства. К тому же оно будет 
        /// зависимым от изначального списка, поэтому ты не сможешь по-разному
        /// модифицировать и выстрелить себе в ногу этим
        List<string> Existing3DView_Names { get => Existing3DViews.Select(x => x.Name).ToList();  }
        List<View3D> Created3DViews { get; set; } = new List<View3D>();
        List <string> SystemNames { get; set; }
        List<View3D> ExcessViews { get; set; } = new List<View3D>();


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Получаем документ
            UIDoc = commandData.Application.ActiveUIDocument;
            // В Existing3DViews хранятся 3D-виды, существующие в проекте.
            Existing3DViews = Get3DViewNames();
            var viewTypes = GetViewTypes();
            var viewTemplates = GetViewTemplates();
            // Все имена систем.
            SystemNames = GetSystemNames();
            if (SystemNames.Count == 0 )
            {
                return Result.Cancelled;
            }

            IzometryGeneratorForm form = new IzometryGeneratorForm(SystemNames, Existing3DView_Names, viewTypes, viewTemplates);
            if (form.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                return Result.Cancelled;
            }

            SystemNames = form.Result;
            using (TransactionGroup transGroup = new TransactionGroup(Doc, "Создание изометрий"))
            {
                transGroup.Start();
                using (Transaction trans = new Transaction(Doc))
                {
                    trans.Start("Создание новых видов");
                    foreach (var sysName in SystemNames)
                    {
                        if (sysName != null)
                        {
                            CreateView(sysName, form.ViewTypeId, form.ViewTemplateId, form.ReplaceUsedViews);
                        }
                    }
                    trans.Commit();

                    /// При попытке удалить активный вид выкидывается Autodesk.Revit.Exceptions ArgumentException 
                    /// поэтому перед удалением старых видов меняем активный вид на любой новосозданный.
                    /// Загвоздка в том, что делать это можно только вне транзакции, поэтому делаем смену вида между транзакциями
                    UIDoc.ActiveView = Created3DViews[0];

                    // удаляем отложенные старые виды
                    trans.Start("Удаление старых видов");
                    Doc.Delete(ExcessViews.Select(x => x.Id).ToList());
                    trans.Commit();
                }
                transGroup.Assimilate();
            }
            return Result.Succeeded;
        }


        // Метод для получения списка
        // существующих 3D-видов.
        private List<View3D> Get3DViewNames()
        {
            // Существующие в проекте 3D-виды.
            List<View3D> views = new FilteredElementCollector(Doc).OfClass(typeof(View3D))
                .WhereElementIsNotElementType().Cast<View3D>().Where(x => !x.IsTemplate).ToList();
            return views;
        }

        // Метод для получения имен систем.
        private List<string> GetSystemNames()
        {
            List<string> names = new List<string>();
            var systems = new FilteredElementCollector(Doc).OfClass(typeof(MEPSystem)).WhereElementIsNotElementType().ToElements();
            if (systems.Count > 0 && systems[0].LookupParameter("ТеррНИИ_Наименование системы") != null)
            {
                foreach (var item in systems)
                {
                    names.Add(item.LookupParameter("ТеррНИИ_Наименование системы").AsString());
                }
                // По какой-то причине залетают дубликаты,
                // удаляем их.
                return names.Distinct().ToList();
            }
            else
            {
                TaskDialog.Show("Ошибка", "В проекте не найдено ни одной системы");
                return names;
            }
        }
        // Метод для получения айди общего параметра.
        // не забыть при общем вызове методов проверить на null!
        private ElementId GetSharedParameterId()
        {
            var shared = new FilteredElementCollector(Doc).OfClass(typeof(SharedParameterElement)).ToElements();
            List<Element> param = new List<Element>();
            foreach (var item in shared)
            {
                if (item.Name == "ТеррНИИ_Наименование системы")
                {
                    param.Add(item);
                }
            }
            if (param.Count > 0)
            {
                ElementId paramId = param[0].Id;
                return paramId;
            }
            else
            {
                ElementId paramId = null;
                return paramId;
            }
        }
        // Метод для создания фильтра
        private ParameterFilterElement CreateFilter(string systemName)
        {
            // Получаем список фильтров в документе.
            List<ParameterFilterElement> docFiltersList = new FilteredElementCollector(Doc).OfClass(typeof(ParameterFilterElement))
                                                          .Cast< ParameterFilterElement>().ToList();
            // Содержит имена существующих в проекте фильтров.
            List<string> docNamesFiltersList = docFiltersList.Select(x => x.Name).ToList();

            // Необходимые для работы категории.
            List<BuiltInCategory> categories = new List<BuiltInCategory>() {
                BuiltInCategory.OST_DuctAccessory,
                BuiltInCategory.OST_PipeAccessory,
                BuiltInCategory.OST_DuctCurves,
                BuiltInCategory.OST_PlaceHolderDucts,
                BuiltInCategory.OST_DuctTerminal,
                BuiltInCategory.OST_FlexDuctCurves,
                BuiltInCategory.OST_FlexPipeCurves,
                BuiltInCategory.OST_DuctInsulations,
                BuiltInCategory.OST_PipeInsulations,
                BuiltInCategory.OST_MechanicalEquipment,
                BuiltInCategory.OST_DuctSystem,
                BuiltInCategory.OST_DuctFitting,
                BuiltInCategory.OST_PipeFitting,
                BuiltInCategory.OST_PipeCurves,
                BuiltInCategory.OST_GenericModel
            };
            // Получаем Id из BuiltInCategory в списке categories.
            List<ElementId> categoriesIds = categories.Select(x => Category.GetCategory(Doc, x).Id).ToList();

            // Id общего параметра(ТеррНИИ_Наименование системы).
            ElementId parameterId = GetSharedParameterId();
            // Правила.
            var rule = ParameterFilterRuleFactory.CreateNotEqualsRule(parameterId, systemName, true);
            var elementFilter = new ElementParameterFilter(rule);
            // Имя фильтра.
            string filterName = systemName + "_terrPlugin";
            ParameterFilterElement filter = null;
            // Проверяет, уникально ли имя systemName.
            // Если да, то создает новое имя для фильтра
            // с префиксом _terrPlugin, в обратном случае
            // использует уже существующее.
            // проверять не методом IsNameUnique,
            // а сверять по списку docFiltersList!!! ***            
            if (docNamesFiltersList.Contains(filterName))
            {
                foreach (var item in docFiltersList)
                {
                    if (item.Name == filterName)
                    {
                        filterName = item.Name;
                        // Используем уже имеющийся фильтр. ***
                        filter = (ParameterFilterElement)item;
                    }
                }
            }
            else
            {
                // Создаем новый, т.к. его ещё нету.
                if (filterName != null)
                {
                    // Создаем фильтр. ***
                    filter = ParameterFilterElement.Create(Doc, filterName, categoriesIds, elementFilter);
                }
            }            
            return filter;
        }

        // Метод для добавления фильтра.
        private void AddFilter(View view, string systemName)
        {
            // сюда надо имя системы без префиксов
            var filter = CreateFilter(systemName);
            view.AddFilter(filter.Id);
            view.SetFilterVisibility(filter.Id, false);
        }

        // Метод для установки ориентации 3D-вида.
        private void SetOrientation(View3D view)
        {
            var eyePosition = new XYZ(500, 50, 200);
            var upDirection = new XYZ(-1, 1, 2);
            var forwardDirection = new XYZ(-1, 1, -1);
            var orientation = new ViewOrientation3D(eyePosition, upDirection, forwardDirection);
            view.SetOrientation(orientation);
            view.SaveOrientationAndLock();
        }        

        // Метод для создания видов.
        private void CreateView(string systemName, ElementId viewType, ElementId viewTemplate, bool changeUsedViews)
        {
            string filterName = systemName;
            View3D view = View3D.CreateIsometric(Doc, viewType);

            if (!Existing3DView_Names.Contains(systemName))
            {
                view.Name = systemName;
            }
            else
            {
                // Если пользователь поставил галку на 
                // автоматическую замену существующих
                // 3D-видов, то будет происходить удаление
                // существующего вида и создание нового с таким же
                // названием.
                if (changeUsedViews == true)
                {
                    CreateInsteadOldView(view, systemName);
                }
                // Если галочка на автоматической замене видов не стоит,
                // то, если 3D-вид для создаваемой системы существует, 
                // пользователю будет дан выбор, как поступить:
                // создать с заменой вида, создать с сохранением старого вида(префикс),
                // либо не создавать новый вид, оставив предыдущий.
                else
                {
                    TaskDialog td = new TaskDialog("Внимание");
                    td.MainInstruction = $"Внимание! В проекте для системы '{systemName}' уже есть 3D-вид.";
                    td.MainContent = "Для продолжения выберите один из трёх вариантов:" +
                        "\n - \"С заменой\". Новый вид будет создан, заменяя старый;" +
                        "\n - \"Без замены\". Создать новый вид, сохранив старый;" +
                        "\n - \"Пропустить\". Новый вид не будет создан;";
                    td.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "С заменой");
                    td.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Без замены");
                    td.AddCommandLink(TaskDialogCommandLinkId.CommandLink3, "Пропустить");
                    td.AllowCancellation = false;
                    var result = td.Show();

                    if (result == TaskDialogResult.CommandLink1)
                    {
                        CreateInsteadOldView(view, systemName);
                    }
                    else if (result == TaskDialogResult.CommandLink2)
                    {
                        CreateOverOldView(view, systemName);
                    }
                    else
                    {
                        Doc.Delete(view.Id);
                        return;
                    }
                }
            }
            // Устанавливаем ориентацию вида.
            SetOrientation(view);
            // добавляем шаблон
            if (viewTemplate != ElementId.InvalidElementId)
            {
                view.ViewTemplateId = viewTemplate;
            }
            else
            {
                view.LookupParameter("Подкатегория")?.Set("Сгенерированные изометрии");
            }            
            // Устанавливаем фильтр для вида.
            AddFilter(view, filterName);            
            // Заносим в подкатегорию
            
            // и вид в свойства, чтобы можно было обратиться при необходимости
            Created3DViews.Add(view);
        }

        // Создание с суффиксом.
        private void CreateOverOldView(View view, string systemName)
        {
            if (Existing3DView_Names.Contains(systemName))
            {
                int counter = 1;
                string systemNameSuffixed = systemName;
                while (Existing3DView_Names.Contains(systemNameSuffixed))
                {
                    /// >> Саша: вот такая проверка - выстрел себе в ногу
                    /// потому что могут теоретически и двухзначные числа
                    //if (systemName.Contains("_"))
                    //{
                    //systemName = systemName.Remove(systemName.Length - 2, 2);
                    //}
                    systemNameSuffixed = String.Concat(systemName, "_", counter);
                    counter++;
                }
                view.Name = systemNameSuffixed;
            }
            else
            {
                view.Name = systemName;
            }
        }

        // Создание с "перезаписью".
        private void CreateInsteadOldView(View view, string systemName)
        {
            foreach (var item in Existing3DViews.Where(x => x.Name == systemName))
            {
                /// >> Саша: удалять на ходу во время транзакции элементы,
                /// которые ко всему прочему находятся в списке, который ты итерируешь,
                /// не очень хорошая идея - выкинет ошибку, если ты обратишься к удаленному элементу
                /// в той же транзакции
                /// Поэтому применяем следующую логику - переименовываем данный вид рандомным именем 
                /// (чтобы не мешать уникальности именам) и откладываем вид в список на удаление потом
                item.Name = new Random().Next().ToString();
                ExcessViews.Add(item);
            }
            view.Name = systemName;
        }

        // Метод для получения шаблона вида из проекта.
        private List<View3D> GetViewTemplates()
        {
            var views = new FilteredElementCollector(Doc).OfClass(typeof(View3D))
                            .Cast<View3D>().Where(x => x.IsTemplate).ToList();
            return views;
        }

        // Метод для получения Id 3D-вида.
        private List<ViewFamilyType> GetViewTypes()
        {
            var views = new FilteredElementCollector(Doc).OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>().Where(x => x.ViewFamily == ViewFamily.ThreeDimensional).ToList();
            return views;
        }
    }
}