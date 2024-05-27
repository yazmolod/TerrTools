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
    public partial class SelectFromList : Form
    {
        public string result { get; set; }
        public SelectFromList(string title, string[] inputs)
        {
            InitializeComponent();
            this.Text = title;
            this.listBox1.Items.AddRange(inputs);
            this.listBox1.SelectedIndex = 0;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.result = (string)this.listBox1.SelectedItem;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
