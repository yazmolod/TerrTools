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
        // Result - те имена систем, для которых пользователь хочет создать 3D-вид.
        public List<string> Result { get; set; } = new List<string>();
        public bool changeUsedViews { get; set; } = false;
        public IzometryGeneratorForm(List<string> systemsNames, List<string> usedSystems)
        {
            InitializeComponent();
            // Те системы, для которых поставится чекер по умолчанию.
            // Для этих систем еще не создан 3D-вид.
            List<string> newSystems = systemsNames.Except(usedSystems).ToList();
            foreach (var item in newSystems)
            {
                if (item!=null)
                {
                    checkedListBox1.Items.Add(item, true);
                }
            }
            // Те системы, которые уже есть 3D-вид.
            // По умолчанию, для них отключен чекер.
            foreach (var item in usedSystems)
            {
                if (item != null)
                {
                    checkedListBox1.Items.Add(item);
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
        // Кнопка "Создать 3D-виды".
        private void button1_Click(object sender, EventArgs e)
        {
            foreach (var item in checkedListBox1.CheckedItems)
            {
                Result.Add((string)item);
            }
            changeUsedViews = checkBox1.Checked;
            this.DialogResult = WF.DialogResult.OK;
            this.Close();
           
        }
    }
}
