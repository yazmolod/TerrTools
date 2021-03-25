using System;
using System.Windows.Forms;
using Revit = Autodesk.Revit.DB;

namespace TerrTools.UI
{
    public partial class PdfPrintingForm : Form
    {
        public enum ExportViewOption { Current, Selection, Set }
        public ExportViewOption SelectedOption { get; set; }
        public string Printer { get; set; }
        public Revit.ViewSheetSet Set { get; set; } 
        public PdfPrintingForm(string[] printers, Revit.ViewSheetSet[] sets)
        {
            InitializeComponent();
            printerComboBox.DataSource = printers;
            setComboBox.DataSource = sets;
            setComboBox.DisplayMember = "Name";
        }

        private void printButton_Click(object sender, EventArgs e)
        {
            if (currentRadioButton.Checked) SelectedOption = ExportViewOption.Current;
            else if (selectionRadioButton.Checked) SelectedOption = ExportViewOption.Selection;
            else if (setRadioButton.Checked) SelectedOption = ExportViewOption.Set;

            Set = setComboBox.SelectedItem as Revit.ViewSheetSet;
            Printer = printerComboBox.SelectedItem as string;
            DialogResult = DialogResult.OK;
        }

        private void printerComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
