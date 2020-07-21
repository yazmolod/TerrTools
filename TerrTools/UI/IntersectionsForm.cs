using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Reflection;

namespace TerrTools.UI
{
    public partial class IntersectionsForm : Form
    {
        private BaseIntersectionHandler Handler;
        public List<Intersection> Intersections { get; private set; } = new List<Intersection>();
        double minPipeSizeValue {get;set;}
        public IntersectionsForm(BaseIntersectionHandler handler)
        {
            Handler = handler;
            InitializeComponent();
            minPipeSizeValue = 150;
            UpdateTableValues();
            ShowDialog();
        }

        private void UpdateTableValues()
        {
            dataGridView1.Rows.Clear();
            foreach (Intersection i in Intersections)
            {
                int nRow = dataGridView1.Rows.Add();
                FillRow(i, nRow, true);
            }
            countLabel.Text = "Количество пересечений: " + Intersections.Count.ToString();
        }

        private void FillRow(Intersection i, int nRow, bool firstFill = false)
        {
            dataGridView1.Rows[nRow].Cells["IntersectionPoint"].Value = String.Format(
                "X: {0}, Y: {1}, Z: {2}",
                Math.Round(i.CenterPoint.X , 3),
                Math.Round(i.CenterPoint.Y, 3),
                Math.Round(i.CenterPoint.Z, 3)
                );
            dataGridView1.Rows[nRow].Cells["Level"].Value = i.Level.Name;
            dataGridView1.Rows[nRow].Cells["HostName"].Value = i.Host.Name;
            dataGridView1.Rows[nRow].Cells["HostId"].Value = i.Host.Id.IntegerValue;
            dataGridView1.Rows[nRow].Cells["PipeName"].Value = i.Pipe.Name;
            dataGridView1.Rows[nRow].Cells["PipeId"].Value = i.Pipe.Id.IntegerValue;
            dataGridView1.Rows[nRow].Cells["Offset"].Value = i.MinOffset * 304.8;
            dataGridView1.Rows[nRow].Cells["IsBrick"].Value = i.IsBrick;
            dataGridView1.Rows[nRow].Cells["HoleId"].Value = i.Id;
            dataGridView1.Rows[nRow].Cells["LevelOffset"].Value = i.LevelOffset * 304.8;
            dataGridView1.Rows[nRow].Cells["GroundOffset"].Value = i.GroundOffset * 304.8;
            dataGridView1.Rows[nRow].Cells["HoleSize"].Value = String.Concat(i.HoleWidth * 304.8, " x ", i.HoleHeight * 304.8, "h");

            if (i.IsRound) dataGridView1.Rows[nRow].Cells["PipeSize"].Value = String.Concat("Ø", i.PipeWidth * 304.8);
            else dataGridView1.Rows[nRow].Cells["PipeSize"].Value = String.Concat(i.PipeWidth * 304.8, " x ", i.PipeHeight * 304.8, "h");

            if (firstFill) dataGridView1.Rows[nRow].Cells["AddToProject"].Value = i.PipeWidth * 304.8 >= minPipeSizeValue;
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            var grid = sender as DataGridView;
            if (e.RowIndex != -1 && e.ColumnIndex != -1 && e.RowIndex < Intersections.Count())
            {
                string columnName = grid.Columns[e.ColumnIndex].Name;
                var value = grid.Rows[e.RowIndex].Cells[columnName].Value;
                switch (columnName)
                {
                    case "Offset":
                        double offset;
                        if (double.TryParse(value.ToString(), out offset)) Intersections[e.RowIndex].MinOffset = offset / 304.8;
                        else Intersections[e.RowIndex].MinOffset = 0;
                        break;
                    //// превратилось в read only
                    //case "IsBrick":
                    //    bool isBrick;
                    //    bool.TryParse(value.ToString(), out isBrick);
                    //    Intersections[e.RowIndex].IsBrick = isBrick;
                    //    break;
                    default:
                        break;
                }
                FillRow(Intersections[e.RowIndex], e.RowIndex);
            }
        }

        private void SetResult()
        {
            List<Intersection> filteredIntersections = new List<Intersection>();
            for (int i = 0; i < Intersections.Count; i++)
            {
                if (Convert.ToBoolean(dataGridView1.Rows[i].Cells["AddToProject"].Value) == true)
                {
                    filteredIntersections.Add(Intersections[i]);
                }
            }
            Intersections = filteredIntersections;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.SetResult();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void offsetTextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox offsetTB = sender as TextBox;
            double value;
            if (double.TryParse(offsetTB.Text, out value))
            {
                for (int i = 0; i < Intersections.Count; i++)
                {
                    Intersections[i].MinOffset = value / 304.8;
                    FillRow(Intersections[i], i);
                }
            }            
        }

        private void minSizeTextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox offsetTB = sender as TextBox;
            double value;
            if (double.TryParse(offsetTB.Text, out value))
            {
                minPipeSizeValue = value;
                for (int i = 0; i < Intersections.Count; i++)
                {
                    FillRow(Intersections[i], i, true);
                }
            }
        }

        private void analyzeBtn_Click(object sender, EventArgs e)
        {
            Intersections = Handler.GetIntersections();
            UpdateTableValues();
        }              


        private void loadBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Отчет о коллизиях (*.html)|*.html";
            dialog.Multiselect = false;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Intersections =  CollisionUtilities.HTMLReportParse(Handler.doc, dialog.FileName);
                UpdateTableValues();
            }
        }
    }
}
