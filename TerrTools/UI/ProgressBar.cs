using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace TerrTools.UI
{
    public partial class ProgressBar : Form
    {
        public ProgressBar(string operation, int count)
        {
            InitializeComponent();
            this.progressBar1.Maximum = count;
            this.progressBar1.Value = 0;
            this.progressBar1.Step = 1;
            this.Text = operation;
            this.Show(GetRevitHandle());
        }
        public void StepUp()
        {
            this.progressBar1.PerformStep();
            int current = this.progressBar1.Value;
            int maximum = this.progressBar1.Maximum;
            this.label1.Text = string.Format("{0} / {1}", current, maximum);
            if (current >= maximum)
            {
                this.Close();
            }
        }

        private WindowHandle GetRevitHandle()
        {
            Process[] processes = Process.GetProcessesByName("Revit");

            if (0 < processes.Length)
            {
                IntPtr h = processes[0].MainWindowHandle;
                return new WindowHandle(h);
            }
            else return null;
        }
    }

    public class WindowHandle : IWin32Window
    {
        IntPtr _hwnd;

        public WindowHandle(IntPtr h)
        {
            _hwnd = h;
        }
        public IntPtr Handle
        {
            get { return _hwnd; }
        }
    }
}
