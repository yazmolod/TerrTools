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
using Autodesk.Revit.UI.Selection;

namespace TerrTools.UI
{
    public partial class IzometryGeneratorForm : WF.Form
    {
        public IzometryGeneratorForm(List<string> systemsNames)
        {
            InitializeComponent();
            foreach (var item in systemsNames)
            {
                if (item!=null)
                {
                    checkedListBox1.Items.Add(item, true);
                }
            }
        }

        private void IzometryGeneratorForm_Load(object sender, EventArgs e)
        {

        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        // Кнопка "Отметить все".
        private void button2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, true);
            }
        }

        // Кнопка "Снять выделения".
        private void button3_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, false);
            }
        }
    }
}
