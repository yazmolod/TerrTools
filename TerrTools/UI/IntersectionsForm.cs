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
        public List<IntersectionMepCurve> Intersections { get; private set; } = new List<IntersectionMepCurve>();
        double minPipeSizeValue { get; set; } = 150;
        public bool DoMerge { get => mergeCheckBox.Checked; }
        public double MergeTolerance { get => double.Parse(toleranceTextBox.Text.Replace('.',',')) / 304.8; }
        public IntersectionsForm(BaseIntersectionHandler handler)
        {
            Handler = handler;
            InitializeComponent();
            UpdateTableValues();
            ShowDialog();
        }

        private void UpdateTableValues()
        {
            dataGridView1.Rows.Clear();
            foreach (IntersectionMepCurve i in Intersections)
            {
                int nRow = dataGridView1.Rows.Add();
                FillRow(i, nRow, true);
            }
            countLabel.Text = "Пересечений: " + Intersections.Count.ToString();
        }

        private void FillRow(IntersectionMepCurve i, int nRow, bool firstFill = false)
        {
            dataGridView1.Rows[nRow].Cells["Level"].Value = i.Level.Name;
            dataGridView1.Rows[nRow].Cells["HostName"].Value = i.Host.Name;
            dataGridView1.Rows[nRow].Cells["HostId"].Value = i.Host.Id.IntegerValue;
            dataGridView1.Rows[nRow].Cells["PipeName"].Value = i.Pipe.Name;
            dataGridView1.Rows[nRow].Cells["PipeId"].Value = i.Pipe.Id.IntegerValue;
            dataGridView1.Rows[nRow].Cells["Offset"].Value = i.Offset * 304.8;
            dataGridView1.Rows[nRow].Cells["IsBrick"].Value = i.IsBrick;
            dataGridView1.Rows[nRow].Cells["HoleId"].Value = i.Id;
            dataGridView1.Rows[nRow].Cells["LevelOffset"].Value = i.LevelOffset * 304.8;
            dataGridView1.Rows[nRow].Cells["GroundOffset"].Value = i.GroundOffset * 304.8;
            dataGridView1.Rows[nRow].Cells["HoleSize"].Value = String.Concat(i.HoleWidth * 304.8, " x ", i.HoleHeight * 304.8, "h");

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
                        if (double.TryParse(value.ToString(), out offset)) Intersections[e.RowIndex].Offset = offset / 304.8;
                        else Intersections[e.RowIndex].Offset = 0;
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
            List<IntersectionMepCurve> filteredIntersections = new List<IntersectionMepCurve>();
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
                    Intersections[i].Offset = value / 304.8;
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
            var i = Handler.GetIntersections();
            Intersections.AddRange(i);
            UpdateTableValues();
        }              


        private void loadBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Отчет о коллизиях (*.html)|*.html";
            dialog.Multiselect = false;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var i = CollisionUtilities.HTMLReportParse(Handler.doc, dialog.FileName);
                Intersections.AddRange(i);
                UpdateTableValues();
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            toleranceTextBox.Enabled = (sender as CheckBox).Checked;
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            Intersections.Clear();
            UpdateTableValues();
        }
    }
}
