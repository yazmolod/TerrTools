using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Printing;

namespace TerrTools.UI
{
    public partial class AskPaperFormatForm : Form
    {
        public PaperSize PaperSize { get; set; }
        public bool IsRotated { get; set; }
        public AskPaperFormatForm(string sheetname, string printerName)
        {
            InitializeComponent();
            label1.Text = $"Не удалось автоматически определить формат листа для\n\"{sheetname}\"\n" +
                $"Выберите самостоятельно или пропустите лист\n\n" +
                $"Примечание: если нужного формата нет в списке,\nвам необходимо настроить формат в самом принтере\n({printerName})";
            PrinterSettings pd = new PrinterSettings();
            pd.PrinterName = printerName;
            List<PaperSize> pslist = new List<PaperSize>();
            foreach (PaperSize ps in pd.PaperSizes)
            {
                pslist.Add(ps);
            }
            comboBox1.DataSource = pslist;
            comboBox1.DisplayMember = "PaperName";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PaperSize = comboBox1.SelectedItem as PaperSize;
            IsRotated = checkBox1.Checked;
            this.DialogResult = DialogResult.OK;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
