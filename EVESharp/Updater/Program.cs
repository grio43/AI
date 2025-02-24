using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Updater
{
    internal class Program
    {
        #region Properties

        private static string AssemblyPath
        {
            get
            {
                if (_assemblyPath == null)
                    _assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                return _assemblyPath;
            }
        }

        #endregion Properties

        #region Fields

        private static readonly string DOWNLOADNAME = "EVESharp";
        private static readonly string EXE_FILE_TO_EXECUTE_AFTERWARDS = "EVESharpLauncher.exe";
        private static readonly string GIT_ZIP_DIRECTORY_NAME = "EVESharp-master";
        private static readonly string SOURCE_DIRECTORY = "Resources\\EVESharpSource";
        private static readonly string ZIP_FILENAME = "EVESharp-master.zip";
        private static string _assemblyPath;
        private static WebClient client;
        private static string DOWNLOAD_LINK = "https://codeload.github.com/duketwo/EVESharp/zip/master";
        private static DateTime LAST_DOWNLOAD_PROGRESS_REFRESH = DateTime.MinValue;
        private static bool working = true;

        #endregion Fields

        #region Methods

        private static void CompileAll()
        {
            string msBuild = GetMsBuildExePath();
            string e1 = "\"EVESharp.sln\" /p:configuration=\"Debug\" /t:Postbuild:Clean /m";
            string e2 = "\"EVESharp.sln\" /p:configuration=\"Debug\" /t:Postbuild:Rebuild /m";

            Console.WriteLine("Compiling all.");
            string questorLauncherSourcePath = AssemblyPath + Path.DirectorySeparatorChar + SOURCE_DIRECTORY + Path.DirectorySeparatorChar +
                                               GIT_ZIP_DIRECTORY_NAME + Path.DirectorySeparatorChar;

            Execute(msBuild, e1, questorLauncherSourcePath, false, false, false);
            Execute(msBuild, e2, questorLauncherSourcePath, false, false, false);

            Console.WriteLine("Done compiling all.");
        }

        private static void CopyEveSharpBinaries()
        {
            Console.WriteLine("CopyEveSharpBinaries");
            string sourcePath = AssemblyPath + Path.DirectorySeparatorChar + SOURCE_DIRECTORY + Path.DirectorySeparatorChar + GIT_ZIP_DIRECTORY_NAME +
                                Path.DirectorySeparatorChar + "output";
            string destinationPath = AssemblyPath;

            foreach (string file in Directory.GetFiles(destinationPath)
                .Where(item => Path.GetFileName(item).StartsWith("SharedComponents.dll") || Path.GetFileName(item).StartsWith("Updater.exe") ||
                               Path.GetFileName(item).StartsWith("geckodriver.exe")))
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception [" + ex + "]");
                }

            if (!Directory.Exists(destinationPath))
                Directory.CreateDirectory(destinationPath);

            if (!Directory.Exists(sourcePath))
                return;

            Random rnd = new Random();

            string updaterBakName = "SharedComponents.dll." + rnd.Next(99999, 9999999) + ".bak";
            string utilityBakName = "Updater.exe." + rnd.Next(99999, 9999999) + ".bak";

            while (File.Exists(Path.Combine(destinationPath, updaterBakName)))
                updaterBakName = "Updater.exe." + rnd.Next(99999, 9999999) + ".bak";

            while (File.Exists(Path.Combine(destinationPath, utilityBakName)))
                utilityBakName = "SharedComponents.dll." + rnd.Next(99999, 9999999) + ".bak";

            if (File.Exists(destinationPath + Path.DirectorySeparatorChar + "Updater.exe"))
                Execute("cmd.exe", "/C ren Updater.exe " + updaterBakName, destinationPath);

            if (File.Exists(destinationPath + Path.DirectorySeparatorChar + "SharedComponents.dll"))
                Execute("cmd.exe", "/C ren SharedComponents.dll " + utilityBakName, destinationPath);

            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                if (!Directory.Exists(dirPath.Replace(sourcePath, destinationPath)))
                    Directory.CreateDirectory(dirPath.Replace(sourcePath, destinationPath));

            try
            {
                foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                    if (newPath.EndsWith(".exe") || newPath.EndsWith(".dll") || newPath.EndsWith(".pdb"))
                    {
                        try
                        {
                            File.Copy(newPath, newPath.Replace(sourcePath, destinationPath), true);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Exception [" + ex + "]");
                        }
                    }
                    else
                    {
                        if (!File.Exists(newPath.Replace(sourcePath, destinationPath)))
                            try
                            {
                                File.Copy(newPath, newPath.Replace(sourcePath, destinationPath), false);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Exception [" + ex + "]");
                            }
                    }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception [" + ex + "]");
            }

            Console.WriteLine("Done CopyEveSharpBinaries");
        }

        private static void CopyQuestorBinaries()
        {
            Console.WriteLine("CopyQuestorBinaries");

            string sourcePath = AssemblyPath + Path.DirectorySeparatorChar + SOURCE_DIRECTORY + Path.DirectorySeparatorChar + GIT_ZIP_DIRECTORY_NAME +
                                Path.DirectorySeparatorChar + "output";
            string destinationPath = AssemblyPath;

            foreach (string file in Directory.GetFiles(destinationPath)
                .Where(item => Path.GetFileName(item).StartsWith("SharedComponents.dll") || Path.GetFileName(item).StartsWith("EVESharpCore.exe") || Path.GetFileName(item).StartsWith("EVESharpCore")))
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception [" + ex + "]");
                }

            if (!Directory.Exists(sourcePath))
                return;

            if (!Directory.Exists(destinationPath))
                Directory.CreateDirectory(destinationPath);

            Random rnd = new Random();

            string questorBakName = "EVESharpCore.exe." + rnd.Next(99999, 9999999) + ".bak";
            string questorPdbBakName = "EVESharpCore.pdb." + rnd.Next(99999, 9999999) + ".bak";
            string utilityBakName = "SharedComponents.dll." + rnd.Next(99999, 9999999) + ".bak";

            while (File.Exists(Path.Combine(destinationPath, questorBakName)))
                questorBakName = "EVESharpCore.exe." + rnd.Next(99999, 9999999) + ".bak";

            while (File.Exists(Path.Combine(destinationPath, questorPdbBakName)))
                questorPdbBakName = "EVESharpCore.pdb." + rnd.Next(99999, 9999999) + ".bak";

            while (File.Exists(Path.Combine(destinationPath, utilityBakName)))
                utilityBakName = "SharedComponents.dll." + rnd.Next(99999, 9999999) + ".bak";

            if (File.Exists(destinationPath + Path.DirectorySeparatorChar + "SharedComponents.dll"))
                Execute("cmd.exe", "/C ren SharedComponents.dll " + utilityBakName, destinationPath);

            if (File.Exists(destinationPath + Path.DirectorySeparatorChar + "EVESharpCore.exe"))
                Execute("cmd.exe", "/C ren EVESharpCore.exe " + questorBakName, destinationPath);

            if (File.Exists(destinationPath + Path.DirectorySeparatorChar + "EVESharpCore.pdb"))
                Execute("cmd.exe", "/C ren EVESharpCore.pdb " + questorPdbBakName, destinationPath);

            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                if (!Directory.Exists(dirPath.Replace(sourcePath, destinationPath)))
                    Directory.CreateDirectory(dirPath.Replace(sourcePath, destinationPath));

            try
            {
                foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                    if (newPath.EndsWith(".exe") || newPath.EndsWith(".dll") || newPath.EndsWith(".pdb"))
                    {
                        try
                        {
                            File.Copy(newPath, newPath.Replace(sourcePath, destinationPath), true);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Exception [" + ex + "]");
                        }
                    }
                    else
                    {
                        if (!File.Exists(newPath.Replace(sourcePath, destinationPath)))
                            try
                            {
                                File.Copy(newPath, newPath.Replace(sourcePath, destinationPath), false);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Exception [" + ex + "]");
                            }
                    }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception [" + ex + "]");
            }

            Console.WriteLine("Done CopyQuestorBinaries");
        }

        private static void DownloadEveSharp(string username, string password)
        {
            try
            {
                string questorSourcePath = AssemblyPath + Path.DirectorySeparatorChar + SOURCE_DIRECTORY + Path.DirectorySeparatorChar;

                Console.WriteLine("Downloading " + DOWNLOADNAME + " from: " + DOWNLOAD_LINK + " to: " + questorSourcePath + ZIP_FILENAME);

                if (!Directory.Exists(questorSourcePath))
                    Directory.CreateDirectory(questorSourcePath);

                Thread thread = new Thread(() =>
                {
                    client = new WebClient();
                    string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));
                    client.Headers[HttpRequestHeader.Authorization] = string.Format("Basic {0}", credentials);
                    client.DownloadProgressChanged += client_DownloadProgressChanged;
                    client.DownloadFileCompleted += client_DownloadFileCompleted;
                    client.DownloadFileAsync(new Uri(DOWNLOAD_LINK), questorSourcePath + ZIP_FILENAME);
                });
                thread.Start();

                while (thread.IsAlive || client.IsBusy || working)
                {
                    Application.DoEvents();
                    Thread.Sleep(10);
                }

                client.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception [" + ex + "]");
            }
        }

        private static void Execute(string filename, string arguments, string workingDirectory, bool async = false, bool useShellExec = false,
            bool createNoWindow = true)
        {
            Process p = new Process();
            p.StartInfo.UseShellExecute = useShellExec;
            p.StartInfo.FileName = filename;
            p.StartInfo.Arguments = arguments;
            p.StartInfo.WorkingDirectory = workingDirectory;
            p.StartInfo.CreateNoWindow = createNoWindow;
            p.Start();
            if (!async)
                p.WaitForExit();
        }

        private static void ExtractEveSharp()
        {
            string questorLauncherSourcePath = AssemblyPath + Path.DirectorySeparatorChar + SOURCE_DIRECTORY + Path.DirectorySeparatorChar;
            string qLZipFile = questorLauncherSourcePath + ZIP_FILENAME;

            Console.WriteLine("Extracting " + DOWNLOADNAME + " from: " + qLZipFile);

            if (Directory.Exists(questorLauncherSourcePath + GIT_ZIP_DIRECTORY_NAME))
            {
                Console.WriteLine("Deleting previously existing Directory " + questorLauncherSourcePath + GIT_ZIP_DIRECTORY_NAME);
                Directory.Delete(questorLauncherSourcePath + GIT_ZIP_DIRECTORY_NAME, true);
            }

            ZipFile.ExtractToDirectory(qLZipFile, questorLauncherSourcePath);

            Console.WriteLine("Done Extracting " + DOWNLOADNAME + " from: " + qLZipFile);
        }

        public static string FindExePath(string exe)
        {
            exe = Environment.ExpandEnvironmentVariables(exe);
            if (!File.Exists(exe))
            {
                if (Path.GetDirectoryName(exe) == string.Empty)
                    foreach (string test in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(';'))
                    {
                        string path = test.Trim();
                        if (!string.IsNullOrEmpty(path) && File.Exists(path = Path.Combine(path, exe)))
                            return Path.GetFullPath(path);
                    }
                throw new FileNotFoundException(new FileNotFoundException().Message, exe);
            }
            return Path.GetFullPath(exe);
        }

        private static string GetMsBuildExePath()
        {
            char s = Path.DirectorySeparatorChar;
            string msb = ProgramFilesX86() + s + "Microsoft Visual Studio" + s + "2017" + s + "Community" + s + "MSBuild" + s + "15.0" + s + "bin" + s + "msbuild.exe";

            if (!File.Exists(msb))
            {
                msb = ProgramFilesX86() + s + "Microsoft Visual Studio" + s + "2017" + s + "BuildTools" + s + "MSBuild" + s + "15.0" + s + "bin" + s + "msbuild.exe";
                if (!File.Exists(msb))
                {
                    msb = ProgramFilesX86() + s + "MSBuild" + s + "15.0" + s + "Bin" + s + "msbuild.exe";
                    if (!File.Exists(msb))
                    {
                        Console.WriteLine("You need to install .NET build tools v15 (https://aka.ms/vs/15/release/vs_buildtools.exe)");
                        Console.WriteLine("Start with following parameters 'vs_buildtools.exe --add Microsoft.VisualStudio.Component.NuGet.BuildTools --add Microsoft.Net.Component.4.5.TargetingPack --add Microsoft.VisualStudio.Workload.MSBuildTools --quiet'");
                        Console.WriteLine("Alternative download: https://c4s.de/build_tools_2017.zip");
                        Console.ReadKey();
                    }
                }
            }

            return msb;
        }

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                DownloadFromGithub();
                CompileAll();
                CopyEveSharpBinaries();
                CopyQuestorBinaries();

                string fileName = AssemblyPath + Path.DirectorySeparatorChar + EXE_FILE_TO_EXECUTE_AFTERWARDS;
                Execute(fileName, "", AssemblyPath, true);
            }

            if (args.Length == 1 && args[0].ToLower().StartsWith("http://github.com") && args[0].ToLower().EndsWith(".zip"))
            {
                DOWNLOAD_LINK = args[0];
                DownloadFromGithub();
                CompileAll();
                CopyEveSharpBinaries();
                CopyQuestorBinaries();

                string fileName = AssemblyPath + Path.DirectorySeparatorChar + EXE_FILE_TO_EXECUTE_AFTERWARDS;
                Execute(fileName, "", AssemblyPath, true);
            }

            if (args.Length == 1 && args[0].Equals("CompileAllAndCopy"))
            {
                CompileAll();
                CopyEveSharpBinaries();
                CopyQuestorBinaries();
            }

            if (args.Length == 1 && args[0].Equals("CopyAll"))
            {
                CopyQuestorBinaries();
                CopyEveSharpBinaries();
            }

            if (args.Length == 3 && args[0].Equals("DownloadEveSharp"))
                DownloadEveSharp(args[1], args[2]);
        }

        private static string ProgramFilesX86()
        {
            if (8 == IntPtr.Size
                || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432")))
                return Environment.GetEnvironmentVariable("ProgramFiles(x86)");

            return Environment.GetEnvironmentVariable("ProgramFiles");
        }

        private static void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Console.WriteLine("Finished Downloading " + DOWNLOADNAME);
            ExtractEveSharp();
            CompileAll();
            CopyEveSharpBinaries();
            CopyQuestorBinaries();

            string fileName = AssemblyPath + Path.DirectorySeparatorChar + EXE_FILE_TO_EXECUTE_AFTERWARDS;
            Execute(fileName, "", AssemblyPath, true);

            working = false;
        }

        private static void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            if (LAST_DOWNLOAD_PROGRESS_REFRESH.AddSeconds(1) < DateTime.UtcNow)
            {
                LAST_DOWNLOAD_PROGRESS_REFRESH = DateTime.UtcNow;
                Console.WriteLine("Download {0}% completed.", percentage.ToString("#.##"));
            }
        }

        private static void DownloadFromGithub()
        {
            string questorLauncherSourceGitPath = AssemblyPath + Path.DirectorySeparatorChar + SOURCE_DIRECTORY + Path.DirectorySeparatorChar +
                                                  GIT_ZIP_DIRECTORY_NAME + Path.DirectorySeparatorChar + ".git";

            if (Directory.Exists(questorLauncherSourceGitPath))
            {
                string gitExec = "Git.exe";
                try
                {
                    string gitExecPath = FindExePath(gitExec);

                    Console.WriteLine("Git directory exists, executing 'git pull' in questor source directory.");

                    DirectoryInfo gitRootPath = Directory.GetParent(questorLauncherSourceGitPath);

                    Process process = new Process();

                    ProcessStartInfo processStartInfo = new ProcessStartInfo();
                    processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    processStartInfo.FileName = gitExecPath;
                    processStartInfo.WorkingDirectory = gitRootPath.ToString();
                    processStartInfo.Arguments = "pull";
                    processStartInfo.RedirectStandardOutput = true;
                    processStartInfo.RedirectStandardError = true;
                    processStartInfo.UseShellExecute = false;
                    process.StartInfo = processStartInfo;
                    process.Start();
                    process.WaitForExit();
                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine(e.ToString());
                    Console.WriteLine("Git executable could not be found. Please install git scm.");
                    Console.WriteLine("Press any key to exit.");
                    Console.ReadLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine("Error. Press any key to exit.");
                    Console.ReadLine();
                }
            }
            else
            {
                Console.WriteLine("Please enter your Github username: ");
                string username = Console.ReadLine();
                Console.WriteLine("Please enter your Github password: ");
                string password = Console.ReadLine();
                DownloadEveSharp(username, password);
            }
        }

        #endregion Methods
    }
}