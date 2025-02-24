/*
 * ---------------------------------------
 * User: duketwo
 * Date: 29.12.2013
 * Time: 21:20
 *
 * ---------------------------------------
 */

using EasyHook;
using HookManager.Win32Hooks;
using SharedComponents.IPC;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharedComponents.SharedMemory;
using SharedComponents.WinApiUtil;

namespace HookManager
{
    /// <summary>
    ///     Class with program entry point.
    /// </summary>
    internal sealed class Program
    {
        #region Methods

        public static DateTime TimeStarted { get; set; } = DateTime.UtcNow;
        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            HandleException(e.Exception);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException((Exception) e.ExceptionObject);
        }

        private static void HandleException(Exception e)
        {
            Console.WriteLine(e);
            Debug.WriteLine(e);
        }

        public static SharedComponents.SharedMemory.SharedArray<bool> DisableWinsockConnectionsSharedArray;


        

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern int SetStdHandle(int device, IntPtr handle);

        private static FileStream filestream;
        private static StreamWriter streamwriter;

        static void Redirect()
        {
            int status;
            IntPtr handle;
            filestream = new FileStream("StdLogs.txt", FileMode.Create);
            streamwriter = new StreamWriter(filestream);
            streamwriter.AutoFlush = true;
            Console.SetOut(streamwriter);
            Console.SetError(streamwriter);

            handle = filestream.Handle;
            status = SetStdHandle(-11, handle); // set stdout
            // Check status as needed
            status = SetStdHandle(-12, handle); // set stderr
            // Check status as needed
        }

        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();

        //public static RpcBuffer IPCMaster { get; set; }

        /// <summary>
        ///     Program entry point.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            try
            {
                // args are always length == 2; [proxyId || charName, pipeName]
                if (args.Length != 3)
                {
                    WCFClient.Instance.GetPipeProxy.RemoteLog("if (args.Length != 3) Exiting HookManager!");
                    Environment.Exit(0);
                    Environment.FailFast("");
                }

                // parse args, setup wcf pipe name

                var cp = Process.GetCurrentProcess();
                var cpName = cp.ProcessName.ToLower();
                var cpId = cp.Id;
                HookManagerImpl.Instance.CharName = cpName.ToLower().Equals("firefox") ? string.Empty : args[0];
                HookManagerImpl.Instance.PipeName = args[1];

                DisableWinsockConnectionsSharedArray = new SharedArray<bool>(args[2] + nameof(UsedSharedMemoryNames.DisableAllWinsocketSocketsAndPreventNew), 1);
                DisableWinsockConnectionsSharedArray[0] = false;

                HookManagerImpl.Log("Starting E# HookMananger: Args 0[" + args[0] + "] 1[" + args[1] + "] 2[" + args[2] + "].");
                //    WCFClient.Instance.GetPipeProxy.RemoteLog($"CharacterName cannot be null! Fill it in even for new toons! decide now.");

                TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                var proxyId = cpName.Equals("firefox") ? Convert.ToInt32(args[0]) : 0;
                WCFClient.Instance.CharName = args[0];
                WCFClient.Instance.pipeName = HookManagerImpl.Instance.PipeName;
                WCFClient.Instance.GUID = args[2];
                WCFClient.Instance.GetPipeProxy.Ping();
                WCFClient.Instance.GetPipeProxy.RemoteLog(string.Format("Injected in process [{0}] pid [{1}]", cpName,
                    cpId.ToString()));
                WCFClient.Instance.GetPipeProxy.RemoteLog(string.Format("Args 0[" + args[0] + "] 1[" + args[1] + "] 2[" + args[2] + "]"));


                switch (cpName)
                {
                    case "wget":
                    case "putty":
                    case "notepad":
                        SharedComponents.EVE.Proxy myProxy = WCFClient.Instance.GetPipeProxy.GetProxy(proxyId);
                        if (myProxy == null)
                        {
                            WCFClient.Instance.GetPipeProxy.RemoteLog("if (myProxy == null)");
                        }
                        else
                        {
                            WCFClient.Instance.GetPipeProxy.RemoteLog("Proxy retrieved. Description [" + myProxy.Description + "]");
                            HookManagerImpl.Instance.InitWinSockControllerHookOnly(myProxy);
                            WCFClient.Instance.GetPipeProxy.RemoteLog("WakeUpProcess");
                            RemoteHooking.WakeUpProcess();
                            break;
                        }

                        Environment.Exit(0);
                        Environment.FailFast("");
                        break;

                    case "exefile":
                    default:
                        while (HookManagerImpl.Instance.EveAccount == null)
                        {
                            WCFClient.Instance.GetPipeProxy.RemoteLog("HookManager: if (HookManagerImpl.Instance.EveAccount == null)");
                            Thread.Sleep(2500);
                        }

                        try
                        {
                            Proceed();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Exception [" + ex + "]");
                        }

                        break;
                }

                MainForm form = new MainForm();
                Application.Run(form);

                while (true)
                {
                    if (HookManagerImpl.Instance.EveAccount.RestartOfBotNeeded)
                    {
                        WCFClient.Instance.GetPipeProxy.RemoteLog("HookManager: if (HookManagerImpl.Instance.EveAccount.RestartOfBotNeeded)");
                        WCFClient.Instance.GetPipeProxy.SetEveAccountAttributeValue(WCFClient.Instance.GUID, nameof(HookManagerImpl.Instance.EveAccount.RestartOfBotNeeded), false);
                        //restart questor here
                        MainForm.RestartBot(null);
                    }

                    Thread.Sleep(500);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception [" + ex + "]");
            }
        }

        private static void Proceed()
        {
            try
            {
                WCFClient.Instance.GetPipeProxy.RemoteLog(string.Format("HookManager: EveAccount [{0}] successfully retrieved via WCF [" + WCFClient.Instance.GUID + "].",
                    HookManagerImpl.Instance.EveAccount.AccountName));

                WCFClient.Instance.GetPipeProxy.RemoteLog("HookManager [" + HookManagerImpl.Instance.EveAccount.MaskedCharacterName + "] InitEVEHooks");

                HookManagerImpl.Instance.InitEVEHooks();
                WCFClient.Instance.GetPipeProxy.RemoteLog("HookManager [" + HookManagerImpl.Instance.EveAccount.MaskedCharacterName + "] Waking up the launched process.");
                RemoteHooking.WakeUpProcess();
                WCFClient.Instance.GetPipeProxy.RemoteLog("HookManager [" + HookManagerImpl.Instance.EveAccount.MaskedCharacterName + "] Waiting for eve.");
                HookManagerImpl.Instance.WaitForEVE();
                WCFClient.Instance.GetPipeProxy.RemoteLog("HookManager [" + HookManagerImpl.Instance.EveAccount.MaskedCharacterName + "] Launching app domain inside the target process");
                HookManagerImpl.Instance.LaunchAppDomain();
                WCFClient.Instance.GetPipeProxy.SetEveAccountAttributeValue(WCFClient.Instance.GUID, nameof(HookManagerImpl.Instance.EveAccount.LastSessionReady), DateTime.UtcNow);
                WCFClient.Instance.GetPipeProxy.SetEveAccountAttributeValue(WCFClient.Instance.GUID, nameof(HookManagerImpl.Instance.EveAccount.DoneLaunchingEveInstance), true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("HM Exception:" + ex);
            }
        }

        private static void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            HandleException(e.Exception);
        }

        #endregion Methods
    }
}