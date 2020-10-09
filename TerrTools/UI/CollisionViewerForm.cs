using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WF = System.Windows.Forms;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

namespace TerrTools.UI
{
    public partial class CollisionViewerForm : WF.Form
    {
        CollisionReportTable Table { get; set; }
        public bool IsWorking { get; set; }
        ShowRowEvent EventHandler { get; set; }
        ExternalEvent ExEvent { get; set; }
        public CollisionViewerForm()
        {
            InitializeComponent();
            EventHandler = new ShowRowEvent();
            ExEvent = ExternalEvent.Create(EventHandler);
            this.Show();
        }

        private void showButton_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedCells.Count > 0)
            {
                var row = Table.Rows[dataGridView.SelectedCells[0].RowIndex];
                EventHandler.Row = row;
                EventHandler.Sender = sender as WF.Button;
                ExEvent.Raise();
            }
        }

        private void loadReport(string path)
        {
            Table = new CollisionReportTable(path);
            this.Text = "Просмотр коллизий: " + System.IO.Path.GetFileName(path);
            dataGridView.DataSource = Table.Rows;
        }

        private void loadButton_Click(object sender, EventArgs e)
        {
            WF.OpenFileDialog dialog = new WF.OpenFileDialog();
            dialog.Filter = "Отчет о коллизиях (*.html)|*.html";
            dialog.Multiselect = false;
            if (dialog.ShowDialog() == WF.DialogResult.OK)
            {
                string path = dialog.FileName;
                loadReport(path);
            }
        }
    }
}
