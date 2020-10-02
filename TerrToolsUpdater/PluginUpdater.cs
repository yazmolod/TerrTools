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
            @"C:\ProgramData\Autodesk\Revit\Addins\2017\",
            @"C:\ProgramData\Autodesk\Revit\Addins\2018\",
            @"C:\ProgramData\Autodesk\Revit\Addins\2019\",
            @"C:\ProgramData\Autodesk\Revit\Addins\2020\",
            @"C:\ProgramData\Autodesk\Revit\Addins\2021\",
            @"C:\ProgramData\Autodesk\Revit\Addins\2022\",
            @"C:\ProgramData\Autodesk\Revit\Addins\2023\",
            @"C:\ProgramData\Autodesk\Revit\Addins\2024\",
            @"C:\ProgramData\Autodesk\Revit\Addins\2025\",
            @"C:\ProgramData\Autodesk\Revit\Addins\2026\",
            @"C:\ProgramData\Autodesk\Revit\Addins\2027\",
            @"C:\ProgramData\Autodesk\Revit\Addins\2028\",
            @"C:\ProgramData\Autodesk\Revit\Addins\2029\",
            @"C:\ProgramData\Autodesk\Revit\Addins\2030\",

        };
        static string[] fileNames =
            {
            "TerrTools.addin",
            "TerrTools.dll",
             "HtmlAgilityPack.dll",
        };

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        public static bool IsFileReady(string filename)
        {
            if (!File.Exists(filename))
            {
                return true;
            }
            try
            {
                using (FileStream inputStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None))
                    return inputStream.Length > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static void WaitForFile(string filename)
        {
            var start = DateTime.Now;

            while (!IsFileReady(filename))
            {
                var diff = DateTime.Now - start;
                if (diff.TotalSeconds > 10)
                {
                    throw new Exception("Файл " + filename + " занят другим процессом");
                }
            }
        }

        static void CopyFiles()
        {
            foreach (string folderTo in foldersTo)
            {
                foreach (string fileName in fileNames)
                {
                    string src = Path.Combine(folderFrom, fileName);
                    string dst = Path.Combine(folderTo, fileName);
                    if (!File.Exists(src)) Console.WriteLine(string.Format("Не найдем файл {0}", src));
                    else if (!Directory.Exists(folderTo))
                    {
                        //Console.WriteLine(string.Format("Не удалось скопировать в путь {0}", folderTo));
                    }
                    else
                    {
                        WaitForFile(dst);
                        File.Copy(src, dst, true);
                        Console.WriteLine(string.Format("Файл {0} успешно скопирован в папку {1}", fileName, folderTo));
                    }
                }
            }
        }
        
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine(">> Начинаю обновление");
                CopyFiles();
                Console.WriteLine(">> Обновление завершено");
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n>> ПРОИЗОШЛА ОШИБКА, ОБНОВЛЕНИЕ НЕ ПРОИЗВЕДЕНО");
                Console.WriteLine(ex.ToString());
            }
            Console.WriteLine("\nНажмите любую клавишу или закройте окно...");
            Console.ReadKey();
        }
    }
}
