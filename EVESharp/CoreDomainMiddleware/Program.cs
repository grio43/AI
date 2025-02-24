using SharedComponents.IPC;
using SharedComponents.Utility;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoreDomainMiddleware
{
    internal static class Program
    {
        #region Methods

        [STAThread]
        private static void Main(string[] args)
        {
            AppDomain coreAppDomain = null;
            string assemblyFolder = string.Empty;
            string exeToRun = string.Empty;

            using (ProcessLock pLock = (ProcessLock) CrossProcessLockFactory.CreateCrossProcessLock(100, Process.GetCurrentProcess().Id.ToString()))
            {
                try
                {
                    Console.WriteLine("CoreDomainMiddleware: Args 0[" + args[0] + "] 1[" + args[1] + "] 2[" + args[2] + "]");
                    coreAppDomain = AppDomain.CreateDomain("CoreDomainMiddleware");
                    assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    exeToRun = assemblyFolder + "\\EVESharpCore-" + args[0] + "-" + args[2] + ".exe";
                    //if (!string.IsNullOrEmpty(args[3]) && args[3].ToLower() == "true".ToLower())
                    //{
                    //    exeToRun = assemblyFolder + "\\EVESharpCore-" + args[0] + "-" + args[2] + "-SISI.exe";
                    //}
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                try
                {
                    //Log32BitOr64Bit();
                    Console.WriteLine("[" + Assembly.GetExecutingAssembly().FullName + "] -------------------------------------------------------------------------");
                    if (File.Exists(exeToRun))
                    {

                        Console.WriteLine("[" + Assembly.GetExecutingAssembly().FullName + "] Launching [" + exeToRun + "] Args 0[" + args[0] + "] 1[" + args[1] + "] 2[" + args[2] + "]");
                        coreAppDomain.ExecuteAssembly(exeToRun, args);
                    }
                    else
                    {
                        Console.WriteLine("[" + Assembly.GetExecutingAssembly().FullName + "] File not found: [" + exeToRun + "]");
                    }

                    Console.WriteLine("[" + Assembly.GetExecutingAssembly().FullName + "] -------------------------------------------------------------------------");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    Console.WriteLine("[" + Assembly.GetExecutingAssembly().FullName + "] [" + exeToRun + "] terminated.");
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    AppDomain.Unload(coreAppDomain);
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }

            Console.WriteLine("[" + Assembly.GetExecutingAssembly().FullName + "] -------------------------------------------------------------------------");
            Console.WriteLine("[" + Assembly.GetExecutingAssembly().FullName + "] [" + exeToRun + "] is terminating and releasing the mutex.");
            Console.WriteLine("[" + Assembly.GetExecutingAssembly().FullName + "] -------------------------------------------------------------------------");
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

        private static void Log32BitOr64Bit()
        {
            if (IntPtr.Size == 4)
            {
                Console.WriteLine("[" + Assembly.GetExecutingAssembly().FullName + "] if (IntPtr.Size == 4): Current Process is 32bit: Environment.Is64BitProcess [" + Environment.Is64BitProcess + "]");
            }
            else if (IntPtr.Size == 8)
            {
                Console.WriteLine("[" + Assembly.GetExecutingAssembly().FullName + "] if (IntPtr.Size == 8): Current Process is 64bit: Environment.Is64BitProcess [" + Environment.Is64BitProcess + "]");

            }
            else
            {
                Console.WriteLine("[" + Assembly.GetExecutingAssembly().FullName + "] Current Process is not 32 or 64bit, what is it?");
            }
        }

        #endregion Methods
    }
}