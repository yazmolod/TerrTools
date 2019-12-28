using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TerrTools
{
    public partial class FinishingForm : Form
    {
        public string AreaParameter {get; set;}
        public string WidthParameter { get; set; }
        public string DoorPlaneParameter { get; set; }
        public string FinishingHoleAreaParameter { get; set; }

        public FinishingForm()
        {
            InitializeComponent();
            ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AreaParameter = textBox1.Text;
            WidthParameter = textBox2.Text;
            DoorPlaneParameter = textBox3.Text;
            FinishingHoleAreaParameter = textBox4.Text;
            Close();
        }
    }
}
