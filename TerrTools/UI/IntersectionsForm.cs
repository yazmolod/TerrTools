using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TerrTools.UI
{
    public partial class IntersectionsForm : Form
    {
        List<Intersection> Intersections { get; set; }
        bool DeleteOld { get; set; }
        double minPipeSizeValue {get;set;}
        public List<Intersection> UpdatedIntersections = new List<Intersection>();
        public IntersectionsForm(List<Intersection> intersections)
        {
            InitializeComponent();
            Intersections = intersections;
            minPipeSizeValue = 150;
            InitValues();
            ShowDialog();
        }

        private void InitValues()
        {
            foreach (Intersection i in Intersections)
            {
                int nRow = dataGridView1.Rows.Add();
                FillRow(i, nRow, true);
            }
        }

        private void FillRow(Intersection i, int nRow, bool firstFill = false)
        {
            dataGridView1.Rows[nRow].Cells["IntersectionPoint"].Value = String.Format(
                "X: {0}, Y: {1}, Смещение снизу: {2}",
                Math.Round(i.CenterPoint.X * 304.8, 1),
                Math.Round(i.CenterPoint.Y * 304.8, 1),
                Math.Round((i.CenterPoint.Z - i.Level.Elevation) * 304.8, 1)
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
            if (e.RowIndex != -1 && e.ColumnIndex != -1)
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
                    case "IsBrick":
                        bool isBrick;
                        bool.TryParse(value.ToString(), out isBrick);
                        Intersections[e.RowIndex].IsBrick = isBrick;
                        break;
                    default:
                        break;
                }
                FillRow(Intersections[e.RowIndex], e.RowIndex);
            }
        }

        private void SetResult()
        {
            for (int i = 0; i < Intersections.Count; i++)
            {
                if (Convert.ToBoolean(dataGridView1.Rows[i].Cells["AddToProject"].Value) == true)
                {
                    UpdatedIntersections.Add(Intersections[i]);
                }
            }
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
    }
}
