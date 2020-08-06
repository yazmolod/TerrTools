using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WF = System.Windows.Forms;
using Autodesk.Revit.DB;

namespace TerrTools.UI
{
    public partial class TwoDoc : WF.Form
    {
        public Document Document1 { get; private set; }
        public Document Document2 { get; private set; }
        public int HostElementNumber { get; private set; } = 0;
        public int LinkElementNumber { get; private set; } = 0;
        public RevitLinkInstance LinkInstance1 { get; private set; }
        public RevitLinkInstance LinkInstance2 { get; private set; }
        public Document CurrentDoc { get; }
        CollisionReportTable Table { get; }

        public TwoDoc(Document doc, CollisionReportTable table)
        {
            InitializeComponent();
            CurrentDoc = doc;
            Table = table;
            var linkedDocs = new FilteredElementCollector(doc).OfClass(typeof(RevitLinkInstance)).Cast<RevitLinkInstance>().Select(x => new TwoDocComboBoxItem(x)).ToArray();
            
            LinkComboBox1.Items.AddRange(linkedDocs);
            LinkComboBox2.Items.AddRange(linkedDocs);
            LinkComboBox1.SelectedIndex = LinkComboBox2.SelectedIndex = 0;

            elementListBox1.Items.AddRange(table.Rows.Select(x => x.E1_Path).ToArray());
            elementListBox2.Items.AddRange(table.Rows.Select(x => x.E2_Path).ToArray());
            Suggest();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = WF.DialogResult.OK;
            if (currentDocRadioButton1.Checked)
            {
                Document1 = CurrentDoc;
                HostElementNumber += 1;
            }
            else if (linkDocRadioButton1.Checked)
            {
                var item = LinkComboBox1.SelectedItem as TwoDocComboBoxItem;
                Document1 = item.Document;
                LinkInstance1 = item.RevitLinkInstance;
                LinkElementNumber += 1;
            }
            else Document1 = null;

            if (currentDocRadioButton2.Checked)
            {
                Document2 = CurrentDoc;
                HostElementNumber += 2;
            }
            else if (linkDocRadioButton2.Checked)
            {
                var item = LinkComboBox2.SelectedItem as TwoDocComboBoxItem;
                Document2 = item.Document;
                LinkInstance2 = item.RevitLinkInstance;
                LinkElementNumber += 2;
            }
            else Document2 = null;
        }

        private void Suggest()
        {
            string currentDocTitle = Regex.Match(CurrentDoc.Title, @"(.+)_").Groups[1].Value;
            var docname1 = Table.Rows[0].E1_DocumentName;
            var docname2 = Table.Rows[0].E2_DocumentName;

            if (docname1 == currentDocTitle)
            {  
                currentDocRadioButton1.Checked = true;
                linkDocRadioButton1.Checked = false;
            }
            else if (LinkComboBox1.Items.Contains(docname1))
            {
                currentDocRadioButton1.Checked = false;
                linkDocRadioButton1.Checked = true;
            }

            if (docname2 == currentDocTitle)
            {
                currentDocRadioButton2.Checked = true;
                linkDocRadioButton2.Checked = false;
            }
            else if (LinkComboBox2.Items.Contains(docname1))
            {
                currentDocRadioButton2.Checked = false;
                linkDocRadioButton2.Checked = true;
            }
        }

        private void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            WF.RadioButton rb = sender as WF.RadioButton;
            switch (rb.Name)
            {
                case "linkDocRadioButton1":
                    LinkComboBox1.Enabled = rb.Checked;
                    break;
                case "linkDocRadioButton2":
                    LinkComboBox2.Enabled = rb.Checked;
                    break;
            }
        }
    }

    public class TwoDocComboBoxItem
    {
        public RevitLinkInstance RevitLinkInstance { get; }
        public Document Document { get; }
        public override string ToString()
        {
            return RevitLinkInstance.Name;
        }

        public TwoDocComboBoxItem(RevitLinkInstance instance)
        {
            RevitLinkInstance = instance;
            Document = instance.GetLinkDocument();
        }
    } 
}
