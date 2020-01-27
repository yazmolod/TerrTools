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
using Autodesk.Revit.DB.Architecture;


namespace TerrTools
{
    public partial class HorizontalFinishingForm : WF.Form
    {
        public List<HorizontalFinishingResult> Result { get; set; }
        List<Room> Rooms;
        List<ElementType> FinishingTypes;
        string FinishingIdParameterName;
        BuiltInParameter FinishingRoomParameter;
        BuiltInParameter FinishingOffsetParameter;
        public HorizontalFinishingForm(List<Room> rooms, List<ElementType> finishingTypes, string finishingIdParameterName, BuiltInParameter finishingRoomParameter, BuiltInParameter finishingOffsetParameter)
        {
            InitializeComponent();
            FinishingTypes = finishingTypes;
            Rooms = rooms;
            FinishingIdParameterName = finishingIdParameterName;
            FinishingRoomParameter = finishingRoomParameter;
            FinishingOffsetParameter = finishingOffsetParameter;
            Result = new List<HorizontalFinishingResult>();
            InitDefaultValues();
            ShowDialog();
        }

        private void InitDefaultValues()
        {
            List<string> finishingTypeNames = (from type in FinishingTypes select type.Name).ToList();
            finishingTypeNames.Insert(0, "");
            for (int i = 0; i < Rooms.Count; i++)
            {
                this.dataGridView1.Rows.Add();
                this.dataGridView1.Rows[i].Cells["RoomNumber"].Value = Rooms[i].LookupParameter("Номер").AsString();
                this.dataGridView1.Rows[i].Cells["RoomName"].Value = Rooms[i].LookupParameter("Имя").AsString();
                this.dataGridView1.Rows[i].Cells["RoomId"].Value = Rooms[i].Id.IntegerValue;
                /// Типы отделок
                WF. DataGridViewComboBoxCell cb = this.dataGridView1.Rows[i].Cells["FinishingType"] as WF.DataGridViewComboBoxCell;
                cb.DataSource = finishingTypeNames;
                Parameter p = Rooms[i].get_Parameter(FinishingRoomParameter);
                if (p != null && p.HasValue && cb.Items.Contains(p.AsString()))
                {
                    cb.Value = p.AsString();
                }
                /// связанный элемент
                Parameter pFloorId = Rooms[i].LookupParameter(FinishingIdParameterName);
                Element linkedFloor = null;
                if (pFloorId != null && pFloorId.HasValue) {
                    this.dataGridView1.Rows[i].Cells["FinishingId"].Value = pFloorId.AsInteger();
                    linkedFloor = Rooms[i].Document.GetElement(new ElementId(pFloorId.AsInteger()));
                        }
                /// смещение пола
                this.dataGridView1.Rows[i].Cells["FinishingOffset"].Value = 
                    linkedFloor != null 
                    ? 
                    linkedFloor.get_Parameter(FinishingOffsetParameter).AsDouble()*304.8
                    : 
                    0.0;
            }
        }

        private void SetResult()
        {
            foreach (WF.DataGridViewRow row in this.dataGridView1.Rows)
            {
                Room r = this.Rooms.First(q => q.Id.IntegerValue == (int)row.Cells["RoomId"].Value);
                ElementType elType;
                var tmp = row.Cells["FinishingType"].Value;
                if (tmp == null || (string)tmp == "" || !this.FinishingTypes.Any(q => q.Name == tmp.ToString())) elType = null;                
                else elType = this.FinishingTypes.First(q => q.Name == tmp.ToString());                
                double off = 0f;
                Double.TryParse(row.Cells["FinishingOffset"].Value.ToString(), out off);
                Result.Add(new HorizontalFinishingResult(r, elType, off));
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.SetResult();
            this.DialogResult = WF.DialogResult.OK;
            this.Close();
        }

        private void FloorFinishingForm_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            foreach (WF.DataGridViewRow row in this.dataGridView1.Rows)
            {
                row.Cells["FinishingType"].Value = null;
            }
        }
    }
    public class HorizontalFinishingResult
    {
        public Room Room { get; set; }
        public CurveArray MainProfile { get; set; }
        public List<CurveArray> OpeningProfiles { get; set; }
        public ElementType FinishingType { get; set; }
        public Level Level { get; set; }
        public double Offset { get; set; }
        public Element FinishingElement { get; set; }

        public HorizontalFinishingResult(Room room, ElementType elementType, double offset)
        {
            Room = room;
            Level = room.Level;
            FinishingType = elementType;
            Offset = offset;
            
            ///
            /// Находим все профили помещения и сортируем по периметру. Самый длинный - контур перекрытия
            ///
            List<List<Curve>> tmp = CustomGeometryUtility.GetCurvesListFromRoom(room);
            tmp = tmp.OrderBy(x => x.Sum(y => y.Length)).ToList();
            MainProfile = ConvertListToCurveArray(tmp.Last());
            ///
            /// Остальное - вырезы
            ///
            OpeningProfiles = new List<CurveArray>();
            for (int i = 0; i < tmp.Count-1; i++)
            {
                OpeningProfiles.Add(ConvertListToCurveArray(tmp[i]));
            }
        }

        private CurveArray ConvertListToCurveArray(List<Curve> curves)
        {
            CurveArray arr = new CurveArray();
            foreach (Curve c in curves)
            {
                arr.Append(c);
            }
            return arr;
        }
    }
}
