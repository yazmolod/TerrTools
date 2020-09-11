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

namespace TerrTools.Commands
{
    [Transaction(TransactionMode.Manual)]
    class ColumnFinish : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // Получаем uiapp
                UIApplication uiapp = commandData.Application;
                // Получаем документ
                Document doc = uiapp.ActiveUIDocument.Document;
                // Получаем все помещения из документа
                IList<Element> rooms = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rooms).WhereElementIsNotElementType().ToElements();
                // Получаем стены из документа(для выбора типа стены)
                IList<Element> walls = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Walls).WhereElementIsElementType().ToElements();
                // Нужно для работы метода GetBoundarySegments

                // Работа с windows forms
                ColumnFinishForm form = new ColumnFinishForm(walls);
                System.Windows.Forms.DialogResult r = form.ShowDialog();
                // wall_type - это элемент(тип стены), выбранный пользователем
                Element wall_type = form.Result;
                if (wall_type != null)
                {
                    // Нужен для работы метода GetBoundarySegments
                    var spatial = new SpatialElementBoundaryOptions();
                    foreach (Room room in rooms)
                    {
                        // Получаем все BoundarySegments в помещении
                        var bs = room.GetBoundarySegments(spatial);
                        // Создаем список для хранения в нем BoundarySegments колонн, которые
                        // находятся(или частично находятся) в помещении
                        List<BoundarySegment> bs_columns = new List<BoundarySegment>();
                        foreach (var elem in bs)
                        {
                            foreach (var el in elem)
                            {
                                ElementId id_element = el.ElementId;
                                Element element = doc.GetElement(id_element);
                                if (element != null)
                                {
                                    // Тут идет проверка на принадлежность элемента к категории 
                                    // колонн(архитектурных: -2000100, несущих: -2001330)
                                    string element_category = element.Category.Id.ToString();
                                    if (element_category == "-2000100" || element_category == "-2001330")
                                    {
                                        bs_columns.Add(el);
                                    }
                                }
                            }
                        }

                        // Обьявляем переменную со значением половины толщины стены,
                        // это нужно для того, чтобы штукатурка примыкала именно к 
                        // колонне, а не входила внутрь
                        Double width = wall_type.LookupParameter("Ширина").AsDouble() / 2;


                        Transaction trans = new Transaction(doc);
                        trans.Start("Создание отделки колонн");

                        foreach (var bs_column in bs_columns)
                        {
                            // Получаем сам элемент(колонну) из BoundarySegment. 
                            // Этот процесс можно было бы оптимизировать, исключив из списка 
                            // касающиеся друг друга segment'ы, но в целом это не особо нужно
                            Element our_element = doc.GetElement(bs_column.ElementId);
                            // Получаем базовый уровень, к которому привязана колонна
                            ElementId level_id = our_element.LevelId;
                            // Получаем верхний уровень, к которому привязана колонна
                            ElementId upper_level = our_element.LookupParameter("Верхний уровень").AsElementId();
                            // Получаем линию от BoundarySegment колонны
                            Curve line = bs_column.GetCurve();
                            // Получаем колонну, которой принадлежит BoundarySegment
                            Element col = doc.GetElement(bs_column.ElementId);
                            // Получаем текущие значения смещений колонны сверху и снизу
                            Double top_offset = col.LookupParameter("Смещение сверху").AsDouble();
                            Double bot_offset = col.LookupParameter("Смещение снизу").AsDouble();
                            // xyz(направление смещения) для работы метода CreateOffset
                            XYZ xyz = new XYZ(0, 0, -1);
                            // Создаем кривую, смещенную от линии BoundarySegment колонны 
                            // На половину толщины стены(чтобы стена-штукатурка не заходила внутрь колонны)
                            Curve line_2 = line.CreateOffset(width / 2, xyz);

                            // Создаем стену(ненесущую)
                            Wall created_wall = Wall.Create(doc, line_2, wall_type.Id, level_id, 3000 / 304.8, 0, false, false);

                            // Устанавливаем для созданной стены нужные смещения,
                            // которые мы взяли с колонны
                            created_wall.LookupParameter("Зависимость сверху").Set(upper_level);
                            created_wall.LookupParameter("Смещение сверху").Set(top_offset);
                            created_wall.LookupParameter("Смещение снизу").Set(bot_offset);

                        }
                        trans.Commit();
                    }
                }
                else
                {
                }

            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
            return Result.Succeeded;
        }
    }
}
