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
    public partial class InsulCurvesForm : Form
    {
        public int ResultStep { get; set; }
        public int ResultHeight { get; set; }
        public Autodesk.Revit.DB.GraphicsStyle ResultStyle { get; set; }
        public ScopeType ResultScope { get; set; }
        public LineType ResultLine { get; set; }
        public InsulCurvesForm(List<Autodesk.Revit.DB.GraphicsStyle> styles)
        {
            InitializeComponent();
            // default init
            ResultStep = 300;
            ResultHeight = 300;
            ResultScope = ScopeType.Document;
            ResultLine = LineType.ModelLine;
            ResultStyle = styles[0];
            graphicComboBox.DataSource = styles;
            graphicComboBox.DisplayMember = "Name";
            ShowDialog();
        }

        private void stepTextBox_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(this.stepTextBox.Text, out int x))
            {
                ResultStep = x;
            }
            
        }

        private void createButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }


        public enum ScopeType
        {
            Document,
            View,
            Selection
        }

        public enum LineType
        {
            ModelLine,
            DetailLine
        }

        private void DocumentRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            var radio = sender as RadioButton;
            if (radio.Checked)
            {
                switch (radio.Text)
                {
                    case "Во всем документе":
                        ResultScope = ScopeType.Document;
                        DetailRadioButton.Enabled = false;
                        DetailRadioButton.Checked = false;
                        ModelCurveRadioButton.Checked = true;
                        break;

                    case "На текущем виде":
                        ResultScope = ScopeType.View;
                        DetailRadioButton.Enabled = true;
                        break;

                    case "Выбрать вручную":
                        ResultScope = ScopeType.Selection;
                        DetailRadioButton.Enabled = true;
                        break;

                    case "Линии модели (3D)":
                        ResultLine = LineType.ModelLine;
                        break;
                    case "Линии детализации (2D)":
                        ResultLine = LineType.DetailLine;
                        break;
                }
            }
        }

        private void heightTextBox_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(this.heightTextBox.Text, out int x))
            {
                ResultHeight = x;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            ResultStyle = (Autodesk.Revit.DB.GraphicsStyle)cb.SelectedItem;
        }
    } 
}
