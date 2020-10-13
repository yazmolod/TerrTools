using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
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
        public bool ReplaceUsedViews { get; set; } = false;
        public ElementId ViewTypeId { get; set; }
        public ElementId ViewTemplateId { get; set; }

        public IzometryGeneratorForm(List<string> systemsNames,
                                     List<string> viewNames,
                                     List<ViewFamilyType> viewTypes,
                                     List<View3D> viewTemplates)
        {
            InitializeComponent();
            InitListBox(systemsNames, viewNames);

            viewTypeComboBox.DataSource = viewTypes;
            viewTypeComboBox.DisplayMember = "Name";
            ViewTypeId = viewTypes[0].Id;

            templateViewComboBox.DataSource = viewTemplates;
            templateViewComboBox.DisplayMember = "Name";
            ViewTemplateId = ElementId.InvalidElementId;
        }

        private void InitListBox(List<string> systemsNames, List<string> viewNames)
        {
            // Те системы, которые уже есть(их соответственно
            // не помечаем).
            List<string> existingSystems = new List<string>();
            List<string> systemsNamesCopy = new List<string>();
            foreach (var item in systemsNames)
            {
                systemsNamesCopy.Add(item);
            }
            foreach (var name in systemsNames)
            {
                if (name != null)
                {
                    foreach (var item in viewNames)
                    {
                        {
                            if (Regex.Match(item, "^" + name + @"(_\d+)?$").Success)
                            {
                                systemsNamesCopy.Remove(name);
                                if (existingSystems.Contains(name))
                                {
                                    continue;
                                }
                                else
                                {
                                    existingSystems.Add(name);
                                }
                                break;
                            }
                        }
                    }
                }
            }
            // systemsNamesCopy - новые системы, для
            // которых нужно создать 3D-вид.
            foreach (var item in systemsNamesCopy)
            {
                if (item != null)
                {
                    checkedListBox1.Items.Add(item, true);
                }
            }
            foreach (var item in existingSystems)
            {
                if (item != null)
                {
                    checkedListBox1.Items.Add(item);
                }
            }
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
            Result.AddRange(checkedListBox1.CheckedItems.Cast<string>());
            ReplaceUsedViews = checkBox1.Checked;
            this.DialogResult = WF.DialogResult.OK;
            this.Close();           
        }

        private void templateCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            WF.CheckBox cb = sender as WF.CheckBox;
            if (cb.Checked)
            {
                ViewTemplateId = (templateViewComboBox.SelectedItem as Element).Id;
                templateViewComboBox.Enabled = true;
            }
            else
            {
                ViewTemplateId = ElementId.InvalidElementId;
                templateViewComboBox.Enabled = false;
            }
        }

        private void viewTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            WF.ComboBox cb = sender as WF.ComboBox;
            ViewTypeId = (cb.SelectedItem as Element).Id;
        }

        private void templateViewComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            WF.ComboBox cb = sender as WF.ComboBox;
            ViewTemplateId = (cb.SelectedItem as Element).Id;
        }
    }
}
