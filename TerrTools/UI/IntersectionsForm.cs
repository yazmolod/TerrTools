using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HAP = HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Reflection;

namespace TerrTools.UI
{
    public partial class IntersectionsForm : Form
    {
        private BaseIntersectionHandler Handler;
        public List<Intersection> Intersections { get; private set; } = new List<Intersection>();
        double minPipeSizeValue {get;set;}
        public IntersectionsForm(BaseIntersectionHandler handler)
        {
            Handler = handler;
            InitializeComponent();
            minPipeSizeValue = 150;
            UpdateTableValues();
            ShowDialog();
            // 
            Assembly.LoadFile(@"L:\REVIT\Плагины\TerrTools\dll\HtmlAgilityPack.dll");
        }

        private void UpdateTableValues()
        {
            dataGridView1.Rows.Clear();
            foreach (Intersection i in Intersections)
            {
                int nRow = dataGridView1.Rows.Add();
                FillRow(i, nRow, true);
            }
        }

        private void FillRow(Intersection i, int nRow, bool firstFill = false)
        {
            dataGridView1.Rows[nRow].Cells["IntersectionPoint"].Value = String.Format(
                "X: {0}, Y: {1}, Смещение снизу: {2}",
                Math.Round(i.CenterPoint.X * 304.8, 1),
                Math.Round(i.CenterPoint.Y * 304.8, 1),
                Math.Round((i.CenterPoint.Z - i.Level.Elevation) * 304.8, 1)
                );
            dataGridView1.Rows[nRow].Cells["Level"].Value = i.Level.Name;
            dataGridView1.Rows[nRow].Cells["HostName"].Value = i.Host.Name;
            dataGridView1.Rows[nRow].Cells["HostId"].Value = i.Host.Id.IntegerValue;
            dataGridView1.Rows[nRow].Cells["PipeName"].Value = i.Pipe.Name;
            dataGridView1.Rows[nRow].Cells["PipeId"].Value = i.Pipe.Id.IntegerValue;
            dataGridView1.Rows[nRow].Cells["Offset"].Value = i.MinOffset * 304.8;
            dataGridView1.Rows[nRow].Cells["IsBrick"].Value = i.IsBrick;
            dataGridView1.Rows[nRow].Cells["HoleId"].Value = i.Id;
            dataGridView1.Rows[nRow].Cells["LevelOffset"].Value = i.LevelOffset * 304.8;
            dataGridView1.Rows[nRow].Cells["GroundOffset"].Value = i.GroundOffset * 304.8;
            dataGridView1.Rows[nRow].Cells["HoleSize"].Value = String.Concat(i.HoleWidth * 304.8, " x ", i.HoleHeight * 304.8, "h");

            if (i.IsRound) dataGridView1.Rows[nRow].Cells["PipeSize"].Value = String.Concat("Ø", i.PipeWidth * 304.8);
            else dataGridView1.Rows[nRow].Cells["PipeSize"].Value = String.Concat(i.PipeWidth * 304.8, " x ", i.PipeHeight * 304.8, "h");

            if (firstFill) dataGridView1.Rows[nRow].Cells["AddToProject"].Value = i.PipeWidth * 304.8 >= minPipeSizeValue;
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            var grid = sender as DataGridView;
            if (e.RowIndex != -1 && e.ColumnIndex != -1 && e.RowIndex < Intersections.Count())
            {
                string columnName = grid.Columns[e.ColumnIndex].Name;
                var value = grid.Rows[e.RowIndex].Cells[columnName].Value;
                switch (columnName)
                {
                    case "Offset":
                        double offset;
                        if (double.TryParse(value.ToString(), out offset)) Intersections[e.RowIndex].MinOffset = offset / 304.8;
                        else Intersections[e.RowIndex].MinOffset = 0;
                        break;
                    //// превратилось в read only
                    //case "IsBrick":
                    //    bool isBrick;
                    //    bool.TryParse(value.ToString(), out isBrick);
                    //    Intersections[e.RowIndex].IsBrick = isBrick;
                    //    break;
                    default:
                        break;
                }
                FillRow(Intersections[e.RowIndex], e.RowIndex);
            }
        }

        private void SetResult()
        {
            List<Intersection> filteredIntersections = new List<Intersection>();
            for (int i = 0; i < Intersections.Count; i++)
            {
                if (Convert.ToBoolean(dataGridView1.Rows[i].Cells["AddToProject"].Value) == true)
                {
                    filteredIntersections.Add(Intersections[i]);
                }
            }
            Intersections = filteredIntersections;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.SetResult();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void offsetTextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox offsetTB = sender as TextBox;
            double value;
            if (double.TryParse(offsetTB.Text, out value))
            {
                for (int i = 0; i < Intersections.Count; i++)
                {
                    Intersections[i].MinOffset = value / 304.8;
                    FillRow(Intersections[i], i);
                }
            }            
        }

        private void minSizeTextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox offsetTB = sender as TextBox;
            double value;
            if (double.TryParse(offsetTB.Text, out value))
            {
                minPipeSizeValue = value;
                for (int i = 0; i < Intersections.Count; i++)
                {
                    FillRow(Intersections[i], i, true);
                }
            }
        }

