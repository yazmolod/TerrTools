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
    public partial class OneComboboxForm : Form
    {
        public string SelectedItem { get; set; }
        public OneComboboxForm(string title, string[] data)
        {
            InitializeComponent();
            this.Text = title;
            comboBox1.Items.AddRange(data);
            ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SelectedItem = comboBox1.Text;
            DialogResult = DialogResult.OK;
        }
    }
}
