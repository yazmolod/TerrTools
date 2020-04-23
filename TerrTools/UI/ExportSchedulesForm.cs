﻿using System;
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
            opt.SplitSheets = MultipleSheetRadioButton.Checked;
            return opt;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Result = listBox1.SelectedItems.Cast<Element>();
            this.ExportOptions = MakeExportOptions();
            this.DialogResult = WF.DialogResult.OK;
            this.Close();
        }
    }    
}