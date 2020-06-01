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

namespace TerrTools.UI
{
    public partial class ExportSchedulesForm : WF.Form
    {
        public IEnumerable<Element> Result { get; set; }
        public ScheduleExportOptions ExportOptions { get; set; }
        public ExportSchedulesForm(Element[] schedules)
        {
            InitializeComponent();            
            listBox1.DataSource = schedules;
            listBox1.DisplayMember = "Name";
            
            ShowDialog();
        }

        private ScheduleExportOptions MakeExportOptions()
        {
            var opt = new ScheduleExportOptions();
            //opt.SplitSheets = oneFileMultipleSheetRB.Checked;
            return opt;
        }

        private void exportBtn_Click(object sender, EventArgs e)
        {
            Result = listBox2.Items.Cast<Element>();
            this.ExportOptions = MakeExportOptions();
            this.DialogResult = WF.DialogResult.OK;
            this.Close();
        }

        private void inBtn_Click(object sender, EventArgs e)
        {
            int index = listBox1.SelectedIndex;
            listBox2.Items.Add(listBox1.Items[index]);
            listBox1.Items.RemoveAt(index);
        }

        private void outBtn_Click(object sender, EventArgs e)
        {
            int index = listBox2.SelectedIndex;
            listBox1.Items.Add(listBox2.Items[index]);
            listBox2.Items.RemoveAt(index);
        }

        private void upBtn_Click(object sender, EventArgs e)
        {
            int index = listBox2.SelectedIndex;
            var item = listBox2.SelectedItem;
            listBox2.Items.RemoveAt(index);
            listBox2.Items.Insert(index - 1, item);
        }

        private void downBtn_Click(object sender, EventArgs e)
        {
            int index = listBox2.SelectedIndex;
            var item = listBox2.SelectedItem;
            listBox2.Items.RemoveAt(index);
            listBox2.Items.Insert(index + 1, item);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            bool status = (sender as WF.CheckBox).Checked;
            splitMultiSheetRB.Enabled = status;
            splitOneSheetRB.Enabled = status;
        }

        private void oneFileOneSheetRB_CheckedChanged(object sender, EventArgs e)
        {
            bool status = (sender as WF.RadioButton).Checked;
            checkBox1.Enabled = status;
        }
    }    
}
