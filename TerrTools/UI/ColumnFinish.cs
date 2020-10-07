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

namespace TerrTools.UI
{
    public partial class ColumnFinishForm : WF.Form
    {
        public Element Result { get; set; }
        public ColumnFinishForm(IList<Element> a)
        {
            InitializeComponent();
            comboBox1.DataSource = a;
            comboBox1.DisplayMember = "Name";
        }

        private void ColumnFinishForm_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Result = comboBox1.SelectedItem as Element;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void ColumnFinishForm_Load_1(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            Result = comboBox1.SelectedItem as Element;
            this.Close();
        }
    }
}
