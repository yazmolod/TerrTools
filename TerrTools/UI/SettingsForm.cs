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
using Autodesk.Revit.Attributes;
using System.Reflection;
using System.Diagnostics;
using TerrTools.Updaters;

namespace TerrTools.UI
{
    public partial class SettingsForm : WF.Form
    {
        Document Document { get; set; }
        public SettingsForm(Document doc)
        {
            Document = doc;
            InitializeComponent();
            InitAboutPage();
            InitUpdatersPage();
            ShowDialog();
        }

        private void InitAboutPage()
        {
            versionLabel.Text = "Версия плагина: " + App.Version;
            richTextBox1.Text = "Обновления этой версии\n" + App.PatchNote;
        }

        private void InitUpdatersPage()
        {
            dataGridView1.DataSource = App.Updaters;
            dataGridView1.Columns["IsActive"].ReadOnly = false;
        }
            

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = WF.DialogResult.Cancel;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = WF.DialogResult.OK;
            this.Close();
        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            App.CheckUpdateDialog();
        }

        private void runCurrentButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGridView1.SelectedCells.Count; i++)
            {
                TerrUpdater updater = App.Updaters[dataGridView1.SelectedCells[i].RowIndex];
                updater.GlobalUpdate(Document);
            }
        }
    }
}
