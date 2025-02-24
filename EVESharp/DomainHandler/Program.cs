/*
 * ---------------------------------------
 * User: duketwo
 * Date: 11.12.2013
 * Time: 12:51
 *
 * ---------------------------------------
 */

using EasyHook;
using SharedComponents.EVE;
using SharedComponents.Utility;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DomainHandler
{
    public class Main : IEntryPoint
    {
        #region Fields

        private readonly EVESharpInterface Interface;

        #endregion Fields

        #region Constructors

        public Main(RemoteHooking.IContext InContext, string ChannelName, string[] args)
        {
            Interface = RemoteHooking.IpcConnectClient<EVESharpInterface>(ChannelName);
            Interface.Ping();
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            HandleException(e.Exception);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException((Exception)e.ExceptionObject);
        }

        private static void HandleException(Exception e)
        {
            Console.WriteLine(e);
            Debug.WriteLine(e);
        }

        private static void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            HandleException(e.Exception);
        }

        #endregion Constructors

        #region Methods

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
        public string GUID;
        public string CharName;

        public string MaskedCharName
        {
            get
            {
                if (!string.IsNullOrEmpty(CharName))
                {
                    return CharName.Substring(0, 4) + "-MaskedCharName-";
                }

                return string.Empty;
            }
        }

        public void Run(RemoteHooking.IContext InContext, string ChannelName, string[] args)
        {
            AppDomain currentDomain = null;
            AppDomain hookManagerDomain = null;
            string assemblyFolder = string.Empty;
            string exeToRun = string.Empty;
            GUID = args[2];
            CharName = args[0];

            try
            {
                CopyHookmanagerForEachUserAtStart();

                currentDomain = AppDomain.CurrentDomain;
                hookManagerDomain = AppDomain.CreateDomain("hookManagerDomain");
                assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                exeToRun = assemblyFolder + "\\HookManager-" + MaskedCharName + "-" + GUID + ".exe";

                Console.WriteLine("[" + Assembly.GetExecutingAssembly().FullName + "] -------------------------------------------------------------------------");
                if (File.Exists(exeToRun))
                {
                    Console.WriteLine("[" + Assembly.GetExecutingAssembly().FullName + "] Launching [" + exeToRun + "]");
                    hookManagerDomain.ExecuteAssembly(exeToRun, args);
                }
                else
                {
                    Console.WriteLine("[" + Assembly.GetExecutingAssembly().FullName + "] File not found: [" + exeToRun + "]");
                    exeToRun = assemblyFolder + "\\HookManager.exe";
                    if (File.Exists(exeToRun))
                    {
                        Console.WriteLine("[" + Assembly.GetExecutingAssembly().FullName + "] Launching [" + exeToRun + "]");
                        hookManagerDomain.ExecuteAssembly(exeToRun, args);
                    }
                }

                Console.WriteLine("[" + Assembly.GetExecutingAssembly().FullName + "] -------------------------------------------------------------------------");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //If HookManager does not run we would have nothing running to spoof our hardware or re-route our traffic: kill the eve process if this happens!
                Util.TaskKill(Process.GetCurrentProcess().Id, false);
                return;
            }


            while (true)
                try
                {
                    Thread.Sleep(200);
                    Interface.Ping();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }


            //Console.WriteLine("[" + Assembly.GetExecutingAssembly().FullName + "] [" + exeToRun + "] terminated.");
            //GC.Collect();
            //GC.WaitForPendingFinalizers();
            //AppDomain.Unload(hookManagerDomain);
            //GC.Collect();
            //GC.WaitForPendingFinalizers();
        }

        private void CopyHookmanagerForEachUserAtStart()
        {
            try
            {
                string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string hookmanagerexe = Path.Combine(path, "Hookmanager.exe");
                string hookmanagerpdb = Path.Combine(path, "Hookmanager.pdb");
                string perToonHookmanagerexe = Path.Combine(path, "Hookmanager-" + MaskedCharName + "-" + GUID + ".exe");
                string perToonHookmanagerpdb = Path.Combine(path, "Hookmanager-" + MaskedCharName + "-" + GUID + ".pdb");

                try
                {
                    File.Copy(hookmanagerpdb, perToonHookmanagerpdb, true);
                    Thread.Sleep(100);
                    while (IsFileLocked(perToonHookmanagerpdb))
                    {
                        Console.WriteLine("HookManager [" + MaskedCharName + "] waiting for [" + perToonHookmanagerpdb + "] to finish copying");
                        Thread.Sleep(300);
                    }

                    Console.WriteLine("HookManager [" + MaskedCharName + "] Done Copying [" + hookmanagerpdb + "] to [" + perToonHookmanagerpdb + "]");
                    Console.WriteLine("HookManager [" + MaskedCharName + "] ----------");

                    File.Copy(hookmanagerexe, perToonHookmanagerexe, true);
                    Thread.Sleep(100);
                    while (IsFileLocked(perToonHookmanagerexe))
                    {
                        Console.WriteLine("DomainHandler [" + MaskedCharName + "] waiting for [" + perToonHookmanagerexe + "] to finish copying");
                        Thread.Sleep(300);
                    }

                    Console.WriteLine("DomainHandler [" + MaskedCharName + "] Done Copying [" + hookmanagerexe + "] to [" + perToonHookmanagerexe + "]");
                    Console.WriteLine("DomainHandler [" + MaskedCharName + "] ----------");
                    Console.WriteLine("DomainHandler [" + MaskedCharName + "] Started Copying [" + hookmanagerpdb + "] to [" + perToonHookmanagerpdb + "]");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("HookManager [" + MaskedCharName + "] Exception [" + ex + "]");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("HookManager: Exception [" + ex + "]");
            }

            return;
        }

        private bool IsFileLocked(string file)
        {
            FileStream stream = null;

            try
            {
                stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        [DllImport("kernel32.dll")]
        private static extern void FreeLibraryAndExitThread(IntPtr hModule, uint dwExitCode);

        #endregion Methods
    }
}