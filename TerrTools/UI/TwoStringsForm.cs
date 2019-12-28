using System;
using System.Windows.Forms;
using DB = Autodesk.Revit.DB;

namespace TerrTools
{
    public partial class TwoStringsForm : Form
    {
        public DB.BuiltInCategory Category { get; set; }
        public string ParameterName { get; set; }

        public TwoStringsForm()
        {
            InitializeComponent();
            ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ParameterName = textBox1.Text;
            switch (comboBox1.Text)
            {
                case "Воздухораспределители":
                    Category = DB.BuiltInCategory.OST_DuctTerminal;
                    break;
                case "Оборудование":
                    Category = DB.BuiltInCategory.OST_MechanicalEquipment;
                    break;
                default:
                    Category = DB.BuiltInCategory.INVALID;
                    break;
            }
            Close();
        }
    }
}
