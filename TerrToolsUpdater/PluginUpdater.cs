using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace TerrToolsUpdater
{
    class PluginUpdater
    {
        static string folderFrom = @"\\serverL\PSD\REVIT\Плагины\TerrTools\";
        static string[] foldersTo =
            {
            @"C:\ProgramData\Autodesk\Revit\Addins\2019\",
            @"C:\ProgramData\Autodesk\Revit\Addins\2017\"
        };
        static string[] fileNames =
            {
            "TerrTools.addin",
            "TerrTools.dll"
        };

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        static void CopyFiles()
        {
            foreach (string folderTo in foldersTo)
            {
                foreach (string fileName in fileNames)
                {
                    string src = Path.Combine(folderFrom, fileName);
                    string dst = Path.Combine(folderTo, fileName);
                    if (!File.Exists(src)) Console.WriteLine(string.Format("Не найдем файл {0}", src));
                    else if (!Directory.Exists(folderTo)) Console.WriteLine(string.Format("Не удалось скопировать в путь {0}", folderTo));
                    else
                    {
                        File.Copy(src, dst, true);
                        Console.WriteLine(string.Format("Файл {0} успешно скопирован в папку {1}", fileName, folderTo));
                    }
                }
            }
        }

        
        static void Main(string[] args)
        {
            // запуск из ревита
            if (args.Length > 0 && args[0] == "-fromRevit")
            {
                ShowWindow(GetConsoleWindow(), SW_HIDE);
                Process[] processes = Process.GetProcessesByName("Revit");
                if (processes.Length > 0)
                {
                    Process revitProcess = processes.First();
                    string processPath = revitProcess.MainModule.FileName;
                    revitProcess.WaitForExit();
                    CopyFiles();
                    if (args.Length > 1 && args[1] == "-restart") 
                        Process.Start(processPath);
                }
            }
            // запуск с диска
            else
            {
                CopyFiles();
                Thread.Sleep(3000);
            }
            
        }
    }
}
