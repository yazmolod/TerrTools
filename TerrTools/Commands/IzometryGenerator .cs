using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
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
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Получаем uiapp
            UIApplication uiapp = commandData.Application;
            // Получаем документ
            Document doc = uiapp.ActiveUIDocument.Document;
            // Получаем uidoc
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            // Переменные вынесены в global scope для оптимизации
            // В views хранятся 3D-виды, существующие в проекте.
            IList<Element> views;
            var viewNames = Get3DViewNames(doc, out views);
            var viewType = GetViewType(doc);
            // Все имена систем.
            var systemsNames = GetSystemNames(doc);
            // Уже существующие системы.
            /*List<string> usedSystems = new List<string>();
            // надо додумать, как быть если название уже с префиксом? 
            // contains может привести к дублированию систем в форме!
            //
            foreach (var item in views)
            {
                foreach (var name in systemsNames)
                {
                    if (item.Name.Contains(name))
                    {
                        usedSystems.Add(name);
                    }
                }
            }*/
            IzometryGeneratorForm form = new IzometryGeneratorForm(systemsNames, viewNames);
            System.Windows.Forms.DialogResult r = form.ShowDialog();
            if (r == System.Windows.Forms.DialogResult.Cancel)
            {
                return Result.Cancelled;
            }
            systemsNames = form.Result;
            // Определяет, нужно ли автоматически заменять существующие виды
            // или нет.
            bool changeUsedViews = form.changeUsedViews;
            using (Transaction trans = new Transaction(doc, "Создание изометрий"))
            {
                trans.Start();
                foreach (var sysName in systemsNames)
                {
                    if (sysName != null)
                    {
                        CreateAView(doc, sysName, viewNames, viewType, changeUsedViews, views);
                    }
                }
                trans.Commit();
            }
            return Result.Succeeded;
        }

        // Метод для получения списка названий 
        // существующих 3D-видов.
        private List<string> Get3DViewNames(Document doc, out IList<Element> views)
        {
            // Названия существующих в проекте 3D-видов.
            List<string> viewNames = new List<string>();
            // Существующие в проекте 3D-виды.
            views = new FilteredElementCollector(doc).OfClass(typeof(View3D)).WhereElementIsNotElementType().ToElements();
            foreach (var item in views)
            {
                viewNames.Add(item.Name);
            }
            return viewNames;
        }
        // Метод для получения имен систем.
        private List<string> GetSystemNames(Document doc)
        {
            List<string> names = new List<string>();
            var systems = new FilteredElementCollector(doc).OfClass(typeof(MEPSystem)).WhereElementIsNotElementType().ToElements();
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
                return names;
                //TaskDialog.Show("Ошибка", "Не найдены системы или параметр ТеррНИИ_Наименование системы");
            }
        }
        // Метод для получения айди общего параметра.
        // не забыть при общем вызове методов проверить на null!
        private ElementId GetSharedParameterId(Document doc)
        {
            var shared = new FilteredElementCollector(doc).OfClass(typeof(SharedParameterElement)).ToElements();
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
        private ParameterFilterElement CreateAFilter(Document doc, string systemName)
        {
            // Получаем список фильтров в документе.
            var docFiltersList = new FilteredElementCollector(doc).OfClass(typeof(ParameterFilterElement)).ToElements();
            // Содержит имена существующих в проекте фильтров.
            List<string> docNamesFiltersList = new List<string>();
            foreach (var item in docFiltersList)
            {
                docNamesFiltersList.Add(item.Name);
            }
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
                BuiltInCategory.OST_PipeCurves
            };
            // Получаем Id из BuiltInCategory в списке categories.
            // но это не точно
            List<ElementId> categoriesIds = new List<ElementId>();
            foreach (var item in categories)
            {
                categoriesIds.Add(Category.GetCategory(doc, item).Id);
            }

            // Id общего параметра(ТеррНИИ_Наименование системы).
            ElementId parameterId = GetSharedParameterId(doc);
            // Правила.
            var rule = ParameterFilterRuleFactory.CreateNotEqualsRule(parameterId, systemName, true);
            var elementFilter = new ElementParameterFilter(rule);
            // Имя фильтра.
            string filterName = null;
            ParameterFilterElement filter = null;
            // Проверяет, уникально ли имя systemName.
            // Если да, то создает новое имя для фильтра
            // с префиксом _terrPlugin, в обратном случае
            // использует уже существующее.
            // проверять не методом IsNameUnique,
            // а сверять по списку docFiltersList!!! ***
            filterName = string.Concat(systemName, "_terrPlugin");
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
                    filter = ParameterFilterElement.Create(doc, filterName, categoriesIds, elementFilter);
                }
            }
            
            return filter;
        }
        // Метод для добавления фильтра.
        private View AddFilter(Document doc, View view, string systemName)
        {
            // сюда надо имя системы без префиксов
            var filter = CreateAFilter(doc, systemName);
            view.AddFilter(filter.Id);
            view.SetFilterVisibility(filter.Id, false);
            return view;
        }
        // Метод для установки ориентации 3D-вида.
        private View3D SetOrientation(View3D view)
        {
            var eyePosition = new XYZ(500, 50, 200);
            var upDirection = new XYZ(-1, 1, 2);
            var forwardDirection = new XYZ(-1, 1, -1);
            var orientation = new ViewOrientation3D(eyePosition, upDirection, forwardDirection);
            view.SetOrientation(orientation);
            view.SaveOrientationAndLock();
            return view;
        }
        // Метод для получения Id 3D-вида.
        private ElementId GetViewType(Document doc)
        {
            var views = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).ToElements();
            var viewtypeId = views.Where(x => x.Name == "3D вид").ToList();
            return viewtypeId[0].Id;
        }
        // Метод для создания видов.
        private void CreateAView(Document doc, string systemName, 
            List<string> viewNames, ElementId viewType, bool changeUsedViews, 
            IList<Element> views)
        {
            string filterName = systemName;
            View view = View3D.CreateIsometric(doc, viewType);

            // Если пользователь поставил галку на 
            // автоматическую замену существующих
            // 3D-видов, то будет происходить удаление
            // существующего вида и создание нового с таким же
            // названием.
            if (changeUsedViews == true)
            {
                // Удаление старого, создание нового.
                CreateInsteadTheOldView(doc, views, systemName, view);
                // Устанавливаем ориентацию вида.
                view = SetOrientation((View3D)view);
                // Устанавливаем фильтр для вида.
                // вероятно тут надо другое имя
                view = AddFilter(doc, view, filterName);
                var p = view.LookupParameter("Подкатегория");
                if (p != null)
                {
                    p.Set("Сгенерированные изометрии");
                }
            }
            // Если галочка на автоматической замене видов не стоит,
            // то, если 3D-вид для создаваемой системы существует, 
            // пользователю будет дан выбор, как поступить:
            // создать с заменой вида, создать с сохранением старого вида(префикс),
            // либо не создавать новый вид, оставив предыдущий.
            else
            {
                if (viewNames.Contains(systemName))
                {
                    IzometryUserChoiceForm form2 = new IzometryUserChoiceForm(systemName);
                    form2.ShowDialog();
                    if (form2.Result == "С заменой")
                    {
                        CreateInsteadTheOldView(doc, views, systemName, view);
                        // Устанавливаем ориентацию вида.
                        view = SetOrientation((View3D)view);
                        // Устанавливаем фильтр для вида.
                        // вероятно тут надо другое имя
                        view = AddFilter(doc, view, filterName);
                        var p = view.LookupParameter("Подкатегория");
                        if (p != null)
                        {
                            p.Set("Сгенерированные изометрии");
                        }
                    }
                    else if (form2.Result == "Без замены")
                    {
                        CreateOverTheOldView(view, systemName, viewNames);
                        // Устанавливаем ориентацию вида.
                        view = SetOrientation((View3D)view);
                        // Устанавливаем фильтр для вида.
                        // вероятно тут надо другое имя
                        view = AddFilter(doc, view, filterName);
                        var p = view.LookupParameter("Подкатегория");
                        if (p != null)
                        {
                            p.Set("Сгенерированные изометрии");
                        }
                    }
                    else
                    {
                        doc.Delete(view.Id);
                    }
                }
                // Если для данной системы нет 3D-вида, 
                // Создаем новый вид с названием имени системы.
                else
                {
                    view.Name = systemName;
                    // Устанавливаем ориентацию вида.
                        view = SetOrientation((View3D)view);
                        // Устанавливаем фильтр для вида.
                        // вероятно тут надо другое имя
                        view = AddFilter(doc, view, filterName);
                        var p = view.LookupParameter("Подкатегория");
                        if (p != null)
                        {
                            p.Set("Сгенерированные изометрии");
                        }
                }
            }
        }
        // Создание с префиксом.
        private void CreateOverTheOldView(View view, string systemName, List<string> viewNames)
        {
            if (viewNames.Contains(systemName))
            {
                int counter = 1;
                while (viewNames.Contains(systemName))
                {
                    if (systemName.Contains("_"))
                    {
                        systemName = systemName.Remove(systemName.Length - 2, 2);
                    }
                    systemName = String.Concat(systemName, "_", counter);
                    counter++;
                }
                view.Name = systemName;
            }
            else
            {
                view.Name = systemName;
            }
        }
        // Создание с "перезаписью".
        private void CreateInsteadTheOldView(Document doc, IList<Element> views, 
            string systemName, View view)
        {
            foreach (var item in views)
            {
                try
                {
                    if (item.Name == systemName)
                    {
                        doc.Delete(item.Id);
                        break;
                    }
                }
                catch (Exception)
                {
                }
                
            }
            view.Name = systemName;
        }
    }
}