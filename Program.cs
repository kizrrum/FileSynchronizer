using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.ServiceProcess;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace FileSynchronizer
{
    [System.ComponentModel.RunInstaller(true)]
    public class Installer : System.Configuration.Install.Installer
    {
        private ServiceInstaller serviceInstaller;
        private ServiceProcessInstaller processInstaller;

        public Installer()
        {
            // Instantiate installers for process and services.
            processInstaller = new ServiceProcessInstaller();
            serviceInstaller = new ServiceInstaller();

            // The services run under the system account.
            processInstaller.Account = ServiceAccount.LocalSystem;

            // The services are started manually.
            serviceInstaller.StartType = ServiceStartMode.Manual;

            // ServiceName must equal those on ServiceBase derived classes.
            serviceInstaller.ServiceName = "aHello-World Service 1";

            // Add installers to collection. Order is not important.
            Installers.Add(serviceInstaller);
            Installers.Add(processInstaller);
        }
    }
    public class Program
    {
        //private static readonly LogFile _logFile = new LogFile("sync.log");
        public static string logFile = AppDomain.CurrentDomain.BaseDirectory + "sync.log";
       
    public static bool Consolemode;
        public static int delayTime = 1;
        public static string FileSynchronizerSvc = "FileSynchronizerSvc";
        public static string exactPath = AppDomain.CurrentDomain.BaseDirectory;
        public static string filePath = AppDomain.CurrentDomain.BaseDirectory + "FileSynchronizer.ini";
        static void Main(string[] args)
        {
            // Читаем значения из ini файла
            //var MyIni = new IniFile();
            //sourcePath2 = MyIni.Read(@"sourcePath");
            //destinationPath2 = MyIni.Read(@"destinationPath");
            if (!File.Exists(logFile))
            {
                File.Create(logFile).Close();
            }
            sourcePath2 = null;
            destinationPath2 = null;
            if (File.Exists(filePath))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    sourcePath2 = reader.ReadLine();
                    destinationPath2 = reader.ReadLine();
                    string delaystr = reader.ReadLine();
                    //delayTime = int.Parse(delaystr.Substring(6));
                    string numericText = new String(delaystr.Where(Char.IsDigit).ToArray());
                    int delayTime = int.Parse(numericText);
                    Console.WriteLine("sourcePath " + sourcePath2);
                    Console.WriteLine("destinationPath " + destinationPath2);
                    Console.WriteLine("delayTime " + delayTime);
                    //  Console.ReadLine();
                }
            }
            else
            {
                { Console.WriteLine("FileSynchronizer.ini not found ");  }
                Environment.Exit(0);
            }

            //Console.Write(delayTime);
            if (!Directory.Exists(sourcePath2))
            {
                Directory.CreateDirectory(sourcePath2);
            }
            if (!Directory.Exists(destinationPath2))
            {
                Directory.CreateDirectory(destinationPath2);
            }
            if (Environment.UserInteractive)
            {
                // Консольный режим
                Console.WriteLine("Console mode");
                //Console.ReadLine();
                Consolemode = true;
            }
            else
            {
                Consolemode = false;
                // Режим службы
                ServiceBase[] servicesToRun = new ServiceBase[]
                {
                    new MyService()
                };
                ServiceBase.Run(servicesToRun);
            }

            if (args.Length == 0)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("-install (-i)   | Installs the application.");
                Console.WriteLine("-uninstall (-u) | Uninstalls the application.");
                Console.WriteLine("-start (-s)     | Starts the application.");
                // Console.WriteLine("-stop (-t)      | Stops the application.");
                Console.WriteLine("-delay (-d)     | Delay time. Default 1 sec. Example: -d 10");
                return;
            }



            bool startJob = false;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i].ToLower();
                switch (arg)
                {
                    case "-install":
                    case "-i":
                        Console.WriteLine("Installing the application...");
                        Install();
                        break;
                    case "-uninstall":
                    case "-u":
                        Console.WriteLine("Uninstalling the application...");
                        Uninstall();
                        break;
                    case "-start":
                    case "-s":
                        Console.WriteLine("Starting the application...");
                        startJob = true;
                        break;
                    case "-stop":
                    case "-t":
                        Console.WriteLine("Stopping the application...");
                        cancl = true;
                        break;
                    case "-delay":
                    case "-d":
                        Console.WriteLine("Delay time assign");
                        if (i < args.Length - 1 && int.TryParse(args[i + 1], out int delay) && delay >= 1)
                        {
                            delayTime = delay;
                            Console.WriteLine($"Delay time set to {delayTime}");
                        }
                        else
                        {
                            Console.WriteLine("Invalid delay time argument");
                        }
                        break;
                    default:
                        Console.WriteLine("Invalid argument: " + arg);
                        break;
                }
            }
            //Console.WriteLine(startJob);
            if (startJob)
            {
                Job(sourcePath2, destinationPath2);
            }


        }

        public static void Install()
        {
            if (IsUserAdministrator())
            {
                if (!IsServiceExists(FileSynchronizerSvc))
                {
                    string assemblyPath = "\"" + Assembly.GetEntryAssembly().Location + "\"";
                    System.Diagnostics.Process proc = new System.Diagnostics.Process();
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    proc.StartInfo.FileName = "sc.exe";
                    proc.StartInfo.Arguments = "create " + FileSynchronizerSvc + " binPath= " + assemblyPath;
                    proc.Start();
                    proc.WaitForExit();

                    List<string> results = new List<string>();
                    while (!proc.StandardOutput.EndOfStream)
                    {
                        string currentline = proc.StandardOutput.ReadLine();
                        if (!string.IsNullOrEmpty(currentline))
                        {
                            results.Add(currentline);
                        }
                    }
                    foreach (string result in results)
                    {
                        Console.WriteLine(result);
                    }
                    // InstallAndStart();
                    System.Diagnostics.Process proc2 = new System.Diagnostics.Process();
                    proc2.StartInfo.UseShellExecute = false;
                    proc2.StartInfo.RedirectStandardOutput = true;
                    proc2.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    proc2.StartInfo.FileName = "sc.exe";
                    proc2.StartInfo.Arguments = "start " + FileSynchronizerSvc;
                    proc2.Start();
                    proc2.WaitForExit();

                    List<string> results2 = new List<string>();
                    while (!proc2.StandardOutput.EndOfStream)
                    {
                        string currentline = proc2.StandardOutput.ReadLine();
                        if (!string.IsNullOrEmpty(currentline))
                        {
                            results2.Add(currentline);
                        }
                    }
                    foreach (string result in results2)
                    {
                        Console.WriteLine(result);
                    }
                }
                else { Console.WriteLine("Error: ServiceExists"); Environment.Exit(0); }
                // _logger.Debug("ServiceExists");
            }
            else { Console.WriteLine("Error: run as administrator");  Environment.Exit(0); }
            //_logger.Debug("run as administrator");
        }

        private static void Uninstall()
        {
            if (IsUserAdministrator())
            {
                if (IsServiceExists(FileSynchronizerSvc))
                {
                    Process p = new Process();
                    p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    p.StartInfo.FileName = "sc.exe";
                    p.StartInfo.Arguments = "delete " + FileSynchronizerSvc;
                    Console.WriteLine("sc delete " + FileSynchronizerSvc);
                    p.Start();
                    Thread.Sleep(1000);
                }

            }
            else { Console.WriteLine("run as administrator"); }
        }
        public static bool cancl = false;
        public static string sourcePath2;
        public static string destinationPath2;
        // Создание логгера NLog
        private static Logger _logger;
        private static FileTarget _fileTarget;
        private static LoggingConfiguration _config;
        public static void Job(string sourcePath, string destinationPath)
        {
            sourcePath2 = sourcePath;
            destinationPath2 = destinationPath;
            // Создаем и настраиваем объект FileTarget
            _fileTarget = new FileTarget
            {
                FileName = logFile,
                ConcurrentWrites = true, // Включаем поддержку многопоточной записи
                KeepFileOpen = true // Оставляем файл открытым между записями
            };

            // Создаем и настраиваем объект Logger
            _config = new LoggingConfiguration();
            _config.AddTarget("file", _fileTarget);
            _config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, _fileTarget));
            LogManager.Configuration = _config;
            _logger = LogManager.GetCurrentClassLogger();


            //sourcePath2 = (@"C:\Users\1\Documents\Графика\Filter Library");
            //destinationPath2 = (@"C:\Users\1\Documents\Графика\fest");
            if (Consolemode)
            {
                Console.Write(DateTime.Now + " tracking copying, deletion\n\n");
                Console.Write("sourcePath:" + sourcePath2 + "\n\n");
                Console.Write("destinationPath:" + destinationPath2 + "\n\n");
            }
            _logger.Debug("Doing some work...");

            // new Thread(() =>
            // {
            while (true)
            {
                // Console.Write("cancl - " + cancl + DateTime.Now);
                CopyDirectory(sourcePath2, destinationPath2);
                Deloutsync(sourcePath2, destinationPath2);

                Thread.Sleep(delayTime * 1000);
                //Thread.Sleep(1000);
                if (cancl == true)
                { break; }
            }
            // })
            //{ IsBackground = true }.Start();
        }

        public static void Deloutsync(string sourcePath, string destinationPath)
        {
            // Создаем и настраиваем объект FileTarget
            _fileTarget = new FileTarget
            {
                FileName = logFile,
                ConcurrentWrites = true, // Включаем поддержку многопоточной записи
                KeepFileOpen = true // Оставляем файл открытым между записями
            };

            // Создаем и настраиваем объект Logger
            _config = new LoggingConfiguration();
            _config.AddTarget("file", _fileTarget);
            _config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, _fileTarget));
            LogManager.Configuration = _config;
            _logger = LogManager.GetCurrentClassLogger();

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
                        if (Consolemode)
                        {
                            Console.WriteLine($"{fileName} already exists. delete");

                            //File.AppendAllText(logFile, DateTime.Now + "," + $"{fileName} already exists. delete" + Environment.NewLine);                       
                        }
                        _logger.Debug($"{fileName} already exists. delete");
                        File.Delete(file);
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            // Удаляем каталоги, которых нет в sourcePath
            try
            {
                foreach (string directory in destinationDirectories)
                {
                    string directoryName = Path.GetFileName(directory);
                    string directoryParent = Path.GetDirectoryName(directory).Replace(destinationPath, sourcePath);

                    if (!sourceDirectories.Any(d => Path.GetFileName(d) == directoryName && Path.GetDirectoryName(d) == directoryParent))
                    {
                        if (Consolemode)
                        {
                            Console.WriteLine($"{directory} already exists. delete");
                        }
                        _logger.Debug($"{directory} already exists. delete");
                        Directory.Delete(directory, true);
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            //}
            //catch (Exception ex)
            // {
            if (Consolemode)
            { //Console.WriteLine($"An error occurred: {ex.Message}"); 

                // _logFile.WriteToFile(DateTime.Now + "," + $"An error occurred: {ex.Message}");
            }
            // }
        }

        public static void CopyDirectory(string sourcePath, string destinationPath)
        {
            // Создаем и настраиваем объект FileTarget
            _fileTarget = new FileTarget
            {
                FileName = logFile,
                ConcurrentWrites = true, // Включаем поддержку многопоточной записи
                KeepFileOpen = true // Оставляем файл открытым между записями
            };

            // Создаем и настраиваем объект Logger
            _config = new LoggingConfiguration();
            _config.AddTarget("file", _fileTarget);
            _config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, _fileTarget));
            LogManager.Configuration = _config;
            _logger = LogManager.GetCurrentClassLogger();

            try
            {
                // Создаем директорию назначения, если она не существует
                if (!Directory.Exists(destinationPath))
                {
                    Directory.CreateDirectory(destinationPath);
                    if (Consolemode) { Console.WriteLine(DateTime.Now + "," + $"{destinationPath} already created."); }
                    _logger.Debug($"{destinationPath} already created.");
                }
                // Копируем файлы
                foreach (string filePath in Directory.GetFiles(sourcePath))
                {
                    string newFilePath = Path.Combine(destinationPath, Path.GetFileName(filePath));
                    // try
                    // {
                    if (File.Exists(newFilePath))
                    {
                        DateTime sourceLastWriteTime = File.GetLastWriteTime(filePath);
                        DateTime destinationLastWriteTime = File.GetLastWriteTime(newFilePath);
                        if (sourceLastWriteTime > destinationLastWriteTime)
                        {
                            File.Copy(filePath, newFilePath, true);
                            if (Consolemode)
                            {
                                Console.WriteLine($"{filePath} already copied.");
                            }
                            _logger.Debug($"{filePath} already copied.");
                        }
                    }
                    else
                    {
                        File.Copy(filePath, newFilePath, true);
                        if (Consolemode)
                        {
                            Console.WriteLine($"{filePath} already copied.");
                        }
                        _logger.Debug($"{filePath} already copied.");
                    }
                    // }
                    //catch (IOException ex)
                    //{
                    // Обработка ошибки
                    if (Consolemode)
                    {// Console.WriteLine($"error {filePath}: {ex.Message}"); 

                        //  _logFile.WriteToFile(DateTime.Now + "," + $"error {filePath}: {ex.Message}");
                    }
                    //}
                }
            }
            catch (DirectoryNotFoundException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            try
            {
                // Рекурсивно копируем вложенные директории
                foreach (string directoryPath in Directory.GetDirectories(sourcePath))
                {
                    Thread.Sleep(1000);
                    string newDirectoryPath = Path.Combine(destinationPath, Path.GetFileName(directoryPath));
                    CopyDirectory(directoryPath, newDirectoryPath);
                }
            }
            catch (DirectoryNotFoundException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        public static bool IsUserAdministrator()
        {
            WindowsIdentity user = WindowsIdentity.GetCurrent();
            if (user == null)
            {
                return false;
            }
            WindowsPrincipal principal = new WindowsPrincipal(user);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        private static bool IsServiceExists(string serviceName)
        {
            ServiceController[] services = ServiceController.GetServices();
            var service = services.FirstOrDefault(s => s.ServiceName == serviceName);
            return service != null;
        }
        public class MyService : ServiceBase
        {
            protected override void OnStart(string[] args)
            {
                // Запускаем отдельный поток для выполнения действий
                Thread thread = new Thread(DoWork);
                thread.Start();
            }
            private void DoWork()
            {
                Job(sourcePath2, destinationPath2);
            }
            protected override void OnStop()
            {
                cancl = true;
            }
        }
    }
}