        private void analyzeBtn_Click(object sender, EventArgs e)
        {
            Intersections = Handler.GetIntersections();
            UpdateTableValues();
        }        

        private List<Intersection> HAP_HTMLReportParse(string path)
        {
            List<Intersection> result = new List<Intersection>();
            Autodesk.Revit.DB.Document hostdoc = Handler.doc;
            Autodesk.Revit.DB.RevitLinkInstance pipedoclink = GeometryUtils.ChooseLinkedDoc(hostdoc);
            if (pipedoclink == null)
            {
                return result;
            }
            else
            {
                Autodesk.Revit.DB.Document pipedoc = pipedoclink.GetLinkDocument();
                Autodesk.Revit.DB.Transform tr = GeometryUtils.GetCorrectionTransform(pipedoclink);
                var doc = new HAP.HtmlDocument();
                doc.Load(path, Encoding.UTF8);
                foreach (var node in doc.DocumentNode.SelectNodes("//div[@class='viewpoint']"))
                {
                    // определение файла
                    List<string> path_nodes = new List<string>();
                    List<string> id_nodes = new List<string>();
                    string str_point = "";
                    foreach (var span in node.SelectNodes("./span[@class='namevaluepair']"))
                    {
                        var sp1 = span.SelectSingleNode("./span[1]/text()");
                        var sp2 = span.SelectSingleNode("./span[2]/text()");
                        if (sp1.InnerText == "Путь")
                        {
                            path_nodes.Add(sp2.InnerText);
                        }
                        else if (sp1.InnerText == "ID объекта")
                        {
                            id_nodes.Add(sp2.InnerText);
                        }
                        else if (sp1.InnerText == "Точка конфликта")
                        {
                            str_point = sp2.InnerText;
                        }
                    }
                    Regex re = new Regex(@"[а-яА-Яa-zA-Z\d\s]+");
                    // выцепляем имя файла из формата
                    // Файл ->Файл ->тестАР.nwd ->Уровень 1 ->Стены ->Базовая стена ->Типовой - Кирпич 250 ->Базовая стена
                    string element1DocName = path_nodes[0].Split(new string[] { "-&gt;" }, StringSplitOptions.None)[2];
                    string element2DocName = path_nodes[1].Split(new string[] { "-&gt;" }, StringSplitOptions.None)[2];
                    string hostDocName = hostdoc.Title;
                    string pipeDocName = pipedoc.Title;
                    int hostOrder = -1;
                    int pipeOrder = -1;
                    // определяем, с каким случаем работаем
                    int x1 = re.Match(hostDocName).Value == re.Match(element1DocName).Value ? 1 : 0;
                    int x2 = re.Match(hostDocName).Value == re.Match(element2DocName).Value ? 2 : 0;
                    int x3 = re.Match(pipeDocName).Value == re.Match(element1DocName).Value ? 4 : 0;
                    int x4 = re.Match(pipeDocName).Value == re.Match(element2DocName).Value ? 8 : 0;
                    switch (x1 + x2 + x3 + x4)
                    {
                        case 9:
                            hostOrder = 0;
                            pipeOrder = 1;
                            break;

                        case 6:
                            hostOrder = 1;
                            pipeOrder = 0;
                            break;

                        case 1:
                        case 2:
                            Autodesk.Revit.UI.TaskDialog.Show("Ошибка", "Файл связи не соответствует файлу из отчета");
                            return result;

                        default:
                            Autodesk.Revit.UI.TaskDialog.Show("Ошибка", "Не удалось определить файлы из отчета");
                            return result;
                    }
                    // id элементов пересечения
                    int hostId = int.Parse(id_nodes[hostOrder]);
                    int pipeId = int.Parse(id_nodes[pipeOrder]);
                    // точка пересечения
                    string[] array_str_point = str_point.Replace("m", string.Empty).Replace(" \r\n \t", string.Empty).Split(',');
                    double x = Double.Parse(array_str_point[0].Replace('.',','));
                    double y = Double.Parse(array_str_point[1].Replace('.', ','));
                    double z = Double.Parse(array_str_point[2].Replace('.', ','));
                    x = Autodesk.Revit.DB.UnitUtils.ConvertToInternalUnits(x, Autodesk.Revit.DB.DisplayUnitType.DUT_METERS);
                    y = Autodesk.Revit.DB.UnitUtils.ConvertToInternalUnits(y, Autodesk.Revit.DB.DisplayUnitType.DUT_METERS);
                    z = Autodesk.Revit.DB.UnitUtils.ConvertToInternalUnits(z, Autodesk.Revit.DB.DisplayUnitType.DUT_METERS);

                    Autodesk.Revit.DB.XYZ pt = new Autodesk.Revit.DB.XYZ(x, y, z);
                    // Пересечение
                    Intersection i = new Intersection(hostdoc, hostId, pipedoc, pipeId, pt);
                    result.Add(i);
                }
                return result;
            }
        }


        private void loadBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Отчет о коллизиях (*.html)|*.html";
            dialog.Multiselect = false;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Intersections =  HAP_HTMLReportParse(dialog.FileName);
                UpdateTableValues();
            }
        }
    }
}
