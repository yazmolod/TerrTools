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
using System.Windows.Forms;
using Autodesk.Revit.UI.Selection;

namespace TerrTools.UI
{
    public partial class MarkingForm : WF.Form
    {
        public MarkingForm()
        {
            InitializeComponent();
            this.Show();
            this.TopLevel = true;
            this.TopMost = true;
        }

        // По центру
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            Marking.MarkOffset = radioButton1.Name;
        }

        private void MarkingForm_Load(object sender, EventArgs e)
        {

        }
        // Верх
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            Marking.MarkOffset = radioButton2.Name;
        }
        // Низ
        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            Marking.MarkOffset = radioButton3.Name;
        }
        // Лево
        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            Marking.MarkOffset = radioButton4.Name;
        }
        // Право
        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            Marking.MarkOffset = radioButton5.Name;
        }
        // верх-лево
        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            Marking.MarkOffset = radioButton6.Name;
        }
        // верх-право
        private void radioButton7_CheckedChanged(object sender, EventArgs e)
        {
            Marking.MarkOffset = radioButton7.Name;
        }
        // низ-лево
        private void radioButton8_CheckedChanged(object sender, EventArgs e)
        {
            Marking.MarkOffset = radioButton8.Name;
        }
        // низ-право
        private void radioButton9_CheckedChanged(object sender, EventArgs e)
        {
            Marking.MarkOffset = radioButton9.Name;
        }
    }
}
