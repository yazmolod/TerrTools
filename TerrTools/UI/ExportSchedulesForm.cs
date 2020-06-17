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
        public ExportSchedulesForm(List<Element> schedules)
        {
            InitializeComponent();

            foreach (var item in schedules)
            {
                leftListBox.Items.Add(item);
            }

            leftListBox.DisplayMember = "Name";
            rightListBox.DisplayMember = "Name";

            ShowDialog();
        }

        private ScheduleExportOptions MakeExportOptions()
        {
            SplitFileOptions splitFile;
            SplitDataOptions splitData;
            if (multipleFilesRB.Checked) splitFile = SplitFileOptions.MultipleFiles;
            else if (oneFileMultipleSheetRB.Checked) splitFile = SplitFileOptions.OneFileMultiSheet;
            else splitFile = SplitFileOptions.OneFileOneSheet;

            if (splitMultiSheetRB.Checked) splitData = SplitDataOptions.MultipleSheet;
            else if (splitOneSheetRB.Checked) splitData = SplitDataOptions.OneSheet;
            else splitData = SplitDataOptions.NoSplit;

            bool merge = mergeCheckBox.Checked;
            bool headers = headerCheckBox.Checked;

            var opt = new ScheduleExportOptions(splitFile, splitData, merge, headers);
            return opt;
        }

        private void exportBtn_Click(object sender, EventArgs e)
        {
            Result = rightListBox.Items.Cast<Element>();
            this.ExportOptions = MakeExportOptions();
            this.DialogResult = WF.DialogResult.OK;
            this.Close();
        }

        private void inBtn_Click(object sender, EventArgs e)
        {
            var indexes = leftListBox.SelectedIndices;
            for (int i = indexes.Count - 1; i >= 0; i--)
            {
                rightListBox.Items.Insert(0, leftListBox.Items[indexes[i]]);
                leftListBox.Items.RemoveAt(indexes[i]);
            }
        }

        private void outBtn_Click(object sender, EventArgs e)
        {
            var indexes = rightListBox.SelectedIndices;
            for (int i = indexes.Count - 1; i >= 0; i--)
            {
                leftListBox.Items.Insert(0, rightListBox.Items[indexes[i]]);
                rightListBox.Items.RemoveAt(indexes[i]);
            }
        }

        private void updownBtn_Click(object sender, EventArgs e)
        {
            var btn = (WF.Button)sender;
            int index = rightListBox.SelectedIndex;
            if (index >= 0)
            {
                int new_index = btn.Name == "upBtn" ? index - 1 : index + 1;
                if (-1 < new_index && new_index < rightListBox.Items.Count)
                {
                    var item = rightListBox.SelectedItem;
                    rightListBox.Items.RemoveAt(index);
                    rightListBox.Items.Insert(new_index, item);
                    rightListBox.SelectedIndex = new_index;
                }
            }
        }

        private void oneFileOneSheetRB_CheckedChanged(object sender, EventArgs e)
        {
            var radio = (WF.RadioButton)sender;
            splitMultiSheetRB.Enabled = headerCheckBox.Enabled = !radio.Checked;
            mergeCheckBox.Enabled = mergeCheckBox.Checked = radio.Checked;
            if (radio.Checked)
            {
                NoSplitRB.Checked = true;
                headerCheckBox.Checked = false;                
            }
            
        }
    }
}
    