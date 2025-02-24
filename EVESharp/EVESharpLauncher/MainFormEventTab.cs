using SharedComponents.Controls;
using SharedComponents.EVE;
using SharedComponents.Events;
using SharedComponents.Extensions;
using SharedComponents.SharpLogLite.Model;
using SharedComponents.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace EVESharpLauncher
{
    internal class MainFormEventTab : TabPage
    {
        #region Constructors

        public string GUID { get; set; }

        public MainFormEventTab(string title, string _GUID, bool islogTab = false, Image image = null) : base(title)
        {
            GUID = _GUID;
            Leave += delegate
            {
                if (ImageIndex == 0)
                    ImageIndex = -1;
            };

            HandleDestroyed += delegate { };

            this.Title = title;
            HeaderlistViewDirectEveEvents = new ColumnHeader
            {
                Text = "",
                Width = 4000
            };

            HeaderlistViewSharpLogLiteMessages = new ColumnHeader
            {
                Text = "",
                Width = 4000
            };

            ListViewDirectEveEvents = new ListView
            {
                OwnerDraw = true,
                AutoArrange = false,
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0),
                HeaderStyle = ColumnHeaderStyle.None,
                Location = new Point(3, 3),
                UseCompatibleStateImageBehavior = false,
                View = View.Details,
                Activation = ItemActivation.Standard,
            };

            ListViewDirectEveEvents.Columns.AddRange(new[] { HeaderlistViewDirectEveEvents });
            ListViewDirectEveEvents.DrawColumnHeader += DrawColumnHeader;
            FormUtil.Color = ListViewDirectEveEvents.BackColor;
            FormUtil.Font = ListViewDirectEveEvents.Font;
            ListViewDirectEveEvents.DrawItem += FormUtil.DrawItem;

            ListViewDirectEveEvents.ItemActivate += delegate
            {
                int i = ListViewDirectEveEvents.SelectedIndices[0];
                new ListViewItemForm(Regex.Replace(ListViewDirectEveEvents.Items[i].Tag.ToString(), @"\r\n|\n\r|\n|\r", Environment.NewLine)).Show();
            };

            if (islogTab)
                Controls.Add(ListViewDirectEveEvents);

            if (!islogTab)
            {
                Text += " (0)";
                ListViewSharpLogLiteMessages = new ListView
                {
                    OwnerDraw = true,
                    AutoArrange = false,
                    Dock = DockStyle.Fill,
                    Font = new Font("Tahoma", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0),
                    HeaderStyle = ColumnHeaderStyle.None,
                    Location = new Point(3, 3),
                    UseCompatibleStateImageBehavior = false,
                    View = View.Details,
                    Activation = ItemActivation.Standard,
                };

                ListViewSharpLogLiteMessages.DrawColumnHeader += DrawColumnHeader;
                FormUtil.Color = ListViewSharpLogLiteMessages.BackColor;
                FormUtil.Font = ListViewSharpLogLiteMessages.Font;
                ListViewSharpLogLiteMessages.DrawItem += FormUtil.DrawItem;
                ListViewSharpLogLiteMessages.Columns.AddRange(new[] { HeaderlistViewSharpLogLiteMessages });

                ListViewSharpLogLiteMessages.ItemActivate += delegate
                {
                    int i = ListViewSharpLogLiteMessages.SelectedIndices[0];
                    new ListViewItemForm(Regex.Replace(ListViewSharpLogLiteMessages.Items[i].Text, @"\r\n|\n\r|\n|\r", Environment.NewLine)).Show();
                };

                TabControl tabControl = new TabControl
                {
                    Font = new Font("Tahoma", 7F, FontStyle.Regular, GraphicsUnit.Point, 0),
                    Location = new Point(3, 3),
                    Multiline = true,
                    SelectedIndex = 0,
                    Dock = DockStyle.Fill
                };

                TabPage tabPage1 = new TabPage("Events");
                TabPage tabPage2 = new TabPage("SharpLogLite");

                tabControl.TabPages.Add(tabPage1);
                tabControl.TabPages.Add(tabPage2);

                TableLayoutPanel = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    Location = new Point(3, 3),
                    RowCount = 1,
                    ColumnCount = 2
                };

                TableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                TableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
                TableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
                TableLayoutPanel.Controls.Add(ListViewDirectEveEvents, 0, 0);

                int directEventsCount = Enum.GetNames(typeof(DirectEvents)).Length;
                TableLayoutPanel eventTableLayoutPanel = new TableLayoutPanel
                {
                    Location = new Point(3, 3),
                    RowCount = directEventsCount,
                    ColumnCount = 1,
                    Dock = DockStyle.Fill,
                };

                eventTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                Dictionary<DirectEvents, Label> labelDict = new Dictionary<DirectEvents, Label>();

                foreach (DirectEvents type in Enum.GetValues(typeof(DirectEvents)))
                {
                    eventTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100 / directEventsCount));
                    Label l1 = new Label();
                    labelDict[type] = l1;
                    l1.Dock = DockStyle.Fill;
                    eventTableLayoutPanel.Controls.Add(l1);
                }

                EveAccount eA = null;
                Thread = new Thread(() =>
                {
                    DateTime lastUIRefresh = DateTime.MinValue;
                    while (true)
                        try
                        {
                            if (Cache.IsShuttingDown)
                                break;

                            if (lastUIRefresh.AddSeconds(5) > DateTime.UtcNow || !IsHandleCreated)
                            {
                                Thread.Sleep(500);
                                continue;
                            }

                            lastUIRefresh = DateTime.UtcNow;

                            if (ImageIndex != 0)
                            {
                                if (eA == null)
                                    eA =
                                        Cache.Instance.EveAccountSerializeableSortableBindingList.List.FirstOrDefault(
                                            e => e.GUID.ToLower().Equals(this.GUID.ToLower()));

                                if (eA == null)
                                    Invoke(new Action(() => ImageIndex = -1));

                                if (eA != null)
                                    if (eA.EveProcessExists)
                                    {
                                        if (ImageIndex != 1)
                                            Invoke(new Action(() => ImageIndex = 1));
                                    }
                                    else
                                    {
                                        if (ImageIndex != 2)
                                            Invoke(new Action(() => ImageIndex = 2));
                                    }
                            }

                            if (IsHandleCreated)
                                foreach (KeyValuePair<DirectEvents, Label> kv in labelDict)
                                {
                                    DateTime? lastEvent = DirectEventHandler.GetLastEventReceived(this.Title, kv.Key);
                                    string lastEventString = lastEvent.HasValue
                                        ? (DateTime.UtcNow - lastEvent.Value).Days.ToString("D2") + ":" +
                                          (DateTime.UtcNow - lastEvent.Value).Hours.ToString("D2") +
                                          ":" + (DateTime.UtcNow - lastEvent.Value).Minutes.ToString("D2") + ":" +
                                          (DateTime.UtcNow - lastEvent.Value).Seconds.ToString("D2") + " ago"
                                        : "-";
                                    try
                                    {
                                        kv.Value.Invoke(new Action(() => kv.Value.Text = kv.Key + ": " + lastEventString));
                                    }
                                    catch (Exception)
                                    {
                                        //ignore this exception
                                    }
                                }
                            Thread.Sleep(500);
                        }
                        catch (Exception ex)
                        {
                            Cache.Instance.Log(ex.ToString());
                            Debug.WriteLine(ex);
                        }
                });

                Thread.Start();

                TableLayoutPanel.Controls.Add(eventTableLayoutPanel, 1, 0);
                tabPage1.Controls.Add(TableLayoutPanel);
                tabPage2.Controls.Add(ListViewSharpLogLiteMessages);
                Controls.Add(tabControl);
            }
        }

        #endregion Constructors

        #region Destructors

        ~MainFormEventTab()
        {
        }

        #endregion Destructors

        #region Properties

        private EveAccount CorrespondingEveAccount { get; set; }
        private ColumnHeader HeaderlistViewDirectEveEvents { get; }
        private ColumnHeader HeaderlistViewSharpLogLiteMessages { get; }
        private ListView ListViewDirectEveEvents { get; }
        private ListView ListViewSharpLogLiteMessages { get; }
        private TableLayoutPanel TableLayoutPanel { get; }
        private Thread Thread { get; }
        private string Title { get; }

        #endregion Properties

        #region Methods

        public void AddCustomMessage(string header, string message)
        {
            Color col = Color.Black;
            ListViewItem item = new ListViewItem
            {
                ForeColor = col,
                Text = "[" + DateTime.UtcNow + "] [" + header + "]" + message
            };

            AddItemListViewDirectEveEvents(item);
        }

        public void AddMessage(string message)
        {
            Color col = Color.Black;
            ListViewItem item = new ListViewItem
            {
                ForeColor = col,
                Text = "[" + DateTime.UtcNow + "] [MESSAGE]" + message
            };

            AddItemListViewDirectEveEvents(item);
        }

        public void AddNewEvent(DirectEvent directEvent)
        {
            Color? col = directEvent.color;
            ListViewItem item = new ListViewItem
            {
                ForeColor = (Color)col,
                Text = "[" + string.Format("{0:dd-MMM-yy HH:mm:ss:fff}", DateTime.UtcNow) + "] [" + directEvent.type + "]" + directEvent.message
            };

            if (directEvent.warning)
                ImageIndex = 0;

            AddItemListViewDirectEveEvents(item);
        }

        public void AddRawMessage(string message)
        {
            Color col = Color.Black;
            int maxLen = 300;
            ListViewItem item = new ListViewItem
            {
                ForeColor = col,
                Text = message.Length > maxLen ? message.Substring(0, maxLen) + "..." : message,
                Tag = message,
            };

            AddItemListViewDirectEveEvents(item);
        }

        public void AddSharpLogLiteMessage(SharpLogMessage msg, string charName, string GUID)
        {
            try
            {
                if (CorrespondingEveAccount == null)
                    CorrespondingEveAccount =
                        Cache.Instance.EveAccountSerializeableSortableBindingList.List.Where(i => i.EveProcessExists).FirstOrDefault(e => e.GUID.ToLower()
                            .Equals(GUID.ToLower()));
                int val = 0;

                if (CorrespondingEveAccount != null)
                {
                    var stracktraceRegex = new Regex(@"STACKTRACE #[0-9]*", RegexOptions.Compiled);
                    var exRegex = new Regex(@"EXCEPTION #[0-9]*", RegexOptions.Compiled);
                    var valEx = 0;
                    var valStack = 0;
                    foreach (Match itemMatch in exRegex.Matches(msg.Message)) int.TryParse(Regex.Match(itemMatch.Value, @"\d+").Value, out valEx);

                    foreach (Match itemMatch in stracktraceRegex.Matches(msg.Message)) int.TryParse(Regex.Match(itemMatch.Value, @"\d+").Value, out valStack);
                    var prevValue = CorrespondingEveAccount.AmountExceptionsCurrentSession;

                    if (valEx != 0)
                        prevValue++;

                    if (valStack != 0)
                        prevValue++;

                    if (msg.Severity == LogSeverity.SEVERITY_ERR)
                    {
                        if (msg.Module.ContainsIgnoreCase("blue") && msg.Channel.ContainsIgnoreCase("resman"))
                        {
                            // TODO add some handler to NOT count specific errors
                        }
                        else if (msg.Module.ContainsIgnoreCase("destiny") && msg.Channel.ContainsIgnoreCase("Ball") && msg.Message.ContainsIgnoreCase("No valid ego"))
                        {
                            return;
                        }
                        else if (msg.Message.ContainsIgnoreCase("Audio listener"))
                        {
                            return;
                        }
                        else if (msg.Message.ContainsIgnoreCase("self.calls"))
                        {
                            return;
                        }
                        else if (msg.Message.ContainsIgnoreCase("Sculpting information for")) // [SEVERITY_ERR] [svc] [paperdoll] Sculpting information for  faceModifiers , ears_rleftshape is missing, skipping!
                        {
                            return;
                        }
                        else if (msg.Message.ContainsIgnoreCase("Failed to send event to Wwise")) //[SEVERITY_ERR] [audio2] [Main] Failed to send event to Wwise: ui_es_button_mouse_down_finalize_play
                        {
                            return;
                        }
                        else if (msg.Message.ContainsIgnoreCase("Neocom button bountyoffice is missing a label")) // [SEVERITY_ERR] [] [General] Neocom button bountyoffice is missing a label
                        {
                            return;
                        }
                        else if (msg.Message.ContainsIgnoreCase("No factory found for extension")) //[SEVERITY_ERR] [blue] [ResMan] BlueResMan::GetResourceW: No factory found for extension (None, atlas)
                        {
                            return;
                        }
                        else if (msg.Message.ContainsIgnoreCase("EVE Client version") && msg.Message.ContainsIgnoreCase("started")) //  [SEVERITY_ERR] [] [General] EVE Client version 20.11 build 2168478 ...
                        {
                            return;
                        }
                        else if (msg.Message.ContainsIgnoreCase("request for membership list timed out")) //[SEVERITY_ERR] [eve] [client.script.ui.services.local] request for membership list timed out
                        {
                            return;
                        }
                        else if (msg.Message.ContainsIgnoreCase("SkillPlansErrorTimeout")) //][SEVERITY_ERR]#[1755][skills][skillplan.skillPlanService] EXCEPTION ... : User error, msg=SkillPlansErrorTimeout, dict=None
                        {
                            return;
                        }
                        else if (msg.Message.ContainsIgnoreCase("Sentry send failed: HTTPSConnectionPool(host='sentry.io', port=443")) //[SEVERITY_WARN]#[0][monolithsentry][transport] Sentry send failed: HTTPSConnectionPool(host='sentry.io', port=443): Max retries exceeded with url: /api/1436728/store/ (Caused by NewConnectionError("<requests.packages.urllib3.connection.VerifiedHTTPSConnection object at 0x00000000CBE79550>: Failed to establish a new connection: [Errno 10061] No connection could be made because the target machine actively refused it: 'connect operation system fail'",))
                        {
                            return;
                        }
                        else if (msg.Message.ContainsIgnoreCase("MaxBandwidthExceeded2")) //[SEVERITY_WARN]# UserError: User error, msg=MaxBandwidthExceeded2, dict={'bandwidthLeft': 0.0, 'droneName': (4, 2466), 'droneBandwidthUsed': 5.0}
                        {
                            return;
                        }

                        else
                        {
                            prevValue++;
                        }
                    }

                    if (msg.Severity == LogSeverity.SEVERITY_WARN)
                    {
                        if (msg.Message.ContainsIgnoreCase("timeout waiting on membership notice for solar_system_id")) //SEVERITY_WARN]#[0][eve][client.script.ui.services.local] timeout waiting on membership notice for solar_system_id= ...
                        {
                            return;
                        }
                    }

                    if (msg.Severity == LogSeverity.SEVERITY_INFO)
                    {
                        return;
                    }

                    if (msg.Severity == LogSeverity.SEVERITY_NOTICE)
                    {
                        return;
                    }

                    if (CorrespondingEveAccount.AmountExceptionsCurrentSession != prevValue)
                        CorrespondingEveAccount.AmountExceptionsCurrentSession = prevValue;

                    Invoke(new Action(() =>
                    {
                        Text =
                            $"{charName} ({CorrespondingEveAccount.AmountExceptionsCurrentSession})";
                    }));

                }

                Color col = Color.Black;

                switch (msg.Severity)
                {
                    case LogSeverity.SEVERITY_INFO:
                        col = Color.Green;
                        break;

                    case LogSeverity.SEVERITY_NOTICE:
                        col = Color.Green;
                        break;

                    case LogSeverity.SEVERITY_WARN:
                        col = Color.Orange;
                        break;

                    case LogSeverity.SEVERITY_ERR:
                        col = Color.Red;
                        break;

                    case LogSeverity.SEVERITY_COUNT:
                        break;
                }

                var item = new ListViewItem();
                item.ForeColor = (Color)col;
                item.Text = $"[{String.Format("{0:dd-MMM-yy HH:mm:ss:fff}", msg.DateTime)}][{msg.Severity}]#[{CorrespondingEveAccount.AmountExceptionsCurrentSession}][{msg.Module}][{msg.Channel}] {msg.Message}";

                // log to file

                char s = Path.DirectorySeparatorChar;
                string path = Util.AssemblyPath + s + "Logs" + s + charName + s + "SharpLogLite";

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                string logFile = Path.Combine(path,
                    string.Format("{0:MM-dd-yyyy}", DateTime.Today) + "-" + charName + "-" + "SharpLogLite" + ".log");

                File.AppendAllText(logFile, item.Text + Environment.NewLine);

                AddItemSharpLogLiteMessage(item);
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception: " + ex);
            }
        }

        private void AddItemListViewDirectEveEvents(ListViewItem item)
        {
            if (ListViewDirectEveEvents.Items.Count >= 100)
                ListViewDirectEveEvents.Items.Clear();

            ListViewDirectEveEvents.Items.Add(item);

            if (ListViewDirectEveEvents.Items.Count > 0)
                ListViewDirectEveEvents.Items[ListViewDirectEveEvents.Items.Count - 1].EnsureVisible();
        }

        private void AddItemSharpLogLiteMessage(ListViewItem item)
        {
            if (ListViewSharpLogLiteMessages.Items.Count >= 100)
                ListViewSharpLogLiteMessages.Items.Clear();

            ListViewSharpLogLiteMessages.Items.Add(item);

            if (ListViewSharpLogLiteMessages.Items.Count > 0)
                ListViewSharpLogLiteMessages.Items[ListViewSharpLogLiteMessages.Items.Count - 1].EnsureVisible();
        }

        private void DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        #endregion Methods
    }
}