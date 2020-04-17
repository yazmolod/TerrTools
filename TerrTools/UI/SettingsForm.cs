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

namespace TerrTools.UI
{
    public partial class SettingsForm : WF.Form
    {
        public SettingsForm()
        {
            InitializeComponent();
            InitAboutPage();
            InitUpdatersPage();
            ShowDialog();
        }

        private void InitAboutPage()
        {
            string loc = Assembly.GetExecutingAssembly().Location;
            string version = FileVersionInfo.GetVersionInfo(loc).FileVersion;
            string patchNote = FileVersionInfo.GetVersionInfo(loc).Comments;

            versionLabel.Text = "Версия плагина: " + version;
            patchnoteLabel.Text = "Обновления этой версии\n" + patchNote;
        }

        private void InitUpdatersPage()
        {
            List<BindedUpdater> updaters = new List<BindedUpdater>();
            foreach (var u in TerrToolsApp.Updaters)
            {
                updaters.Add(new BindedUpdater(u as Updaters.TerrUpdater));
            }
            ((WF.ListBox)updatersListBox).DataSource = updaters;
            ((WF.ListBox)updatersListBox).DisplayMember = "ShowName";
            ((WF.ListBox)updatersListBox).ValueMember = "ShowValue";
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
    }

    public class BindedUpdater
    {
        public BindedUpdater(Updaters.TerrUpdater upd) { Updater = upd; }
        public Updaters.TerrUpdater Updater { get; set; }
        public string ShowName { get { return Updater.GetUpdaterName(); } }
        public bool ShowValue { get { return Updater.IsActive; } }
    }
}
