
using SharedComponents.EVE;
using SharedComponents.BrowserAutomation;
using SharedComponents.Events;
using SharedComponents.Extensions;
using SharedComponents.IPC;
using SharedComponents.Notifcations;
using SharedComponents.SharpLogLite.Model;
using SharedComponents.Utility;
using SharedComponents.WinApiUtil;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EVESharpLauncher
{
    public partial class MainForm : Form
    {
        #region Constructors

        public MainForm()
        {
            try
            {
                InitializeComponent();
                EvenTabs = new Dictionary<string, MainFormEventTab>();
                LogTab = new MainFormEventTab("Log","", true);
                tabControl1.TabPages.Add(LogTab);
                Cache.AsyncLogQueue.OnMessage += Log;
                DirectEventHandler.OnDirectEvent += FireOnNewDirectEvent;
                //SharpLogLiteHandler.Instance.OnSharpLogLiteMessage += InstanceOnSharpLogLiteMessage;
                Cache.Instance.Log("EveSharpLauncher: Started");
                //thumbPreviewTab = new ThumbPreviewTab(this, mainTabCtrl);
                EveManager.Instance.DisposeEveManager();
                Thread.Sleep(100);
                EveManager.Instance.StartEveManagerDecide();
                EveManager.Instance.StartSettingsManager();
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
            }
        }

        #endregion Constructors

        #region Classes

        public static class DrawingControl
        {
            #region Fields

            private const int WM_SETREDRAW = 11;

            #endregion Fields

            #region Methods

            /// <summary>
            ///     Resume drawing updates for the specified control.
            /// </summary>
            /// <param name="control">The control to resume draw updates on.</param>
            public static void ResumeDrawing(Control control)
            {
                SendMessage(control.Handle, WM_SETREDRAW, true, 0);
                control.Refresh();
            }

            [DllImport("user32.dll")]
            public static extern int SendMessage(IntPtr hWnd, int wMsg, bool wParam, int lParam);

            /// <summary>
            ///     Some controls, such as the DataGridView, do not allow setting the DoubleBuffered property.
            ///     It is set as a protected property. This method is a work-around to allow setting it.
            ///     Call this in the constructor just after InitializeComponent().
            /// </summary>
            /// <param name="control">The Control on which to set DoubleBuffered to true.</param>
            public static void SetDoubleBuffered(Control control)
            {
                // if not remote desktop session then enable double-buffering optimization
                if (!SystemInformation.TerminalServerSession)
                    typeof(Control).InvokeMember("DoubleBuffered",
                        BindingFlags.SetProperty |
                        BindingFlags.Instance |
                        BindingFlags.NonPublic,
                        null,
                        control,
                        new object[] {true});
            }

            /// <summary>
            ///     Suspend drawing updates for the specified control. After the control has been updated
            ///     call DrawingControl.ResumeDrawing(Control control).
            /// </summary>
            /// <param name="control">The control to suspend draw updates on.</param>
            public static void SuspendDrawing(Control control)
            {
                SendMessage(control.Handle, WM_SETREDRAW, false, 0);
            }

            #endregion Methods
        }

        #endregion Classes

        #region Fields

        private static readonly Color backColor = Color.FromArgb(0, 113, 188);
        //private static SemaphoreSlim sema = new SemaphoreSlim(1, 1);
        //private ClientSettingForm clientSettingForm;

        private ThumbPreviewTab thumbPreviewTab;

        #endregion Fields

        #region Properties

        private IntPtr CurrentHandle { get; set; }
        private Dictionary<string, MainFormEventTab> EvenTabs { get; }
        private MainFormEventTab LogTab { get; }

        #endregion Properties

        #region Methods

        public void DisableDrawDatagrid()
        {
            Pinvokes.SendMessage(dataGridEveAccounts.Handle, Pinvokes.WM_SETREDRAW, false, 0);
        }

        public void EnableDrawDatagrid()
        {
            Pinvokes.SendMessage(dataGridEveAccounts.Handle, Pinvokes.WM_SETREDRAW, true, 0);
            dataGridEveAccounts.Refresh();
        }

        public void Log(string msg, Color? col = null)
        {
            if (!IsHandleCreated)
                return;
            try
            {
                LogTab.Invoke(new Action(() => { LogTab.AddRawMessage(msg); }));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public void FireOnNewDirectEvent(string charName, string GUID, DirectEvent directEvent)
        {
            try
            {
                Invoke(new Action(() =>
                {
                    Email.onNewDirectEvent(charName, GUID, directEvent);
                    GetEventTab(charName, GUID).AddNewEvent(directEvent);
                }));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            foreach (EveAccount eA in Cache.Instance.EveAccountSerializeableSortableBindingList.List)
                if (eA.GetClientCallback() != null)
                {
                    Debug.WriteLine(eA.MaskedCharacterName + " callback available, calling...");
                    eA.GetClientCallback().OnCallback();
                }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            new Thread(() =>
            {
                try
                {
                    Cache.Instance.EveAccountSerializeableSortableBindingList.List.Where(k => k.GetClientCallback() != null)
                        .ToList()
                        .ForEach(k =>
                        {
                            Thread.Sleep(1);
                            try
                            {
                                k.GetClientCallback().GotoHomebaseAndIdle();
                            }
                            catch
                            {
                                //ignore this exception
                            }
                        });
                    //EveManager.Instance.DisposeEveManager();
                }
                catch (Exception ex)
                {
                    Log("Exception: " + ex);
                }
            }).Start();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            new Thread(() =>
            {
                try
                {
                    Cache.Instance.EveAccountSerializeableSortableBindingList.List.Where(k => k.GetClientCallback() != null)
                        .ToList()
                        .ForEach(k =>
                        {
                            Thread.Sleep(1);
                            try
                            {
                                k.GetClientCallback().PauseAfterNextDock();
                            }
                            catch
                            {
                                //ignore this exception
                            }
                        });
                    //EveManager.Instance.DisposeEveManager();
                }
                catch (Exception ex)
                {
                    Log("Exception: " + ex);
                }
            }).Start();
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            new Thread(() =>
            {
                try
                {
                    Cache.Instance.EveAccountSerializeableSortableBindingList.List.Where(k => k.GetClientCallback() != null)
                        .ToList()
                        .ForEach(k =>
                        {
                            Thread.Sleep(1);
                            try
                            {
                                k.GetClientCallback().GotoJita();
                            }
                            catch
                            {
                                //ignore this exception
                            }
                        });
                    //EveManager.Instance.DisposeEveManager();
                }
                catch (Exception ex)
                {
                    Log("Exception: " + ex);
                }
            }).Start();
        }

        private void Button6_Click(object sender, EventArgs e)
        {
            Cache.Instance.EveAccountSerializeableSortableBindingList.List.ToList().ForEach(k => k.ClearCache());
        }

        private void ButtonbuttonKillAllEveInstancesNowClick(object sender, EventArgs e)
        {
            EveManager.Instance.KillEveInstances();
        }

        private void ButtonCheckHWProfiles_Click(object sender, EventArgs e)
        {
            Cache.Instance.AnyAccountsLinked(true);
        }

        private void ButtonGenNewBeginEndClick(object sender, EventArgs e)
        {
            foreach (EveAccount eA in Cache.Instance.EveAccountSerializeableSortableBindingList.List)
                eA.GenerateNewTimeSpans();
        }

        private void ButtonKillAllEveInstancesClick(object sender, EventArgs e)
        {
            EveManager.Instance.KillEveInstancesDelayed();
        }

        private void ButtonStartEveMangerClick(object sender, EventArgs e)
        {
            EveManager.Instance.StartEveManagerDecide();
            EveManager.Instance.StartSettingsManager();
        }

        private void ButtonStartUpdateLeaderAndSlaveStaticInfoClick(object sender, EventArgs e)
        {
            EveManager.NextUpdateLeaderAndSlaveStaticInfo = DateTime.UtcNow.AddMinutes(-1);
            EveManager.UpdateLeaderAndSlaveStaticInfo(true);
        }

        private void ButtonStopEveMangerClick(object sender, EventArgs e)
        {
            EveManager.Instance.DisposeEveManager();
        }

        private void TestImapEmailToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataGridView dgv = ActiveControl as DataGridView;
            if (dgv == null) return;
            int index = dgv.SelectedCells[0].OwningRow.Index;
            EveAccount eA = Cache.Instance.EveAccountSerializeableSortableBindingList.List[index];
            //eA.TestImapEmail();
        }

        private void ClearCacheToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataGridView dgv = ActiveControl as DataGridView;
            if (dgv == null) return;
            int index = dgv.SelectedCells[0].OwningRow.Index;
            EveAccount eA = Cache.Instance.EveAccountSerializeableSortableBindingList.List[index];
            eA.ClearCache();
        }

        private void CompileEveSharpCoreToolStripMenuItemClick(object sender, EventArgs e)
        {
            Util.RunInDirectory("Updater.exe", "CompileAllAndCopy");
        }

        private void ContextMenuStrip1Opening(object sender, CancelEventArgs e)
        {
            try
            {
                DataGridView dgv = ActiveControl as DataGridView;
                if (dgv == null) return;
                int index = dgv.SelectedCells[0].OwningRow.Index;
                EveAccount eA = Cache.Instance.EveAccountSerializeableSortableBindingList.List[index];
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.ToString());
            }
        }

        private void DataGridEveAccounts_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            Task.Run(() =>
            {
                dataGridEveAccounts.Invoke(new Action(() =>
                {
                    dataGridEveAccounts.ColumnHeadersDefaultCellStyle.BackColor = backColor; // #0071BC
                    dataGridEveAccounts.BackColor = backColor;
                }));
                Cache.Instance.EveAccountSerializeableSortableBindingList.List.RaiseListChangedEvents = false;
            });
            dataGridEveAccounts.SuspendLayout();
        }

        private void DataGridEveAccounts_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    dataGridEveAccounts.Invoke(new Action(() => { dataGridEveAccounts.ColumnHeadersDefaultCellStyle.BackColor = SystemColors.Control; }));
                    Cache.Instance.EveAccountSerializeableSortableBindingList.List.RaiseListChangedEvents = true;
                }
                catch (Exception)
                {
                    //ignore this exception
                }
            });
            dataGridEveAccounts.ResumeLayout();
        }

        private void DataGridEveAccounts_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                if (dataGridEveAccounts.CurrentCell != null && dataGridEveAccounts.CurrentCell.IsInEditMode)
                {
                    int cIndex = dataGridEveAccounts.CurrentCell.ColumnIndex;
                    int rIndex = dataGridEveAccounts.CurrentCell.RowIndex;

                    if (e.RowIndex == rIndex && e.ColumnIndex == cIndex)
                        return;
                }

                string name = dataGridEveAccounts.Columns[e.ColumnIndex].Name;
                if (name.Equals("Password") && e.Value != null)
                {
                    dataGridEveAccounts.Rows[e.RowIndex].Tag = e.Value;
                    e.Value = new string('\u25CF', e.Value.ToString().Length);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void DataGridEveAccounts_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                DataGridView dgv = (DataGridView) sender;
                DataGridViewCell c = dataGridEveAccounts.SelectedCells[0];
                int index = c.OwningRow.Index;
                if (c.OwningColumn.Name == "Controller")
                {
                    EveAccount eA = Cache.Instance.EveAccountSerializeableSortableBindingList.List[index];
                    eA.SelectedController = (string) c.Value;
                    Cache.Instance.Log("[" + eA.MaskedCharacterName + "] Selected Controller is now [" + eA.SelectedController + "] for GUID [" + eA.GUID + "]");
                }
            }
            catch
            {
                //ignore this exception
            }
        }

        private void DataGridEveAccounts_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            // Don't throw an exception when we're done.
            e.ThrowException = false;

            //extra : more customized to replace the
            // system error text & in any language
            string exType = e.Exception.GetType().ToString();

            // Display an error message.
            string txt = "Error with [][ " + dataGridEveAccounts.Columns[e.ColumnIndex].HeaderText + " ] \n\n" + e.Exception.Message;
            if (exType == "System.FormatException")
                Cache.Instance.Log($"Datagrid error: {txt}");
            // If this is true, then the user is trapped in this cell.
            e.Cancel = false;
        }

        private void DataGridEveAccounts_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
        }

        private void DataGridEveAccounts_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            using (SolidBrush b = new SolidBrush(dataGridEveAccounts.RowHeadersDefaultCellStyle.ForeColor))
            {
                e.Graphics.DrawString((e.RowIndex + 1).ToString(), e.InheritedRowStyle.Font, b, e.RowBounds.Location.X + 14, e.RowBounds.Location.Y + 4);
            }
        }

        private void DataGridEveAccounts_SelectionChanged(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    const int count = 12;
                    TableLayoutPanel eventTableLayoutPanel = new TableLayoutPanel
                    {
                        Location = new Point(3, 3),
                        RowCount = count,
                        ColumnCount = 1,
                        Dock = DockStyle.Fill
                    };

                    eventTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                    Dictionary<int, Label> labelDict = new Dictionary<int, Label>();

                    for (int i = 0; i < count; i++)
                    {
                        eventTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100 / count));
                        Label l1 = new Label();
                        labelDict[i] = l1;
                        l1.Dock = DockStyle.Fill;
                        eventTableLayoutPanel.Controls.Add(l1);
                    }

                    tabPage3.Controls.Clear();
                    tabPage3.Controls.Add(eventTableLayoutPanel);

                    if (!(ActiveControl is DataGridView dgv)) return;
                    if (dgv.SelectedCells.Count > 0)
                    {
                        int index = dgv.SelectedCells[0].OwningRow.Index;
                        if (Cache.Instance.EveAccountSerializeableSortableBindingList.List.ElementAtOrDefault(index) != null)
                        {
                            EveAccount eA = Cache.Instance.EveAccountSerializeableSortableBindingList.List[index];
                            if (eA != null && eA.CharsOnAccount != null)
                                labelDict[0].Text = $"Other chars: {string.Join(", ", eA.CharsOnAccount.Where(h => !h.Equals(eA.CharacterName)))}";
                            labelDict[1].Text = $"Last ammo buy: {eA.LastAmmoBuy}";
                            labelDict[2].Text = $"Last plex buy: {eA.LastPlexBuy}";
                            labelDict[3].Text = $"Last start time: {eA.LastEveClientLaunched}";
                            labelDict[4].Text = $"Next cache deletion: {eA.NextCacheDeletion}";
                            labelDict[5].Text = $"Process Id: {eA.Pid}";
                            labelDict[6].Text = $"Ram usage: {Math.Round(eA.RamUsage, 1)} mb";
                            labelDict[7].Text = $"Run time today: {Math.Round(eA.StartingTokenTimespan.TotalHours, 2)} h";
                            DateTime skillQueueEnd = eA.MySkillQueueEnds > DateTime.UtcNow ? eA.MySkillQueueEnds : DateTime.UtcNow;
                            labelDict[8].Text = $"Skill queue end: {Math.Round((skillQueueEnd - DateTime.UtcNow).TotalDays, 2)} days";
                            labelDict[9].Text = $"Dump loot iterations: {eA.DumpLootIterations}";
                        }
                    }

                    //tabControl3.Invoke(new Action(() =>
                    //{
                    //    tabPage3.Controls.Clear();
                    //    tabPage3.SuspendLayout();
                    //    tabPage3.Controls.Add(eventTableLayoutPanel);
                    //    tabPage3.ResumeLayout();
                    //}));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            });
        }

        private void DeleteToolStripMenuItem1Click(object sender, EventArgs e)
        {
            if (!(ActiveControl is DataGridView dgv)) return;
            int selected = dgv.SelectedCells[0].OwningRow.Index;

            if (selected >= 0)
                Cache.Instance.EveAccountSerializeableSortableBindingList.List.RemoveAt(selected);
        }

        private void EditAdapteveHWProfileToolStripMenuItemClick(object sender, EventArgs e)
        {
            DataGridView dgv = ActiveControl as DataGridView;
            if (dgv == null) return;
            int index = dgv.SelectedCells[0].OwningRow.Index;
            EveAccount eA = Cache.Instance.EveAccountSerializeableSortableBindingList.List[index];

            HWProfileForm hwPf = new HWProfileForm(eA);
            hwPf.Show();
        }

        private void EditClientConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /**
            DataGridView dgv = ActiveControl as DataGridView;
            if (dgv == null) return;
            int index = dgv.SelectedCells[0].OwningRow.Index;
            EveAccount eA = Cache.Instance.EveAccountSerializeableSortableBindingList.List[index];
            if (clientSettingForm != null)
                clientSettingForm.Close();
            clientSettingForm = new ClientSettingForm(eA);
            clientSettingForm.Show();
            **/
        }

        private MainFormEventTab GetEventTab(string charName, string GUID)
        {
            MainFormEventTab tab;
            EvenTabs.TryGetValue(GUID, out tab);
            if (tab == null)
            {
                tab = new MainFormEventTab(charName, GUID);
                EvenTabs[GUID] = tab;
                tabControl1.TabPages.Add(tab);
            }
            return tab;
        }

        private void HideToolStripMenuItemClick(object sender, EventArgs e)
        {
            foreach (EveAccount eA in Cache.Instance.EveAccountSerializeableSortableBindingList.List.Where(a => a.IsProcessAlive()))
                eA.HideWindows();
        }

        private void InstanceOnSharpLogLiteMessage(SharpLogMessage msg)
        {
            try
            {
                string charName;
                EveAccount.CharnameByPid.TryGetValue((int)msg.Pid, out charName);

                string GUID;
                EveAccount.GUIDByPid.TryGetValue((int)msg.Pid, out GUID);

                if (!string.IsNullOrEmpty(charName) && !string.IsNullOrEmpty(GUID))
                    Invoke(new Action(() =>
                    {
                        MainFormEventTab tab = GetEventTab(charName, GUID);
                        tab.AddSharpLogLiteMessage(msg, charName ,GUID);
                    }));
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            CurrentHandle = Handle;
            MainFormResize(this, EventArgs.Empty);
            Cache.Instance.MainFormHWnd = (long) CurrentHandle;
        }

        private List<System.IDisposable> drivers = new List<System.IDisposable>();

        private void MainFormFormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                FirewallRuleHelper.RemoveRule("BlockEVE-EveLauncher.exe");

                foreach (var d in drivers)
                    if (d != null)
                        try
                        {
                            d.Dispose();
                        }
                        catch (Exception exception)
                        {
                            Debug.WriteLine(exception);
                        }

                SharpLogLiteHandler.Instance.Dispose();
                Cache.Instance.EveAccountSerializeableSortableBindingList.List.XmlSerialize(Cache.Instance.EveAccountSerializeableSortableBindingList.FilePathName);
                Cache.Instance.EveSettingsSerializeableSortableBindingList.List.XmlSerialize(
                    Cache.Instance.EveSettingsSerializeableSortableBindingList.FilePathName);
                SharpLogLiteHandler.Instance.OnSharpLogLiteMessage -= InstanceOnSharpLogLiteMessage;
                DirectEventHandler.OnDirectEvent -= FireOnNewDirectEvent;
                EveManager.Instance.Dispose();
                Cache.IsShuttingDown = true;
                Cache.BroadcastShutdown();
            }
            catch (Exception)
            {
                //ignore this exception
            }

            try
            {
                Application.Exit();
            }
            catch (Exception)
            {
                //ignore this exception
            }
        }

        private void MainFormLoad(object sender, EventArgs e)
        {
            try
            {
                DrawingControl.SuspendDrawing(dataGridEveAccounts);
                DrawingControl.SetDoubleBuffered(dataGridEveAccounts);
                dataGridEveAccounts.DataSource = Cache.Instance.EveAccountSerializeableSortableBindingList.List;
                DataGridViewComboBoxColumn cmb = new DataGridViewComboBoxColumn
                {
                    HeaderText = "Controller",
                    Name = "Controller",
                    MaxDropDownItems = EveAccount.ListOfAvailableControllers.Count + 1,
                    DataSource = EveAccount.ListOfAvailableControllers
                };

                dataGridEveAccounts.Columns.Insert(dataGridEveAccounts.Columns["UseScheduler"].Index, cmb);
                dataGridEveAccounts.AllowUserToOrderColumns = true;

                foreach (DataGridViewRow r in dataGridEveAccounts.Rows)
                {
                    r.Resizable = DataGridViewTriState.True;
                    r.Frozen = false;
                    foreach (DataGridViewCell c in r.Cells)
                        if (c.OwningColumn.Name == "Controller")
                        {
                            int index = c.OwningRow.Index;
                            try
                            {
                                EveAccount eA = Cache.Instance.EveAccountSerializeableSortableBindingList.List[index];
                                c.Value = eA.SelectedController;
                            }
                            catch
                            {
                                //ignore this exception
                            }
                        }
                }

                try
                {
                    if (textBoxPastebin != null)
                        textBoxPastebin.Text = Regex.Replace(Cache.Instance.EveSettings.Pastebin, @"\r\n|\n\r|\n|\r", Environment.NewLine);
                }
                catch (Exception)
                {
                    //ignore this exception
                }

                WCFServer.Instance.StartWCFServer();
                WCFClient.Instance.pipeName = Cache.Instance.EveSettings.WCFPipeName;
                foreach (DataGridViewColumn col in dataGridEveAccounts.Columns)
                {
                    int index = col.Index;
                    string name = col.Name;
                    ToolStripMenuItem menuItem = new ToolStripMenuItem
                    {
                        Checked = true
                    };
                    col.Frozen = false;
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    col.Resizable = DataGridViewTriState.True;

                    if (Cache.Instance.EveSettings.DatagridViewHiddenColumns.Contains(index))
                    {
                        menuItem.Checked = false;
                        dataGridEveAccounts.Columns[index].Visible = false;
                    }

                    if (Cache.Instance.EveAccountSerializeableSortableBindingList.List.All(i => !i.SelectedController.Equals(nameof(EveAccount.AvailableControllers.QuestorController)) && !i.SelectedController.Equals(nameof(EveAccount.AvailableControllers.CareerAgentController))))
                    {
                        if (col.Name == "Agent") col.Visible = false;
                        if (col.Name == "AgentLevel") col.Visible = false;
                        if (col.Name == "AgentCorp") col.Visible = false;
                        if (col.Name == "IskPerLp") col.Visible = false;
                        if (col.Name == "InMission") col.Visible = false;
                        if (col.Name == "LoyaltyPoints") col.Visible = false;
                        if (col.Name == "LpValue") col.Visible = false;
                        if (col.Name == "LastBuyLpItemAttempt") col.Visible = false;
                        if (col.Name == "LastMissionName") col.Visible = false;
                        if (col.Name == "MissionStarted") col.Visible = false;
                        if (col.Name == "LastBuyLpItems") col.Visible = false;
                    }

                    if (Cache.Instance.EveAccountSerializeableSortableBindingList.List.All(i => !i.SelectedController.Equals(nameof(EveAccount.AvailableControllers.AbyssalDeadspaceController))))
                    {
                        //if (col.Name == "InAbyssalDeadspace") col.Visible = false;
                        //if (col.Name == "AbyssalPocketNumber") col.Visible = false;
                        //if (col.Name == "NumOfAbyssalSitesBeforeRestarting") col.Visible = false;
                    }

                    if (Cache.Instance.EveAccountSerializeableSortableBindingList.List.All(i => !i.SelectedController.Equals(nameof(EveAccount.AvailableControllers.AbyssalDeadspaceController)) && !i.SelectedController.Equals(nameof(EveAccount.AvailableControllers.QuestorController))))
                    {
                        //if (col.Name == "CMBCtrlAction") col.Visible = false;
                        //if (col.Name == "FleetName") col.Visible = false;
                    }

                    if (Cache.Instance.EveAccountSerializeableSortableBindingList.List.All(i => !i.SelectedController.Equals(nameof(EveAccount.AvailableControllers.HydraController)) && !i.SelectedController.Equals(nameof(EveAccount.AvailableControllers.CombatDontMoveController))))
                    {
                        //if (col.Name == "IsLeader") col.Visible = false;
                        //if (col.Name == "FleetName") col.Visible = false;
                    }

                    if (Cache.Instance.EveAccountSerializeableSortableBindingList.List.All(i => !i.SelectedController.Equals(nameof(EveAccount.AvailableControllers.HydraController))))
                    {
                        //if (col.Name == "BotUsesHydra") col.Visible = false;
                    }

                    menuItem.Click += (o, args) =>
                    {
                        ToolStripMenuItem ts = (ToolStripMenuItem)o;
                        ts.Checked = !ts.Checked;
                        dataGridEveAccounts.Columns[index].Visible = ts.Checked;

                        if (ts.Checked)
                        {
                            if (Cache.Instance.EveSettings.DatagridViewHiddenColumns.Any(k => k == index))
                                Cache.Instance.EveSettings.DatagridViewHiddenColumns.Remove(index);
                        }
                        else
                        {
                            if (!Cache.Instance.EveSettings.DatagridViewHiddenColumns.Any(k => k == index))
                                Cache.Instance.EveSettings.DatagridViewHiddenColumns.Add(index);
                        }
                    };

                    menuItem.Text = name;

                    if (menuItem.Text == "WalletBalance" ||
                        menuItem.Text == "LpValue" ||
                        menuItem.Text == "TotalValue" ||
                        menuItem.Text == "LootValueGatheredToday" ||
                        menuItem.Text == "ItemHangarValue")
                        col.DefaultCellStyle.Format = "c0";

                    if (menuItem.Text == "LoyaltyPoints")
                        col.DefaultCellStyle.Format = "n0";

                    /**
                    if (menuItem.Text == "StartTime" ||
                        menuItem.Text == "EndTime" ||
                        menuItem.Text == "LastAmmoBuy" ||
                        menuItem.Text == "MissionStarted" ||
                        menuItem.Text == "LastInWarp" ||
                        menuItem.Text == "LastSessionChange")
                    {
                        col.DefaultCellStyle.Format = "MM/dd HH:mm";
                    }
                    **/
                    columnsToolStripMenuItem.DropDownItems.Add(menuItem);
                }

                //dataGridEveAccounts.FastAutoSizeColumns();
                dataGridEveAccounts.AllowUserToResizeColumns = true;
                dataGridEveAccounts.AlternatingRowsDefaultCellStyle.BackColor = Color.LightGray;
                dataGridEveAccounts.Anchor = AnchorStyles.None;
                dataGridEveAccounts.Dock = DockStyle.Fill;
                dataGridEveAccounts.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
                dataGridEveAccounts.ColumnHeadersHeight = 50;
                dataGridEveAccounts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader;
                dataGridEveAccounts.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.EnableResizing;
                dataGridEveAccounts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                dataGridEveAccounts.ScrollBars = ScrollBars.Both;

                try
                {
                    dataGridEveAccounts.Columns["Num"].DisplayIndex = 0;
                    dataGridEveAccounts.Columns["AccountName"].DisplayIndex = 1;
                    dataGridEveAccounts.Columns["Password"].DisplayIndex = 2;
                    dataGridEveAccounts.Columns["Controller"].DisplayIndex = 3;
                    dataGridEveAccounts.Columns["UseScheduler"].DisplayIndex = 4;
                    dataGridEveAccounts.Columns["CharacterName"].DisplayIndex = 5;

                    dataGridEveAccounts.Columns["myNote1"].DisplayIndex = 6;
                    dataGridEveAccounts.Columns["myNote2"].DisplayIndex = 7;
                    dataGridEveAccounts.Columns["myNote3"].DisplayIndex = 8;
                    dataGridEveAccounts.Columns["myNote4"].DisplayIndex = 9;
                    dataGridEveAccounts.Columns["StartTime"].DisplayIndex = 10;
                    dataGridEveAccounts.Columns["EndTime"].DisplayIndex = 11;
                    dataGridEveAccounts.Columns["HoursPerDay"].DisplayIndex = 12;
                    dataGridEveAccounts.Columns["StartHour"].DisplayIndex = 13;
                    dataGridEveAccounts.Columns["WalletBalance"].DisplayIndex = 14;
                    dataGridEveAccounts.Columns["ItemHangarValue"].DisplayIndex = 15;
                    dataGridEveAccounts.Columns["UseFleetMgr"].DisplayIndex = 16;

                }
                catch (Exception){}

                //Cache.Instance.Log("MainForm: Sort EveAccounts by the Num Column");

                //dataGridEveAccounts.Sort(dataGridEveAccounts.Columns[0], ListSortDirection.Descending);

                foreach (DataGridViewColumn col in dataGridEveAccounts.Columns)
                {
                    ToolStripMenuItem menuItem = new ToolStripMenuItem
                    {
                        Text = col.Name
                    };

                    if (menuItem.Text == "CharacterName")
                    {
                        col.DividerWidth = 2;
                        col.Frozen = true;
                    }
                }

                dataGridEveAccounts.Select();
                DrawingControl.ResumeDrawing(dataGridEveAccounts);
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
            }

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            Task.Run(() =>
            {
                while (!tokenSource.Token.IsCancellationRequested)
                {
                    tokenSource.Token.WaitHandle.WaitOne(5000);
                    try
                    {
                        dataGridEveAccounts.Invoke(new Action(() => { dataGridEveAccounts.FastAutoSizeColumns(); }));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                    }
                    finally
                    {
                        Thread.Sleep(1);
                    }
                }
            }, tokenSource.Token);
        }

        private void MainFormResize(object sender, EventArgs e)
        {
            try
            {
                Rectangle screenRectangle = RectangleToScreen(ClientRectangle);
                int titleHeight = screenRectangle.Top - Top;
                dataGridEveAccounts.Height = Height - LogTab.Height - titleHeight - menuStrip1.Height;
                //groupBox2.Width = screenRectangle.Right - groupBox2.Left - screenRectangle.Left - 3;

                if (WindowState == FormWindowState.Minimized)
                {
                    Visible = false;
                    notifyIconQL.Visible = true;
                    Cache.Instance.IsMainFormMinimized = true;

                    if (Cache.Instance.EveSettings.ToggleHideShowOnMinimize)
                        foreach (EveAccount eA in Cache.Instance.EveAccountSerializeableSortableBindingList.List) //.Where(i => i.AllowHideEveWindow))
                            eA.HideWindows();
                }
            }
            catch (Exception ex)
            {
                Cache.Instance.Log(ex.ToString());
            }
        }

        private void MainTabCtrl_Selected(object sender, TabControlEventArgs e)
        {
            // pass to...
            thumbPreviewTab.Selected(sender, e);
        }

        private void NotifyIconQL_Click(object sender, EventArgs e)
        {
            ((NotifyIcon) sender).Visible = !((NotifyIcon) sender).Visible;
            Visible = !Visible;
            Cache.Instance.IsMainFormMinimized = false;
            if (Cache.Instance.EveSettings.ToggleHideShowOnMinimize)
            {
                foreach (EveAccount eA in Cache.Instance.EveAccountSerializeableSortableBindingList.List.Where(eA => !eA.Hidden))
                    eA.ShowWindows();
            }

            WindowState = FormWindowState.Normal;
        }

        private void NotifyIconQLMouseDoubleClick(object sender, MouseEventArgs e)
        {
            Cache.Instance.IsMainFormMinimized = false;
            WindowState = FormWindowState.Maximized;
            if (Cache.Instance.EveSettings.ToggleHideShowOnMinimize)
            {
                ((NotifyIcon) sender).Visible = !((NotifyIcon) sender).Visible;
                Visible = !Visible;

                foreach (EveAccount eA in Cache.Instance.EveAccountSerializeableSortableBindingList.List)
                    eA.ShowWindows();
            }

            WindowState = FormWindowState.Normal;
        }

        private void OpenEveAccountCreatorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new EveAccountCreatorForm().Show();
        }

        private void ProxiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new ProxiesForm().Show();
        }

        private void SelectProcessToProxyToolStripMenuItemClick(object sender, EventArgs e)
        {
            DataGridView dgv = ActiveControl as DataGridView;
            if (dgv == null) return;
            int index = dgv.SelectedCells[0].OwningRow.Index;
            EveAccount eA = Cache.Instance.EveAccountSerializeableSortableBindingList.List[index];
            eA.StartExecuteable(string.Empty);
        }

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SettingsForm form = new SettingsForm();
            form.ShowDialog();
        }

        private void ShowRealHardwareInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new RealHardwareInfoForm().Show();
        }

        private void ShowToolStripMenuItemClick(object sender, EventArgs e)
        {
            foreach (EveAccount eA in Cache.Instance.EveAccountSerializeableSortableBindingList.List.Where(a => a.IsProcessAlive()))
                eA.ShowWindows();
        }

        private void OpenEmailUsingProxyItemClick(object sender, EventArgs e)
        {
            Cache.Instance.Log("User Clicked on [ OpenEmailUsingProxy ]");
            if (!(ActiveControl is DataGridView dgv))
            {
                Cache.Instance.Log("if (!(ActiveControl is DataGridView dgv))");
                return;
            }

            int index = dgv.SelectedCells[0].OwningRow.Index;
            EveAccount eA = Cache.Instance.EveAccountSerializeableSortableBindingList.List[index];
            if (string.IsNullOrEmpty(eA.Email))
            {
                Cache.Instance.Log("Email address is blank for account [" + eA.MaskedAccountName + "]");
                return;
            }

            if (string.IsNullOrEmpty(eA.EmailPassword))
            {
                Cache.Instance.Log("Email password is blank for account [" + eA.MaskedAccountName + "]");
                return;
            }

            if (eA.HWSettings.Proxy == null)
            {
                Cache.Instance.Log("Proxy is null. Choose a proxy under Hardware Settings");
                return;
            }

            if (!eA.HWSettings.Proxy.IsValid)
            {
                Cache.Instance.Log("Proxy is inValid");
                return;
            }

            SeleniumAutomation seleniumAutomationImpl = new SeleniumAutomation();
            drivers.Add(seleniumAutomationImpl);
            seleniumAutomationImpl.CreateTaskToOpenEmail(eA.Email, eA.EmailPassword, eA.HWSettings.Proxy);
        }

        private void debugShowAllCookiesItemClick(object sender, EventArgs e)
        {
            Cache.Instance.Log("User Clicked on [ debugShowAllCookies ]");
            if (!(ActiveControl is DataGridView dgv))
            {
                Cache.Instance.Log("if (!(ActiveControl is DataGridView dgv))");
                return;
            }

            int index = dgv.SelectedCells[0].OwningRow.Index;
            EveAccount eA = Cache.Instance.EveAccountSerializeableSortableBindingList.List[index];
            if (string.IsNullOrEmpty(eA.AccountName))
            {
                Cache.Instance.Log("AccountName is blank for account Num [" + eA.Num + "]");
                return;
            }

            if (string.IsNullOrEmpty(eA.Password))
            {
                Cache.Instance.Log("Password is blank for account [" + eA.MaskedAccountName + "]");
                return;
            }

            if (eA.HWSettings.Proxy == null)
            {
                Cache.Instance.Log("Proxy is null. Choose a proxy under Hardware Settings");
                return;
            }

            if (!eA.HWSettings.Proxy.IsValid)
            {
                Cache.Instance.Log("Proxy is inValid");
                return;
            }

            eA.ShowAllISBELCookies();
        }

        private void editPatternManagerSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(ActiveControl is DataGridView dgv)) return;
            var index = dgv.SelectedCells[0].OwningRow.Index;
            var eA = Cache.Instance.EveAccountSerializeableSortableBindingList.List[index];

            var frm = new PatternManagerForm(eA);
            frm.Show();
        }

        private void clearEveAccessTokensItemClick(object sender, EventArgs e)
        {
            try
            {
                Cache.Instance.Log("User Clicked on [ clearEveAccessTokens ]");
                if (!(ActiveControl is DataGridView dgv))
                {
                    Cache.Instance.Log("if (!(ActiveControl is DataGridView dgv))");
                    return;
                }

                int index = dgv.SelectedCells[0].OwningRow.Index;
                EveAccount eA = Cache.Instance.EveAccountSerializeableSortableBindingList.List[index];
                if (string.IsNullOrEmpty(eA.AccountName))
                {
                    Cache.Instance.Log("AccountName is blank for account Num [" + eA.Num + "]");
                    return;
                }

                if (string.IsNullOrEmpty(eA.Password))
                {
                    Cache.Instance.Log("Password is blank for account [" + eA.MaskedAccountName + "]");
                    return;
                }

                if (eA.HWSettings.Proxy == null)
                {
                    Cache.Instance.Log("Proxy is null. Choose a proxy under Hardware Settings");
                    return;
                }

                if (!eA.HWSettings.Proxy.IsValid)
                {
                    Cache.Instance.Log("Proxy is inValid");
                    return;
                }

                eA.ClearEveAccessToken();
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
            }
        }

        private void clearRefreshTokensItemClick(object sender, EventArgs e)
        {
            try
            {
                Cache.Instance.Log("User Clicked on [ cleaRefreshTokens ]");
                if (!(ActiveControl is DataGridView dgv))
                {
                    Cache.Instance.Log("if (!(ActiveControl is DataGridView dgv))");
                    return;
                }

                int index = dgv.SelectedCells[0].OwningRow.Index;
                EveAccount eA = Cache.Instance.EveAccountSerializeableSortableBindingList.List[index];
                if (string.IsNullOrEmpty(eA.AccountName))
                {
                    Cache.Instance.Log("AccountName is blank for account Num [" + eA.Num + "]");
                    return;
                }

                if (string.IsNullOrEmpty(eA.Password))
                {
                    Cache.Instance.Log("Password is blank for account [" + eA.MaskedAccountName + "]");
                    return;
                }

                if (eA.HWSettings.Proxy == null)
                {
                    Cache.Instance.Log("Proxy is null. Choose a proxy under Hardware Settings");
                    return;
                }

                if (!eA.HWSettings.Proxy.IsValid)
                {
                    Cache.Instance.Log("Proxy is inValid");
                    return;
                }

                eA.ClearRefreshToken();
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
            }
        }

        private void OpenEveSSOWebsiteUsingProxyItemClick(object sender, EventArgs e)
        {
            Cache.Instance.Log("User Clicked on [ OpenEveSSOUsingProxy ]");
            if (!(ActiveControl is DataGridView dgv))
            {
                Cache.Instance.Log("if (!(ActiveControl is DataGridView dgv))");
                return;
            }

            int index = dgv.SelectedCells[0].OwningRow.Index;
            EveAccount eA = Cache.Instance.EveAccountSerializeableSortableBindingList.List[index];
            if (string.IsNullOrEmpty(eA.AccountName))
            {
                Cache.Instance.Log("AccountName is blank for account Num [" + eA.Num + "]");
                return;
            }

            if (string.IsNullOrEmpty(eA.Password))
            {
                Cache.Instance.Log("Password is blank for account [" + eA.MaskedAccountName + "]");
                return;
            }

            if (eA.HWSettings.Proxy == null)
            {
                Cache.Instance.Log("Proxy is null. Choose a proxy under Hardware Settings");
                return;
            }

            if (!eA.HWSettings.Proxy.IsValid)
            {
                Cache.Instance.Log("Proxy is inValid");
                return;
            }

            Cache.Instance.Log("Starting Selenium (Chrome):");
            SeleniumAutomation seleniumAutomationImpl = new SeleniumAutomation();
            drivers.Add(seleniumAutomationImpl);

            IsbelEveAccount myIsbelEveAccount = new IsbelEveAccount(eA.AccountName,
                                                       eA.ConnectToTestServer,
                                                       eA.TranquilityEveAccessTokenString,
                                                       eA.TranquilityEveAccessTokenValidUntil,
                                                       eA.TranquilityRefreshTokenString,
                                                       eA.TranquilityRefreshTokenValidUntil,
                                                       eA.SisiEveAccessTokenString,
                                                       eA.SisiEveAccessTokenValidUntil,
                                                       eA.SisiRefreshTokenString,
                                                       eA.SisiRefreshTokenValidUntil);
            myIsbelEveAccount.LogIntoEveAccountWebSiteForSSOToken(eA, myIsbelEveAccount, seleniumAutomationImpl);

            //if (myIsbelEveAccount.EveAccessToken != null && !myIsbelEveAccount.EveAccessToken.IsExpired)
            //{
            //    Cache.Instance.Log("EveAccessToken: IsExpired [" + myIsbelEveAccount.EveAccessToken.IsExpired + "][" + myIsbelEveAccount.EveAccessToken.TokenString + "]");
            //    eA.QueueThisAccountToBeStarted(EveAccountStartPriority.ManuallyStartedPriority, "ManuallyStarted");
            //}
        }

        private void DuplicateThisAccountInLauncherItemClick(object sender, EventArgs e)
        {
            Cache.Instance.Log("User Clicked on [ Duplicate This Account In Launcher ]");
            if (!(ActiveControl is DataGridView dgv))
            {
                Cache.Instance.Log("if (!(ActiveControl is DataGridView dgv))");
                return;
            }

            int index = dgv.SelectedCells[0].OwningRow.Index;
            EveAccount eA = Cache.Instance.EveAccountSerializeableSortableBindingList.List[index];
            if (string.IsNullOrEmpty(eA.AccountName))
            {
                Cache.Instance.Log("Eve AccountName is blank for account [" + eA.MaskedAccountName + "]");
                return;
            }

            Cache.Instance.Log("ToDo: Impliment this!");
        }

        private void OpenEveAccountSiteUsingProxyItemClick(object sender, EventArgs e)
        {
            Cache.Instance.Log("User Clicked on [ OpenEveAccountSiteUsingProxy ]");
            if (!(ActiveControl is DataGridView dgv))
            {
                Cache.Instance.Log("if (!(ActiveControl is DataGridView dgv))");
                return;
            }

            int index = dgv.SelectedCells[0].OwningRow.Index;
            EveAccount eA = Cache.Instance.EveAccountSerializeableSortableBindingList.List[index];
            if (string.IsNullOrEmpty(eA.AccountName))
            {
                Cache.Instance.Log("Eve AccountName is blank for account [" + eA.MaskedAccountName + "]");
                return;
            }

            if (string.IsNullOrEmpty(eA.Password))
            {
                Cache.Instance.Log("Eve Password is blank for account [" + eA.MaskedAccountName + "]");
                return;
            }

            if (eA.HWSettings.Proxy == null)
            {
                Cache.Instance.Log("Proxy is null. Choose a proxy under Hardware Settings");
                return;
            }

            if (!eA.HWSettings.Proxy.IsValid)
            {
                Cache.Instance.Log("Proxy is inValid");
                return;
            }

            SeleniumAutomation seleniumAutomationImpl = new SeleniumAutomation();
            drivers.Add(seleniumAutomationImpl);
            seleniumAutomationImpl.CreateTaskToOpenEveAccountSite(eA.AccountName, eA.Password, eA.HWSettings);
        }

        private void EveClientLogsToolStripMenuItemClick(object sender, EventArgs e)
        {
            Cache.Instance.Log("User Clicked on [ EveClientLogs ]");
            if (!(ActiveControl is DataGridView dgv))
            {
                Cache.Instance.Log("if (!(ActiveControl is DataGridView dgv))");
                return;
            }

            int index = dgv.SelectedCells[0].OwningRow.Index;
            EveAccount eA = Cache.Instance.EveAccountSerializeableSortableBindingList.List[index];

            Process.Start(new ProcessStartInfo()
            {
                FileName = eA.EveClientLogpath,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        private void BotLogsToolStripMenuItemClick(object sender, EventArgs e)
        {
            Cache.Instance.Log("User Clicked on [ BotLogs ]");
            if (!(ActiveControl is DataGridView dgv))
            {
                Cache.Instance.Log("if (!(ActiveControl is DataGridView dgv))");
                return;
            }

            int index = dgv.SelectedCells[0].OwningRow.Index;
            EveAccount eA = Cache.Instance.EveAccountSerializeableSortableBindingList.List[index];

            Process.Start(new ProcessStartInfo()
            {
                FileName = eA.BotLogpath,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        private void BotXmlConfigToolStripMenuItemClick(object sender, EventArgs e)
        {
            Cache.Instance.Log("User Clicked on [ BotXmlConfig ]");
            if (!(ActiveControl is DataGridView dgv))
            {
                Cache.Instance.Log("if (!(ActiveControl is DataGridView dgv))");
                return;
            }

            int index = dgv.SelectedCells[0].OwningRow.Index;
            EveAccount eA = Cache.Instance.EveAccountSerializeableSortableBindingList.List[index];

            Process.Start(new ProcessStartInfo()
            {
                FileName = eA.BotXmlConfigFile,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        private void StartInjectToolStripMenuItemClick(object sender, EventArgs e)
        {
            Cache.Instance.Log("User Clicked on [ Start Eve ]");
            if (!(ActiveControl is DataGridView dgv))
            {
                Cache.Instance.Log("if (!(ActiveControl is DataGridView dgv))");
                return;
            }

            int index = dgv.SelectedCells[0].OwningRow.Index;
            EveAccount eA = Cache.Instance.EveAccountSerializeableSortableBindingList.List[index];

            eA.QueueThisAccountToBeStarted(EveAccountStartPriority.ManuallyStartedPriority, "ManuallyStarted");
        }

        private void StatisticsToolStripMenuItemClick(object sender, EventArgs e)
        {
            new Thread(() =>
                {
                    try
                    {
                        new StatisticsForm().ShowDialog();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            ).Start();
        }

        private void TextBoxPastebin_TextChanged(object sender, EventArgs e)
        {
            Cache.Instance.EveSettings.Pastebin = textBoxPastebin.Text;
        }

        private void TextBoxPastebinTextChanged(object sender, EventArgs e)
        {
            Cache.Instance.EveSettings.Pastebin = textBoxPastebin.Text;
        }

        //private void textBoxPastebin_TextChanged(object sender, EventArgs e)
        //{
        //    Cache.Instance.EveSettings.Pastebin = textBoxPastebin.Text;
        //}

        private DateTime _nextRecalcStats = DateTime.MinValue;
        private DateTime _nextRecalcStats2 = DateTime.MinValue;

        private void Timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                lblCurrentEVETime.Text = "EVE Time (GMT): [ " + DateTime.UtcNow.ToShortDateString() + " " + DateTime.UtcNow.ToLongTimeString() + " ]";
                lblISK.Text = "Wallet (ISK): " + EveSetting.FormatIsk(EveSetting.StatisticsAllIsk);
                lblLP.Text = "LoyaltyPoints (LP): " + EveSetting.FormatIsk(EveSetting.StatisticsAllLp);
                lblLPISK.Text = "LoyaltyPoints (ISK): " + EveSetting.FormatIsk(EveSetting.StatisticsAllLpValue);
                lblHangarValue.Text = "Hangar value (ISK): " + EveSetting.FormatIsk(EveSetting.StatisticsAllItemHangar);
                lblTotalValue.Text = "Net Worth (ISK): " + EveSetting.FormatIsk(EveSetting.StatisticsNetWorth);
                EveSharpCompileTimelbl.Text = "EVESharpCore.exe Compile Time: [ " + Cache.Instance.EveSharpCoreCompileTime + " ]";
                EveSharpLauncherCompileTimelbl.Text = "EVESharpLauncher.exe Compile Time: [ " + Cache.Instance.EveSharpLauncherCompileTime + " ]";
                HookManagerCompileTimelbl.Text = "HookManager.exe Compile Time: [ " + Cache.Instance.HookManagerCompileTime + " ]";
                LastScheduleIterationlbl.Text = "Last Scheduler Iteration: [" + EveManager.LastSchedulerIteration + "]";
                LastIPCIterationlbl.Text = "Last IPC Iteration: [" + EveManager.LastIPCIteration + "]";
                StartEveForTheseAccountsQueuelbl.Text = "StartEveForTheseAccountsQueue: [" + Cache.StartEveForTheseAccountsQueue.Count() + "]";
                SharedComponentsCompileTimelbl.Text = "SharedComponents.dll Compile Time: [ " + Cache.Instance.SharedComponentsCompileTime + " ]";
                if (EveManager.eveManagerDecideThread.IsAlive) lblEVESharpLauncherScheduler.Text = "EVESharpLauncher Scheduler: [ Running ]";
                else lblEVESharpLauncherScheduler.Text = "EVESharpLauncher Scheduler: [ Stopped ]";
                if (EveManager.eveSettingsManagerThread.IsAlive) lblEVESharpLauncherIPC.Text = "EVESharpLauncher IPC: [ Running ]";
                else lblEVESharpLauncherIPC.Text = "EVESharpLauncher IPC: [ Stopped ]";

                if (_nextRecalcStats > DateTime.UtcNow)
                {
                    EveSetting.ClearEveSettingsStatistics();
                    Cache.Instance.ClearCache();
                    _nextRecalcStats = _nextRecalcStats.AddSeconds(10);
                }

                if (_nextRecalcStats2 > DateTime.UtcNow)
                {
                    EveSetting.ClearEveSettingsStatisticsEveryFewSec();
                    _nextRecalcStats2 = _nextRecalcStats2.AddSeconds(3);
                }
            }
            catch (Exception)
            {
                //ignore this exception
            }
        }

        private void EVESSOLastStepManualCodeTorefreshTokenToolStripMenuItem1Click(object sender, EventArgs e)
        {
            DataGridView dgv = ActiveControl as DataGridView;
            if (dgv == null) return;

            int index = dgv.SelectedCells[0].OwningRow.Index;
            EveAccount eA = Cache.Instance.EveAccountSerializeableSortableBindingList.List[index];
            IsbelEveAccount myIsbelEveAccount = new IsbelEveAccount(eA.AccountName,
                                                       eA.ConnectToTestServer,
                                                       eA.TranquilityEveAccessTokenString,
                                                       eA.TranquilityEveAccessTokenValidUntil,
                                                       eA.TranquilityRefreshTokenString,
                                                       eA.TranquilityRefreshTokenValidUntil,
                                                       eA.SisiEveAccessTokenString,
                                                       eA.SisiEveAccessTokenValidUntil,
                                                       eA.SisiRefreshTokenString,
                                                       eA.SisiRefreshTokenValidUntil);
            myIsbelEveAccount.TakeCodeAsInputAndRequestRefreshToken(eA, myIsbelEveAccount);
        }

        private void StartBrowserSOCKS5ToolStripMenuItem1Click(object sender, EventArgs e)
        {
            DataGridView dgv = ActiveControl as DataGridView;
            if (dgv == null) return;

            int index = dgv.SelectedCells[0].OwningRow.Index;
            EveAccount eA = Cache.Instance.EveAccountSerializeableSortableBindingList.List[index];

            var seleniumAutomationImpl = new SeleniumAutomation();
            drivers.Add(seleniumAutomationImpl);
            new Task(() =>
            {
                try
                {
                    const string myURL = "https://www.google.com/search?q=what+is+my+ip";
                    seleniumAutomationImpl.OpenSeleniumChromeBrowser(seleniumAutomationImpl.MySocks5ChromeOptions(eA.HWSettings.Proxy), myURL);
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log("Exception: " + ex);
                }
            }).Start();
        }

        private void StartBrowserHTTPSToolStripMenuItem1Click(object sender, EventArgs e)
        {
            DataGridView dgv = ActiveControl as DataGridView;
            if (dgv == null) return;

            int index = dgv.SelectedCells[0].OwningRow.Index;
            EveAccount eA = Cache.Instance.EveAccountSerializeableSortableBindingList.List[index];

            var seleniumAutomationImpl = new SeleniumAutomation();
            drivers.Add(seleniumAutomationImpl);
            new Task(() =>
            {
                try
                {
                    const string myURL = "https://www.google.com/search?q=what+is+my+ip";
                    seleniumAutomationImpl.OpenSeleniumChromeBrowser(seleniumAutomationImpl.MyHTTPProxyChromeOptions(eA.HWSettings), myURL);
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log("Exception: " + ex);
                }
            }).Start();
        }

        private void UpdateEVESharpToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Cache.Instance.EveSettings.UrlToPullEveSharpCode))
                Util.RunInDirectory("Updater.exe", Cache.Instance.EveSettings.UrlToPullEveSharpCode, false);
            else
                Util.RunInDirectory("Updater.exe", false);

            Close();
            try
            {
                Application.Exit();
            }
            catch (Exception)
            {
                //ignore this exception
            }
        }

        #endregion Methods
    }
}