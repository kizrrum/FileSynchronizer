using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace FileSynchronizer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            job();
        }
        public static bool cancl = false;
        public static void job()
        {    // Читаем значения из ini файла
            var MyIni = new IniFile();
            var sourcePath2 = MyIni.Read(@"sourcePath");
            var destinationPath2 = MyIni.Read(@"destinationPath");
            Console.Write(sourcePath2);
            Console.Write(destinationPath2);
            while (cancl == false)
            {
                CopyDirectory(sourcePath2, destinationPath2);
                deloutsync(sourcePath2, destinationPath2);
                Thread.Sleep(1000);
            }
        }
        public static void deloutsync(string sourcePath, string destinationPath)
        {
            string[] sourceFiles = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);
            string[] sourceDirectories = Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories);
            string[] destinationFiles = Directory.GetFiles(destinationPath, "*", SearchOption.AllDirectories);
            string[] destinationDirectories = Directory.GetDirectories(destinationPath, "*", SearchOption.AllDirectories);
            // Удаляем файлы, которых нет в sourcePath
            try
            {
                foreach (string file in destinationFiles)
                {
                    string fileName = Path.GetFileName(file);
                    string fileDirectory = Path.GetDirectoryName(file).Replace(destinationPath, sourcePath);

                    if (!sourceFiles.Any(f => Path.GetFileName(f) == fileName && Path.GetDirectoryName(f) == fileDirectory))
                    {
                        Console.WriteLine($"{fileName} already exists. delete");
                        WriteToFile($"{fileName} already exists. delete");
                        File.Delete(file);
                    }
                }
                // Удаляем каталоги, которых нет в sourcePath
                foreach (string directory in destinationDirectories)
                {
                    string directoryName = Path.GetFileName(directory);
                    string directoryParent = Path.GetDirectoryName(directory).Replace(destinationPath, sourcePath);

                    if (!sourceDirectories.Any(d => Path.GetFileName(d) == directoryName && Path.GetDirectoryName(d) == directoryParent))
                    {
                        Console.WriteLine($"{directory} already exists. delete");
                        WriteToFile($"{directory} already exists. delete");
                        Directory.Delete(directory, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                WriteToFile($"An error occurred: {ex.Message}");
            }
        }
        private static readonly object _locker = new object();
        private static void WriteToFile(string info)
        {
            Monitor.Enter(_locker);
            try
            {
                TextWriter tw = new StreamWriter("sync.log", true);
                tw.WriteLine(info);
                tw.Close();
            }
            finally
            {
                Monitor.Exit(_locker);
            }
        }
        public static void CopyDirectory(string sourcePath, string destinationPath)
        {
            // Создаем директорию назначения, если она не существует
            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
                WriteToFile($"{destinationPath} already created.");
            }
            // Копируем файлы
            foreach (string filePath in Directory.GetFiles(sourcePath))
            {
                string newFilePath = Path.Combine(destinationPath, Path.GetFileName(filePath));
                try
                {
                    if (File.Exists(newFilePath))
                    {
                        DateTime sourceLastWriteTime = File.GetLastWriteTime(filePath);
                        DateTime destinationLastWriteTime = File.GetLastWriteTime(newFilePath);
                        if (sourceLastWriteTime > destinationLastWriteTime)
                        {
                            File.Copy(filePath, newFilePath, true);
                            Console.WriteLine($"{filePath} already copied.");
                            WriteToFile($"{filePath} already copied.");

                        }
                    }
                    else
                    {
                        File.Copy(filePath, newFilePath, true);
                        Console.WriteLine($"{filePath} already copied.");
                        WriteToFile($"{filePath} already copied.");
                    }
                }
                catch (IOException ex)
                {
                    // Обработка ошибки
                    Console.WriteLine($"Ошибка при копировании файла {filePath}: {ex.Message}");
                    WriteToFile($"Ошибка при копировании файла {filePath}: {ex.Message}");
                }
            }
            // Рекурсивно копируем вложенные директории
            foreach (string directoryPath in Directory.GetDirectories(sourcePath))
            {
                Thread.Sleep(1000);
                string newDirectoryPath = Path.Combine(destinationPath, Path.GetFileName(directoryPath));
                CopyDirectory(directoryPath, newDirectoryPath);

            }
        }
    }
}
