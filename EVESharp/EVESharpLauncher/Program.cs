/*
 * Created by SharpDevelop.
 * User: dserver
 * Date: 02.12.2013
 * Time: 09:09
 *
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using SharedComponents.EVE;
using SharedComponents.Utility;
using System;
using System.Net;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using SharedComponents.SharedMemory;
using System.Threading.Tasks;

namespace EVESharpLauncher
{
    /// <summary>
    ///     Class with program entry point.
    /// </summary>
    internal sealed class Program
    {
        #region Constructors

        /// <summary>
        ///     Program entry point.
        /// </summary>
        static Program()
        {
            Debug.WriteLine("Init");
        }

        #endregion Constructors

        #region Methods

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

        [STAThread]
        private static void Main(string[] args)
        {
            Cache.IsServer = true;
            Task.Run(() =>
            {
                try
                {
                    LauncherHash.GetLauncherHash();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            });

            string path = Util.AssemblyPath.Replace(@"\", string.Empty).Replace(@"/", string.Empty);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.ThreadException += Application_ThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            // add in tls2 and tls3 as options as they are not included as the default, and are not the system default for some dumbass reason. TLS 1.3 is not a recognised enum in this version of .net, so use the hex code.
            //ServicePointManager.SecurityProtocol |= (SecurityProtocolType.Tls12 | (SecurityProtocolType)0x00003000);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;
            // remove SSL3, TLS1.0 and TLS 1.1 as options
            //ServicePointManager.SecurityProtocol &= ~(SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11);

            // Allow a 100 continuation
            //ServicePointManager.Expect100Continue = true;

            try
            {
                using (ProcessLock pLock = (ProcessLock)CrossProcessLockFactory.CreateCrossProcessLock(100, path))
                {
                    //todo: fixme
                    //if (Cache.Instance.EveSettings.SharpLogLite)
                        //SharpLogLiteHandler.Instance.StartListening();

                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    try
                    {
                        Application.Run(new MainForm());
                        Thread EveSharpLauncherMainThread = Thread.CurrentThread;
                        EveSharpLauncherMainThread.Priority = ThreadPriority.Lowest;
                    }
                    catch (ThreadAbortException)
                    {
                        Cache.Instance.Log("ThreadAbortException");
                    }
                    catch (Exception ex)
                    {
                        Cache.Instance.Log("Exception: " + ex);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is CrossProcessLockFactoryMutexException)
                    MessageBox.Show("EVESharpLauncher.exe is already running from path: [" + Util.AssemblyPath + "EVESharpLauncher.exe]", "Another Instance Already Running!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    Cache.Instance.Log("Exception: " + ex);
            }
        }

        #endregion Methods
    }
}