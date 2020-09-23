using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WF = System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Forms;
using Autodesk.Revit.UI.Selection;

namespace TerrTools.UI
{

    public partial class GridAxesForm : WF.Form

    {
        // В этих списках содержатся значения шагов и отступов для осей.
        internal List<object> HorisontalIndentsResult { get; set; }
        internal List<object> VerticalIndentsResult { get; set; }
        internal List<object> HorisontalStepsResult { get; set; }
        internal List<object> VerticalStepsResult { get; set; }
        // В этих списках содержатся имена осей. 
        internal List<object> HorisontalNamesResult { get; set; }
        internal List<object> VerticalNamesResult { get; set; }
        // В этих переменных содержатся координаты вставки сетки осей.
        internal double X { get; set; }
        internal double Y { get; set; }
        internal double Z { get; set; }
        internal bool userChoice { get; set; }
        // Учитываем отсутствие некоторых букв в нормах оформления.
        private List<string> symbols = new List<string> {"А","Б","В","Г","Д","Е","Ж","И","К","Л","М","Н","П","Р",
        "С","Т","У","Ф","Ш","Э","Ю","Я" };
        private static int firstValue = 1;
        private static int stringValueIndex = 0;
        private static int NamesCounter = 0;
        // На тот случай, если форма закроется
        internal bool closedForm { get; set; } = false;
        public GridAxesForm()
        {
            InitializeComponent();
        }
        
        // Кнопка "Добавить" в горизонтальных осях.
        private void button2_Click(object sender, EventArgs e)
        {
            // Для автоскролла таблицы при добавлении большого кол-ва строк.
            int firstDisplayed = dataGridView1.FirstDisplayedScrollingRowIndex;
            int displayed = dataGridView1.DisplayedRowCount(true);
            int lastVisible = (firstDisplayed + displayed) - 1;
            int lastIndex = dataGridView1.RowCount - 1;
            if (dataGridView1.Rows.Count==0)
            {
                dataGridView1.Rows.Add(firstValue, null, 0);
            }
            else
            {
                dataGridView1.Rows.Add(firstValue, null, null);
            }
            // Для автоскролла таблицы при добавлении большого кол-ва строк.
            if (lastVisible == lastIndex)
            {
                dataGridView1.FirstDisplayedScrollingRowIndex = firstDisplayed + 1;
            }

            firstValue += 1;
        }

        // Кнопка "Удалить" в горизонтальных осях.
        private void button3_Click(object sender, EventArgs e)
        {
            var currentCell = dataGridView1.CurrentCell;
            if (currentCell != null)
            {
                var lastRowIndex = currentCell.RowIndex;
                dataGridView1.Rows.RemoveAt(lastRowIndex);
            }
            
        }

        // Кнопка "Добавить" в вертикальных осях.
        private void button1_Click(object sender, EventArgs e)
        {
            // Для автоскролла таблицы при добавлении большого кол-ва строк.
            int firstDisplayed = dataGridView2.FirstDisplayedScrollingRowIndex;
            int displayed = dataGridView2.DisplayedRowCount(true);
            int lastVisible = (firstDisplayed + displayed) - 1;
            int lastIndex = dataGridView2.RowCount - 1;

            if (symbols[stringValueIndex] == "Я" || symbols[stringValueIndex] == $"Я/{NamesCounter}")
            {
                dataGridView2.Rows.Add(symbols[stringValueIndex], null, null);
                NamesCounter += 1;
                string nc = NamesCounter.ToString();
                symbols = new List<string> {$"А/{nc}",$"Б/{nc}",$"В/{nc}",$"Г/{nc}", $"Д/{nc}", $"Е/{nc}", $"Ж/{nc}", $"И/{nc}",
                    $"К/{nc}", $"Л/{nc}", $"М/{nc}", $"Н/{nc}", $"П/{nc}", $"Р/{nc}",
                     $"С/{nc}", $"Т/{nc}", $"У/{nc}", $"Ф/{nc}", $"Ш/{nc}", $"Э/{nc}", $"Ю/{nc}", $"Я/{nc}" };
                stringValueIndex = 0;

            }
            else
            {
                if (dataGridView2.Rows.Count == 0)
                {
                    dataGridView2.Rows.Add(symbols[stringValueIndex], null, 0);
                    stringValueIndex += 1;
                }
                else
                {
                    dataGridView2.Rows.Add(symbols[stringValueIndex], null, null);
                    stringValueIndex += 1;
                }
            }
            if (lastVisible == lastIndex)
            {
                dataGridView2.FirstDisplayedScrollingRowIndex = firstDisplayed + 1;
            }

            /*if (symbols[ind] != "Я" || symbols[ind] != $"Я{NamesCounter}")
            {
                symbols.Remove(symbols[ind]);
            }*/

        }

