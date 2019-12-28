using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Revit = Autodesk.Revit.DB;

namespace TerrTools
{
    public partial class FloorRoomsForm : Form
    {
        public Revit.ElementId ScheduleId {get; set;}
        List<Revit.Element> schedules { get; set; }

        public FloorRoomsForm(Revit.Document doc)
        {
            InitializeComponent();
            string defaultSchedule = "В_Полы-помещения-01_Стили полов_Ключевая";
            schedules = new Revit.FilteredElementCollector(doc).OfClass(typeof(Revit.ViewSchedule)).ToList();
            foreach (Revit.Element schedule in schedules)
            {
                comboBox1.Items.Add(schedule.Name);
            }
            if (comboBox1.Items.Contains(defaultSchedule)) comboBox1.Text = defaultSchedule;
            ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Revit.Element currentSchedule = (from s in schedules
                                                 where s.Name == comboBox1.Text
                                                 select s).First();
                ScheduleId = currentSchedule.Id;
            }
            catch {
                ScheduleId = Revit.ElementId.InvalidElementId;
            }
            Close();
        }
    }
}
