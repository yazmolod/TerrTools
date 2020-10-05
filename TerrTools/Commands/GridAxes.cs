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
    class GridAxes : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Получаем uiapp
            UIApplication uiapp = commandData.Application;
            // Получаем документ
            Document doc = uiapp.ActiveUIDocument.Document;
            // Получаем uidoc
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            GridAxesForm form = new GridAxesForm();
            System.Windows.Forms.DialogResult r = form.ShowDialog();
            if (r == System.Windows.Forms.DialogResult.Cancel)
            {
                return Result.Cancelled;
            }
            else
            {
                // В этом списке хранятся горизонтальные
                // значения отступов осей с формы
                List<object> HorisontalIndentValues = form.HorisontalIndentsResult;
                // В этом списке хранятся вертикальные
                // значения отступов осей с формы
                List<object> VerticalIndentValues = form.VerticalIndentsResult;
                // В этом списке хранятся горизонтальные
                // значения шагов осей с формы
                List<object> HorisontalStepValues = form.HorisontalStepsResult;
                // В этом списке хранятся вертикальные
                // значения шагов осей с формы
                List<object> VerticalStepValues = form.VerticalStepsResult;
                // В этом списке хранятся горизонтальные
                // значения имен осей
                List<object> HorisontalNameValues = form.HorisontalNamesResult;
                // В этом списке хранятся вертикальные
                // значения имен осей
                List<object> VerticalNameValues = form.VerticalNamesResult;


                // Объект GridsCreator
                GridsCreator gc = new GridsCreator();

                // Координаты вставки сетки осей,
                // приходящие из формы. userChoice - 
                // зависит от выбора пользователя в форме
                // (по каким координатам создавать сетку)
                double x;
                double y;
                double z;
                bool userChoice = form.userChoice;
                if (userChoice == true)
                {
                    x = form.X;
                    y = form.Y;
                    z = form.Z;
                }
                else
                {
                    XYZ userXYZ = gc.GettingXYZFromUser(uidoc);
                    x = UnitUtils.ConvertToInternalUnits(userXYZ.X, DisplayUnitType.DUT_MILLIMETERS);
                    y = UnitUtils.ConvertToInternalUnits(userXYZ.Y, DisplayUnitType.DUT_MILLIMETERS);
                    z = userXYZ.Z;
                }

                using (Transaction trans = new Transaction(doc))
                {
                    trans.Start("Creating a first horisontal and vertical grids");

                    ElementId FirstVertGridId;
                    ElementId FirstHorGridId;
                    // Создание первых двух осей.
                    gc.CreateAGrids(HorisontalIndentValues, VerticalIndentValues,
                        HorisontalStepValues, VerticalStepValues, doc, uiapp, out FirstVertGridId,
                        out FirstHorGridId, HorisontalNameValues, VerticalNameValues, x, y, z);
                    trans.Commit();

                    Transaction trans2 = new Transaction(doc);
                    trans2.Start("Copying a grids");

                    // Копирование двух созданных осей.
                    gc.CopyAGrids(HorisontalIndentValues, VerticalIndentValues, doc, uiapp,
                        FirstVertGridId, FirstHorGridId, HorisontalNameValues, VerticalNameValues);
                    trans2.Commit();
                }


                return Result.Succeeded;
            }
        }
    }
    class GridsCreator
    {
        // Для ГОСТовского отступа в начале оси.
        private double defaultLowIndent { get; } = 4200;
        // Для ГОСТовского отступа в конце оси.
        private double defaultTopIndent { get; } = 500;
        // Метод CreateAGrids создает первые две оси.
        public void CreateAGrids(List<object> horIndentsVal, List<object> vertIndentsVal,
            List<object> horStepsVal, List<object> vertStepsVal,
            Document doc, UIApplication uiapp, out ElementId FirstVertGridId,
            out ElementId FirstHorGridId, List<object> HorisontalNameValues,
            List<object> VerticalNameValues, double x, double y, double z)
        {
            // Для вертикальной оси
            double vertGridYstart = y - defaultLowIndent;
            XYZ firstVertGridStartPoint = new XYZ(x / 304.8, vertGridYstart / 304.8, z);
            // vertGridYend это y end для вертикальной оси.

            double vertGridYend = y + defaultTopIndent;
            foreach (object item in horStepsVal)
            {
                vertGridYend += Convert.ToInt32(item);
            }
            XYZ firstVertGridEndPoint = new XYZ(x / 304.8, vertGridYend / 304.8, z);
            Line FirstVertGridLine = Line.CreateBound(firstVertGridStartPoint, firstVertGridEndPoint);
            // Первая вертикальная ось.
            Grid FirstVertGrid = Grid.Create(doc, FirstVertGridLine);
            // Имя оси(первое из списка)
            var n1 = VerticalNameValues[0];
            try
            {
                FirstVertGrid.Name = n1.ToString();
            }
            catch (Autodesk.Revit.Exceptions.ArgumentException)
            {
            }
            // out айди для первой вертикальной оси
            FirstVertGridId = FirstVertGrid.Id;
            // Для горизонтальной оси.
            double horGridXstart = x - defaultLowIndent;
            XYZ firstHorGridStartPoint = new XYZ(horGridXstart / 304.8, y / 304.8, z);
            double vertGridXend = x + defaultTopIndent;
            foreach (object item in vertStepsVal)
            {
                vertGridXend += Convert.ToInt32(item);
            }
            XYZ firstHorGridEndPoint = new XYZ(vertGridXend / 304.8, y / 304.8, z);
            Line FirstHorGridLine = Line.CreateBound(firstHorGridStartPoint, firstHorGridEndPoint);
            // Первая горизонтальная ось.
            Grid FirstHorGrid = Grid.Create(doc, FirstHorGridLine);
            // Имя оси(первое из списка)
            var n2 = HorisontalNameValues[0];
            try
            {
                //Autodesk.Revit.Exceptions.ArgumentException
                FirstHorGrid.Name = n2.ToString();
            }
            catch (Autodesk.Revit.Exceptions.ArgumentException)
            {
            }
            
            // out айди для первой горизонтальной оси
            FirstHorGridId = FirstHorGrid.Id;
        }
        // Метод для копирования первых двух созданных осей.
        public void CopyAGrids(List<object> horVal, List<object> vertVal,
            Document doc, UIApplication uiapp, ElementId FirstVertGridId,
            ElementId FirstHorGridId, List<object> HorisontalNameValues,
            List<object> VerticalNameValues)
        {   
            object horToDel = HorisontalNameValues[0];
            object vertToDel = VerticalNameValues[0];
            HorisontalNameValues.Remove(horToDel);
            VerticalNameValues.Remove(vertToDel);

            // Копируем оси

            // Сначала вертикальные
            foreach (object item in vertVal)
            {
                int MoveByX = Convert.ToInt32(item);
                if (MoveByX != 0)
                {
                    // Смещение на нужный шаг
                    XYZ xyz = new XYZ(MoveByX / 304.8, 0, 0);
                    List<ElementId> elementId = ElementTransformUtils.CopyElement(doc, FirstVertGridId, xyz).ToList();
                    // Получаем только что созданную ось,
                    // чтобы иметь возможность изменить её имя.
                    Grid gr = (Grid)doc.GetElement(elementId[0]);
                    string vertUsableName = VerticalNameValues[0].ToString();
                    // Оборачиваем в try/catch для того, чтобы избегать исключения, 
                    // когда наименование оси(которое дефолтно ставится ревитом)
                    // совпадает с введенным пользователем
                    try
                    {
                        gr.Name = vertUsableName;
                    }
                    catch (Autodesk.Revit.Exceptions.ArgumentException)
                    {
                    }
                    
                    VerticalNameValues.Remove(vertUsableName);

                }
            }
            // Теперь горизонтальные
            foreach (object item in horVal)
            {
                int MoveByY = Convert.ToInt32(item);
                if (MoveByY != 0)
                {
                    // Смещение на нужный шаг
                    XYZ xyz = new XYZ(0, MoveByY / 304.8, 0);
                    List<ElementId> elementId = ElementTransformUtils.CopyElement(doc, FirstHorGridId, xyz).ToList();
                    // Получаем только что созданную ось,
                    // чтобы иметь возможность изменить её имя.
                    Grid gr = (Grid)doc.GetElement(elementId[0]);
                    string horUsableName = HorisontalNameValues[0].ToString();
                    // Оборачиваем в try/catch для того, чтобы избегать исключения, 
                    // когда наименование оси(которое дефолтно ставится ревитом)
                    // совпадает с введенным пользователем
                    try
                    {
                        gr.Name = horUsableName;
                    }
                    catch (Autodesk.Revit.Exceptions.ArgumentException)
                    {
                    }
                    
                    VerticalNameValues.Remove(horUsableName);
                }
            }
        }
        public XYZ GettingXYZFromUser(UIDocument uidoc)
        {
            Selection sel = uidoc.Selection;
            XYZ coords = sel.PickPoint();
            return coords;
        }
    }
    class DimensionsCreator
    {
        // Есть идея образмеривать созданные оси,
        // если пользователь нажмет нужный чекер на
        // форме, но Document не понимает куда ссылаться
        // на Creation или же DB.
        // Надо бы подумать как лучше с точки зрения 
        // организации кода
        internal void CreateADimensions()
        { 
        }
    }
}


