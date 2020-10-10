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
    public partial class IzometryUserChoiceForm : WF.Form
    {
        public string Result { get; set; }
        public IzometryUserChoiceForm(string systemName)
        {
            InitializeComponent();
            label1.Text = $"Внимание! Для системы '{systemName}' уже есть 3D-вид.";
            label2.Text = "Для продолжения выберите один из трёх вариантов.";
            
        }

        private void IzometryUserChoiceForm_Load(object sender, EventArgs e)
        {

        }
        // Кнопка "С заменой".
        private void button1_Click(object sender, EventArgs e)
        {
            Result = "С заменой";
            this.DialogResult = WF.DialogResult.OK;
            this.Close();
        }
        // Кнопка "Без замены".
        private void button2_Click(object sender, EventArgs e)
        {
            Result = "Без замены";
            this.DialogResult = WF.DialogResult.OK;
            this.Close();
        }
        // Кнопка "Пропустить".
        private void button3_Click(object sender, EventArgs e)
        {
            Result = "Пропустить";
            this.DialogResult = WF.DialogResult.OK;
            this.Close();
        }
    }
}
