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
            List<string> usedNames = new List<string>();
            var grids = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Grids).ToElements();
            foreach (var grid in grids)
            {
                usedNames.Add(grid.Name.ToString());
            }
            GridAxesForm form = new GridAxesForm(usedNames);
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
                bool userChoice = form.userChoice;
                if (userChoice == true)
                {
                    x = form.X;
                    y = form.Y;
                }
                else
                {
                    XYZ userXYZ = gc.GettingXYZFromUser(uidoc);
                    x = userXYZ.X * 304.8;
                    y = userXYZ.Y * 304.8;
                    /*x = UnitUtils.ConvertToInternalUnits(userXYZ.X, DisplayUnitType.DUT_MILLIMETERS);
                    y = UnitUtils.ConvertToInternalUnits(userXYZ.Y, DisplayUnitType.DUT_MILLIMETERS);*/
                }

                using (Transaction trans = new Transaction(doc))
                {
                    trans.Start("Creating a first horisontal and vertical grids");
                    gc.CreateAGrids2(HorisontalIndentValues, VerticalIndentValues, 
                        VerticalNameValues, HorisontalNameValues, doc, uiapp, x, y);
                    trans.Commit();
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
        public void CreateAGrids2(List<object> horIndentsVal, List<object> vertIndentsVal,
            List<object> VerticalNameValues, List<object> HorisontalNameValues,
            Document doc, UIApplication uiapp, double x, double y)
        {
            // Создание вертикальных осей.
            int vertNamesCounter = 0;
            foreach (var item in vertIndentsVal)
            {
                int vertIndentVal = Convert.ToInt32(item);
                int horIndentVal = Convert.ToInt32(horIndentsVal.Last());
                XYZ lineStartPoint = new XYZ((x + vertIndentVal) / 304.8, (y - defaultLowIndent) / 304.8 , 0);
                XYZ lineEndPoint = new XYZ((x + vertIndentVal) / 304.8, (y + horIndentVal + defaultTopIndent) / 304.8, 0);
                Line vertGridLine = Line.CreateBound(lineStartPoint, lineEndPoint);
                Grid vertGrid = Grid.Create(doc, vertGridLine);
                vertGrid.Name = VerticalNameValues[vertNamesCounter].ToString();
                vertNamesCounter++;
            }
            // Создание горизонтальных осей.
            int horNamesCounter = 0;
            foreach (var item in horIndentsVal)
            {
                int horIndentVal = Convert.ToInt32(item);
                int vertIndentVal = Convert.ToInt32(vertIndentsVal.Last());
                XYZ lineStartPoint = new XYZ((x - defaultLowIndent) / 304.8, (y + horIndentVal) / 304.8, 0);
                XYZ lineEndPoint = new XYZ((x + vertIndentVal + defaultTopIndent) / 304.8, (y + horIndentVal) / 304.8, 0);
                Line horGridLine = Line.CreateBound(lineStartPoint, lineEndPoint);
                Grid horGrid = Grid.Create(doc, horGridLine);
                horGrid.Name = HorisontalNameValues[horNamesCounter].ToString();
                horNamesCounter++;
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


