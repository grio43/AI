/*
 * ---------------------------------------
 * User: duketwo
 * Date: 29.12.2013
 * Time: 21:20
 *
 * ---------------------------------------
 */

using HookManager.Win32Hooks;
using SharedComponents.Controls;
using SharedComponents.EVE;
using SharedComponents.IPC;
using SharedComponents.Py.D3DDetour;
using SharedComponents.Py.PythonBrowser;
using SharedComponents.Utility;
using SharedComponents.WinApiUtil;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HookManager
{
    /// <summary>
    ///     Description of MainForm.
    /// </summary>
    public partial class MainForm : Form
    {
        #region Fields

        private static volatile Thread t;

        #endregion Fields

        #region Constructors

        public MainForm()
        {
            InitializeComponent();
            CurrentProc = Process.GetCurrentProcess().ProcessName.ToLower();

            listViewLogs.ItemActivate += delegate (object sender, EventArgs _args)
            {
                var i = listViewLogs.SelectedIndices[0];
                new ListViewItemForm(Regex.Replace(listViewLogs.Items[i].Text, @"\r\n|\n\r|\n|\r", Environment.NewLine)).Show();
            };

            HookManagerImpl.Instance.AsyncLogQueue.OnMessage += AddLog;
            HookManagerImpl.Instance.AsyncLogQueue.StartWorker();
        }

        #endregion Constructors

        #region Properties

        private static IntPtr CurrentHandle { get; set; }
        private string CurrentProc { get; set; }

        #endregion Properties

        #region Methods

        private void AddLog(string msg, Color? col = null)
        {
            try
            {
                if (CurrentHandle == IntPtr.Zero || !this.IsHandleCreated || !this.listViewLogs.IsHandleCreated)
                    return;

                if (!WinApiUtil.IsValidHWnd(CurrentHandle))
                    return;

                Invoke(new Action(() =>
                {
                    try
                    {
                        col = col ?? Color.White;
                        ListViewItem item = new ListViewItem
                        {
                            Text = msg,
                            ForeColor = (Color)col
                        };

                        if (listViewLogs.Items.Count >= 10000)
                            listViewLogs.Items.Clear();
                        listViewLogs.Items.Add(item);

                        if (listViewLogs.Items.Count > 1)
                            listViewLogs.Items[listViewLogs.Items.Count - 1].EnsureVisible();
                    }
                    catch (Exception){}
                }));
            }
            catch (Exception ex)
            {
                WCFClient.Instance.GetPipeProxy.RemoteLog(ex.ToString());
            }
        }

        private void Button1Click(object sender, EventArgs e)
        {
            Button button = (Button) sender;
            RestartBot(button);
        }

        private void Button2Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure? This should be only used on throw-away accounts.", "Launch pyBrowser", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                PythonBrowserFrm frm = new PythonBrowserFrm();
                frm.Show();
            }
            else if (dialogResult == DialogResult.No)
            {
            }
        }

        private void Button3Click(object sender, EventArgs e)
        {
            Button button = (Button) sender;
            RestartBot(button, true);
        }

        private void Button4Click(object sender, EventArgs e)
        {
            button4.Enabled = false;
            Visible = false;

            if (!WinApiUtil.IsValidHWnd(HookManagerImpl.Instance.EveHWnd))
                return;

            if (Width == 820)
            {
                Width = 20;
                button4.Location = new Point(6, 135);

                while (Width != 20)
                {
                    Application.DoEvents();
                    Thread.Sleep(1);
                }
            }
            else
            {
                Width = 820;
                button4.Location = new Point(806, 135);
                while (Width != 820)
                {
                    Application.DoEvents();
                    Thread.Sleep(1);
                }
            }

            WinApiUtil.DockToParentWindow(WinApiUtil.GetWindowRect(HookManagerImpl.Instance.EveHWnd), HookManagerImpl.Instance.EveHWnd, Handle,
                Alignment.RIGHTBOT);

            button4.Enabled = true;
            Visible = true;
        }

        private void StealthTestClick(object sender, EventArgs e)
        {
            //EnvVars.PrintEnvVars();
            StealthTest.Test();
        }

        private void PrintEnvVarsClick(object sender, EventArgs e)
        {
            EnvVars.PrintEnvVars();
            //StealthTest.Test();
        }

        private static void DockQuestorWindowToParent()
        {
            if (!WinApiUtil.IsValidHWnd(HookManagerImpl.Instance.EVESharpCoreFormHWnd))
                return;
            WinApiUtil.DockToParentWindow(WinApiUtil.GetWindowRect(HookManagerImpl.Instance.EveHWnd), HookManagerImpl.Instance.EveHWnd,
                HookManagerImpl.Instance.EVESharpCoreFormHWnd, Alignment.TOP);
        }

        private void DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void MainFormFormClosing(object sender, FormClosingEventArgs e)
        {
            Hooks.DisposeHooks();
        }

        private void MainFormLoad(object sender, EventArgs e)
        {
        }

        private void MainFormShown(object sender, EventArgs e)
        {
            try
            {
                CurrentProc = Process.GetCurrentProcess().ProcessName;
                if (!CurrentProc.Equals("exefile")) // hide HM if injected into other process
                {
                    Hide();
                    Visible = false;
                    return;
                }

                try
                {
                    listViewLogs.OwnerDraw = true;
                    listViewLogs.DrawColumnHeader += DrawColumnHeader;
                    FormUtil.Color = listViewLogs.BackColor;
                    FormUtil.Font = listViewLogs.Font;
                    listViewLogs.DrawItem += FormUtil.DrawItem;
                    listViewLogs.AutoArrange = false;
                }
                catch (Exception exception)
                {
                    WCFClient.Instance.GetPipeProxy.RemoteLog(exception.ToString());
                }

                CurrentHandle = Handle;
                WCFClient.Instance.GetPipeProxy.SetEveAccountAttributeValue(WCFClient.Instance.GUID, nameof(EveAccount.HookmanagerHWnd),
                    (Int64)CurrentHandle);

                Width = 20;
                DateTime timeout = DateTime.UtcNow.AddSeconds(60);
                while (!WinApiUtil.IsValidHWnd((IntPtr)HookManagerImpl.Instance.EVESharpCoreFormHWnd))
                {
                    Thread.Sleep(500);
                    Application.DoEvents();
                    if (DateTime.UtcNow > timeout)
                    {
                        HookManagerImpl.Log("EVESharpCore window error!", Color.Purple);
                        Environment.Exit(0);
                        Environment.FailFast("");
                    }
                }

                IntPtr eveHwnd = WinApiUtil.GetEveHWnd(Process.GetCurrentProcess().Id);

                if (eveHwnd == IntPtr.Zero)
                {
                    HookManagerImpl.Log("Eve window error!", Color.Purple);
                    Environment.Exit(0);
                    Environment.FailFast("");
                    return;
                }

                RECT eveWindowRect = WinApiUtil.GetWindowRect(eveHwnd);
                int eveWindowThreadId = Pinvokes.GetWindowThreadProcessId(eveHwnd, IntPtr.Zero);

                WCFClient.Instance.GetPipeProxy.SetEveAccountAttributeValue(WCFClient.Instance.GUID, nameof(EveAccount.EveHWnd), (Int64)eveHwnd);

                //WCFClient.Instance.GetPipeProxy.SetEveAccountAttributeValue(WCFClient.Instance.GUID, nameof(EveAccount.LastQuestorSessionReady),
                //    DateTime.UtcNow);

                HookManagerImpl.Log(
                    $"EveHWnd :" + eveHwnd + " HookmanagerHWnd: " + CurrentHandle + " EVESharpCoreFormHWnd: " + HookManagerImpl.Instance.EVESharpCoreFormHWnd,
                    Color.Purple);

                if (!WinApiUtil.IsValidHWnd(HookManagerImpl.Instance.EVESharpCoreFormHWnd)
                    || !WinApiUtil.IsValidHWnd(CurrentHandle)
                    || !WinApiUtil.IsValidHWnd(eveHwnd))
                {
                    HookManagerImpl.Log("Window error!", Color.Purple);
                    Environment.Exit(0);
                    Environment.FailFast("");
                    return;
                }

                WinApiUtil.SetWindowTopMost(CurrentHandle, true);
                WinApiUtil.SetWindowTopMost(HookManagerImpl.Instance.EVESharpCoreFormHWnd, true);
                WinApiUtil.SetWindowTopMost(CurrentHandle, false);
                WinApiUtil.SetWindowTopMost(HookManagerImpl.Instance.EVESharpCoreFormHWnd, false);
                WinApiUtil.SetHWndInsertAfter(CurrentHandle, HookManagerImpl.Instance.EVESharpCoreFormHWnd);
                WinApiUtil.SetHWndInsertAfter(eveHwnd, CurrentHandle);
                WinApiUtil.DockToParentWindow(eveWindowRect, eveHwnd, HookManagerImpl.Instance.EVESharpCoreFormHWnd, Alignment.TOP);
                WinApiUtil.DockToParentWindow(eveWindowRect, eveHwnd, CurrentHandle, Alignment.RIGHTBOT);
                HookManagerImpl.Instance.SetupWindowHooks(eveWindowThreadId, eveHwnd, CurrentHandle);

                if (!HookManagerImpl.Instance.EveAccount.Console) HookManagerImpl.Instance.EveAccount.HideConsoleWindow();

                new Thread(() =>
                {
                    try
                    {
                        if (WCFClient.Instance.GetPipeProxy.IsMainFormMinimized()
                        || HookManagerImpl.Instance.EveAccount.Hidden)
                            HookManagerImpl.Instance.EveAccount.HideWindows();

                        if (!HookManagerImpl.Instance.EveAccount.Console) HookManagerImpl.Instance.EveAccount.HideConsoleWindow();
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }
                }).Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Console.WriteLine(ex);
            }
        }

        public static void RestartBot(Button from, bool compile = false)
        {
            try
            {
                if (t != null)
                    return;

                new Thread(() =>
                {
                    if (from != null)
                        from.Invoke(new Action(() => from.Enabled = false));

                    WCFClient.Instance.GetPipeProxy.RemoteLog("HookManager: RestartBot");
                    HookManagerImpl.Instance.CloseQuestorWindow();
                    try
                    {
                        using (ProcessLock pLock = (ProcessLock)CrossProcessLockFactory.CreateCrossProcessLock(8000, Process.GetCurrentProcess().Id.ToString()))
                        {
                            Console.WriteLine("Got the mutex.. unloading AppDomain.");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    HookManagerImpl.Instance.UnloadAppDomain();
                    if (compile)
                    {
                        HookManagerImpl.Log("[STARTING_COMPILING]");
                        WCFClient.Instance.GetPipeProxy.CompileEveSharpCore(WCFClient.Instance.GUID);
                        HookManagerImpl.Log("[FINISHED_COMPILING]");
                    }

                    HookManagerImpl.Instance.LaunchAppDomain();
                    t = new Thread(() =>
                    {
                        try
                        {
                            DateTime timeout = DateTime.UtcNow.AddSeconds(5);
                            while (!WinApiUtil.IsValidHWnd(HookManagerImpl.Instance.EVESharpCoreFormHWnd) && timeout > DateTime.UtcNow)
                                Thread.Sleep(10);
                            DockQuestorWindowToParent();

                            WinApiUtil.SetWindowTopMost(CurrentHandle, true);
                            WinApiUtil.SetWindowTopMost(HookManagerImpl.Instance.EVESharpCoreFormHWnd, true);
                            WinApiUtil.SetWindowTopMost(CurrentHandle, false);
                            WinApiUtil.SetWindowTopMost(HookManagerImpl.Instance.EVESharpCoreFormHWnd, false);
                            WinApiUtil.SetHWndInsertAfter(CurrentHandle, HookManagerImpl.Instance.EVESharpCoreFormHWnd);
                            WinApiUtil.SetHWndInsertAfter(HookManagerImpl.Instance.EveHWnd, CurrentHandle);

                            t = null;
                            if (from != null)
                                from.Invoke(new Action(() => from.Enabled = true));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    });

                    t.Start();
                }).Start();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }
        }

        #endregion Methods

        private void button6_Click(object sender, EventArgs e)
        {
            new PacketViewerForm().Show();
        }
    }
}