        // // Кнопка "Удалить" в вертикальных осях.
        private void button4_Click(object sender, EventArgs e)
        {
            // Поправить название 
            var currentCell2 = dataGridView2.CurrentCell;
            if (currentCell2 != null)
            {
                var lastRowIndex = currentCell2.RowIndex;
                dataGridView2.Rows.RemoveAt(lastRowIndex);
            }
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }



        // Кнопка "Создать". 
        // В Result заносит список, в котором указан отступ осей.
        private void button5_Click(object sender, EventArgs e)
        {
            // В horisontalIndents хранятся данные из столбца Column2
            List<object> horisontalIndents = new List<object>();
            // В verticalIndents хранятся данные из столбца Column2
            List<object> verticalIndents = new List<object>();
            // В horisontalSteps хранятся данные из столбца Column3
            List<object> horisontalSteps = new List<object>();
            // В verticalSteps хранятся данные из столбца Column3
            List<object> verticalSteps = new List<object>();
            // В HorisontalNames хранятся данные из столбца Column1
            List<object> HorisontalNames = new List<object>();
            // В VerticalNames хранятся данные из столбца Column1
            List<object> VerticalNames = new List<object>();
            // Горизонтальные оси
            // Имена осей
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                HorisontalNames.Add(dataGridView1[0, i].Value);
            }
            // Отступы
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                horisontalIndents.Add(dataGridView1[1, i].Value);
            }
            // Шаги
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                horisontalSteps.Add(dataGridView1[2, i].Value);
            }
            // Горизонтальные имена.
            HorisontalNamesResult = HorisontalNames;
            // Горизонтальные отступы.
            HorisontalIndentsResult = horisontalIndents;
            // Горизонтальные шаги.
            HorisontalStepsResult = horisontalSteps;

            // Вертикальные оси
            // Имена осей
            for (int i = 0; i < dataGridView2.Rows.Count; i++)
            {
                VerticalNames.Add(dataGridView2[0, i].Value);
            }
            // Отступы
            for (int i = 0; i < dataGridView2.Rows.Count; i++)
            {
                verticalIndents.Add(dataGridView2[1, i].Value);
            }
            // Шаги
            for (int i = 0; i < dataGridView2.Rows.Count; i++)
            {
                verticalSteps.Add(dataGridView2[2, i].Value);
            }
            // Вертикальные имена.
            VerticalNamesResult = VerticalNames;
            // Вертикальные отступы.
            VerticalIndentsResult = verticalIndents;
            // Вертикальные шаги.
            VerticalStepsResult = verticalSteps;
            

            // Обрабатываем исключения при заполнении формы.
            try
            {
                // Если стоит галка по координатам, то
                // для вставки берутся координаты, указанные 
                // в форме.
                if (checkBox1.Checked == true)
                {
                    userChoice = true;
                    X = Convert.ToInt32(textBox1.Text);
                    Y = Convert.ToInt32(textBox2.Text);
                    Z = Convert.ToInt32(textBox3.Text);
                }
                else
                {
                    userChoice = false;
                }
                // Неверный отступ для первых двух осей.
                if (Convert.ToInt32(horisontalIndents[0]) != 0
                    || Convert.ToInt32(verticalIndents[0]) != 0)
                {
                    throw new Exception("Шаг/отступ для первых осей должен быть равен 0.");
                }

                // Указан пустой шаг осей.
                bool nullVertCounter = false;
                bool nullHorCounter = false;
                foreach (var item in verticalSteps)
                {
                    if (item == null)
                    {
                        nullVertCounter = true;
                    }
                }
                foreach (var item in horisontalSteps)
                {
                    if (item == null)
                    {
                        nullHorCounter = true;
                    }
                }
                if (nullVertCounter == true && nullHorCounter == true)
                {
                    throw new Exception("Для создаваемых осей не указан шаг.");
                }





                // Исключение, когда конечный отступ не совпал с суммой шагов.
                // Вспомогательные переменные для циклов.
                var count1 = dataGridView1.Rows.Count;
                var count2 = dataGridView2.Rows.Count;
                int i = 0;
                int l = 0;
                for (int j = 0; j < dataGridView1.Rows.Count; j++)
                    i += Convert.ToInt32(dataGridView1[2, j].Value);
                for (int j = 0; j < dataGridView2.Rows.Count; j++)
                    l += Convert.ToInt32(dataGridView2[2, j].Value);

                if (Convert.ToInt32(dataGridView1.Rows[count1 - 1].Cells[1].Value) != i ||
                    Convert.ToInt32(dataGridView2.Rows[count2 - 1].Cells[1].Value) != l)
                {
                    throw new Exception("Введенные значения оступов и/или шагов некорректны.\n Перепроверьте значения.");
                }
                this.DialogResult = WF.DialogResult.OK;
                this.Close();
            }
            // Случай, когда пользователь по какой-то причине
            // некорректно ввел отступ.
            catch (NullReferenceException)
            {
                TaskDialog.Show("Title2", "В заполненных данных обнаружены ошибки,\n" +
                    "перепроверьте введенные данные.");
            }
            catch (ArgumentOutOfRangeException)
            {
                TaskDialog.Show("Ошибка!", "Добавьте минимальное количество осей!");
            }
            // Обработка исключения, когда
            // пользователь пытается ввести строку вместо числа.
            catch (System.FormatException)
            {
                TaskDialog.Show("Ошибка!", "Некорректно заданы координаты вставки сетки осей.");
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Ошибка!", $"{ex.Message}");
            }
        }
        // Обработка события, когда пользователь
        // изменяет значение ячейки в dataGridView1
        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // Номер строки для ячейки
                var item = dataGridView1.Rows[e.RowIndex].Cells[0].Value;
                // Значение ячейки
                int celValue = Convert.ToInt32(dataGridView1.CurrentCell.Value);
                int columnIndex = dataGridView1.CurrentCell.ColumnIndex;
                int rowIndex = dataGridView1.CurrentRow.Index;
                if (columnIndex == 2 && rowIndex > 0)
                {
                    if (dataGridView1.Rows[rowIndex - 1].Cells[columnIndex - 1].Value == null)
                    {
                        dataGridView1.Rows[rowIndex - 1].Cells[columnIndex - 1].Value = 0;
                    }
                    int previousVal = Convert.ToInt32(dataGridView1.Rows[rowIndex - 1].Cells[columnIndex - 1].Value);
                    dataGridView1.Rows[rowIndex].Cells[columnIndex - 1].Value = celValue + previousVal;
                }

            }
            // Обработка исключения, когда
            // пользователь пытается ввести строку вместо числа.
            catch (System.FormatException)
            {
            }
            
        }


        // Обработка события, когда пользователь
        // изменяет значение ячейки в dataGridView2
        private void dataGridView2_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // Номер строки для ячейки
                var item = dataGridView2.Rows[e.RowIndex].Cells[0].Value;
                // Значение ячейки
                int celValue = Convert.ToInt32(dataGridView2.CurrentCell.Value);
                int columnIndex = dataGridView2.CurrentCell.ColumnIndex;
                int rowIndex = dataGridView2.CurrentRow.Index;
                if (columnIndex == 2 && rowIndex > 0)
                {
                    if (dataGridView2.Rows[rowIndex - 1].Cells[columnIndex - 1].Value == null)
                    {
                        dataGridView2.Rows[rowIndex - 1].Cells[columnIndex - 1].Value = 0;
                    }
                    int previousVal = Convert.ToInt32(dataGridView2.Rows[rowIndex - 1].Cells[columnIndex - 1].Value);
                    dataGridView2.Rows[rowIndex].Cells[columnIndex - 1].Value = celValue + previousVal;
                }
            }
            // Обработка исключения, когда
            // пользователь пытается ввести строку вместо числа.
            catch (System.FormatException)
            {
            }
        }

        private void checkBox2_MouseClick(object sender, MouseEventArgs e)
        {
            if (checkBox2.Checked)
            {
                checkBox1.Checked = false;
            }
            else
            {
                checkBox1.Checked = true;
            }
            
        }

        private void checkBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (checkBox1.Checked)
            {
                checkBox2.Checked = false;
            }
            else
            {
                checkBox2.Checked = true;
            }
        }
      

        // Кнопка "Обновить нумерацию".
        private void button6_Click(object sender, EventArgs e)
        {
            symbols = new List<string> {"А","Б","В","Г","Д","Е","Ж","И","К","Л","М","Н","П","Р",
            "С","Т","У","Ф","Ш","Э","Ю","Я" };
            firstValue = 1;
            stringValueIndex = 0;
        }

        private void GridAxesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Refresh();
        }
    }
}
