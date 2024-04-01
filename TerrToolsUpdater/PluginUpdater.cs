using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace TerrToolsUpdater
{
    class PluginUpdater
    {        
        static string dirTo = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Revit\Addins\";
        static string[] fileNames =
            {
            "TerrTools.addin",
            "TerrToolsDLL",
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

        static void FileCopy(string src, string dst)
        {
            if (!File.Exists(src)) Console.WriteLine(string.Format("Не найдем файл {0}", src));
            else
            {
                WaitForFile(dst);
                File.Copy(src, dst, true);                
            }
        }

        static void DirectoryCopy(string src, string dst, bool copySubDirs)
        {
            if (!Directory.Exists(src)) Console.WriteLine(string.Format("Не найдена папка {0}", src));
            else
            {
                if (!Directory.Exists(dst))
                {
                    Directory.CreateDirectory(dst);
                }
                DirectoryInfo srcDir = new DirectoryInfo(src);
                FileInfo[] files = srcDir.GetFiles();
                foreach (FileInfo file in files)
                {
                    string destPath = Path.Combine(dst, file.Name);
                    FileCopy(file.FullName, destPath);
                }

                // копируем подпапки
                if (copySubDirs)
                {
                    DirectoryInfo[] subSrcDirs = srcDir.GetDirectories();
                    foreach (DirectoryInfo subdir in subSrcDirs)
                    {
                        // Create the subdirectory.
                        string temppath = Path.Combine(dst, subdir.Name);

                        // Copy the subdirectories.
                        DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                    }
                }
            }
        }

        static void CopyFiles()
        {
            string[] foldersTo = Directory.GetDirectories(dirTo);
            foreach (string folderTo in foldersTo)
            {
                foreach (string fileName in fileNames)
                {
                    string src = Path.Combine(Directory.GetCurrentDirectory(), fileName);
                    string dst = Path.Combine(folderTo, fileName);
                    FileAttributes attr = File.GetAttributes(src);

                    // копируем папку и ее содержимое
                    if (attr.HasFlag(FileAttributes.Directory))
                    {
                        DirectoryCopy(src, dst, true);
                        Console.WriteLine(string.Format("Папка {0} успешно скопирована в папку {1}", fileName, folderTo));
                    }

                    // копируем файлы
                    else
                    {
                        FileCopy(src, dst);
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
