using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace TerrTools
{
    /* Так как диалоговое окно просмотрщика не модальное
     * Revit теряет контекст (тред с API context),
     * поэтому мы не можем просто так совершать транзакции.
     * Специально для решения этой проблемы существует интерфейс
     * IExternalEventHandler, который здесь и реализован
     * 
     * Источники:
     * https://stackoverflow.com/questions/31490990/starting-a-transaction-from-an-external-application-running-outside-of-api-conte
     * https://help.autodesk.com/cloudhelp/2018/ENU/Revit-API/Revit_API_Developers_Guide/Advanced_Topics/External_Events.html
     */
    public class ShowRowEvent : IExternalEventHandler
    {
        public CollisionReportRow Row { get; set; }
        public System.Windows.Forms.Button Sender { get; set; }
        public void Execute(UIApplication app)
        {
            UIDocument uidoc = app.ActiveUIDocument;
            switch (Sender.Name)
            {
                case "selectButton":
                    CollisionViewer.SelectRowItems(uidoc, Row);
                    break;
                case "showButton":
                    CollisionViewer.LookAtRow(uidoc, Row);
                    break;
            }            
        }

        public string GetName()
        {
            return "ShowCollisionRowEvent";
        }
    }

    [Transaction(TransactionMode.Manual)]
    class CollisionViewer : IExternalCommand
    {
        /// <summary>
        /// Создает отдельный вид для просмотра коллизий
        /// или возвращает его, если он существует
        /// </summary>
        /// <returns></returns>
        static public View3D GetView(Document doc, string name)
        {
            View3D[] views = new FilteredElementCollector(doc).OfClass(typeof(View3D))
                .WhereElementIsNotElementType().ToElements().Cast<View3D>().ToArray();
            var v = views.Where(x => x.Name == name);
            if (v.Count() == 0)
            {
                var viewType = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).ToElements()
                            .Cast<ViewFamilyType>().Where(x => x.ViewFamily == ViewFamily.ThreeDimensional).First();
                View3D view;
                using (var tr = new Transaction(doc, "Создание 3D вида"))
                {
                    tr.Start();
                    view = View3D.CreateIsometric(doc, viewType.Id);
                    view.Name = name;
                    tr.Commit();
                }
                return view;
            }
            else
            {
                return v.First();
            }
        }

        static public void SelectRowItems(UIDocument uidoc, CollisionReportRow row)
        {
            CollisionUtils.GetElementsFromReportRow(uidoc.Document, row, 
                out ICollection<Element> sdoc, out ICollection<Element> slink, out RevitLinkInstance instance);
            uidoc.Selection.SetElementIds(sdoc.Cast<Element>().Select(x => x.Id).ToList());
        }        

        static public void ResetElementOverrides(Document doc)
        {
            throw new NotImplementedException();
        }

        static public void ColorRowElements(Document doc, ElementSet elements)
        {
            throw new NotImplementedException();
        }

        static public void LookAtRow(UIDocument uidoc, CollisionReportRow row)
        {
            CollisionUtils.GetElementsFromReportRow(uidoc.Document, row, 
                out ICollection<Element> sdoc, out ICollection<Element> slink, out RevitLinkInstance instance); 
            if (sdoc.Count()==0 && slink.Count()==0)
            {
                var td = new TaskDialog("Просмотр коллизий");
                td.MainInstruction = "Ни один элемент не найден ни в текущем, ни в связанном документах";
                td.MainContent = "Возможные причины:" +
                    "\n1) Отчет составлен по другим документам" +
                    "\n2) Элементы находятся в связи, которая не загружена в проект" +
                    "\n3) Элементы из отчета были удалены в проекте";
                td.Show();
                return;
            }

            // находим подходящий вид
            View3D view;
            if (uidoc.ActiveView.Name != "Просмотр коллизий")
            {
                view = GetView(uidoc.Document, "Просмотр коллизий");
                uidoc.ActiveView = view;
            }
            else
            {
                view = uidoc.ActiveView as View3D;
            }
            // в Naviswork система координат по внутренней площадке документа
            // поэтому необходимо и корректировать по местной системе координат
            ProjectLocation pl = uidoc.Document.ActiveProjectLocation;
            XYZ centerPoint = pl.GetTransform().OfPoint(row.Point);
            // создаем подрезку 3д вида                    
            double m = UnitUtils.ConvertToInternalUnits(1, DisplayUnitType.DUT_METERS);
            Outline outline = Intersection.CreateOutline(centerPoint, m, m, m);
            BoundingBoxXYZ bbox = new BoundingBoxXYZ();
            bbox.Min = outline.MinimumPoint;
            bbox.Max = outline.MaximumPoint;
            // обновляем вид
            using (var tr = new Transaction(uidoc.Document, "Обновление 3D вида"))
            {
                tr.Start();

                //граница 3д вида
                view.IsSectionBoxActive = true;
                view.SetSectionBox(bbox);
                // обрезка листа
                view.CropBoxActive = false;

                //!!
                // Фичу надо доделать, но для релизной версии можно оставить в таком виде
                //
                // переопределение графики для элемента    
                //OverrideGraphicSettings greenGraphics = new OverrideGraphicSettings();
                //Color green = new Color(0, 255, 0);
                //Color dark_green = new Color(0, 128, 0);
                //greenGraphics.SetSurfaceForegroundPatternColor(green);
                //greenGraphics.SetSurfaceBackgroundPatternColor(green);
                //greenGraphics.SetProjectionLineColor(green);
                //greenGraphics.SetCutBackgroundPatternColor(dark_green);
                //greenGraphics.SetCutForegroundPatternColor(dark_green);
                //greenGraphics.SetCutLineColor(dark_green);

                //OverrideGraphicSettings redGraphics = new OverrideGraphicSettings();
                //Color red = new Color(255, 0, 0);
                //Color dark_red = new Color(128, 0, 0);
                //redGraphics.SetSurfaceForegroundPatternColor(red);
                //redGraphics.SetSurfaceBackgroundPatternColor(red);
                //redGraphics.SetProjectionLineColor(red);
                //redGraphics.SetCutBackgroundPatternColor(dark_red);
                //redGraphics.SetCutForegroundPatternColor(dark_red);
                //redGraphics.SetCutLineColor(dark_red);
                //foreach (ElementId i in sdoc.Cast<Element>().Select(x => x.Id))
                //{
                //    view.SetElementOverrides(i, greenGraphics);
                //}
                //foreach (ElementId i in new FilteredElementCollector(uidoc.Document).OfClass(typeof(RevitLinkInstance)).WhereElementIsNotElementType().Select(x=>x.Id))
                //{
                //    view.SetElementOverrides(i, redGraphics);
                //}

                tr.Commit();
            }            
            // зумируем содержимое окна до границ
            foreach (var uiview in uidoc.GetOpenUIViews())
            {
                if (uiview.ViewId.Equals(view.Id))
                {
                    uiview.ZoomToFit();
                    break;
                }
            }
            
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var form = new UI.CollisionViewerForm();
            return Result.Succeeded;
        }
    }
}
