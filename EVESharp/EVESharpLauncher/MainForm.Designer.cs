/*
 * Created by SharpDevelop.
 * User: dserver
 * Date: 02.12.2013
 * Time: 09:09
 *
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace EVESharpLauncher
{
	partial class MainForm
	{
        /// <summary>
        /// Designer variable used to keep track of non-visual components.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private Button button1;
        private Button button2;
        private Button button3;
        private Button button4;
        private Button button5;
        private Button button6;
        private Button buttonCheckAccountLinks;
        private DataGridView dataGridEveAccounts;
        private FlowLayoutPanel flowLayoutPanel1;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private Label EveSharpCompileTimelbl;
        private Label EveSharpLauncherCompileTimelbl;
        private Label lblCurrentEVETime;
        private Label lblEVESharpLauncherIPC;
        private Label lblEVESharpLauncherScheduler;
        private Label lblHangarValue;
        private Label lblISK;
        private Label lblLP;
        private Label lblLPISK;
        private Label lblTotalValue;
        private System.Windows.Forms.Button buttonGenNewBeginEnd;
        private System.Windows.Forms.Button buttonStartEveManger;
        private System.Windows.Forms.Button buttonStopEveManger;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.NotifyIcon notifyIconQL;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.ToolStripMenuItem compileEveSharpCoreToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem editAdapteveHWProfileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showEveClientLogs;
        private System.Windows.Forms.ToolStripMenuItem browseToWebmail;
        private System.Windows.Forms.ToolStripMenuItem browseToEveSsoLogin;
        private System.Windows.Forms.ToolStripMenuItem clearEveAccessTokens;
        private System.Windows.Forms.ToolStripMenuItem clearRefreshTokens;
        private System.Windows.Forms.ToolStripMenuItem debugShowAllCookies;
        private System.Windows.Forms.ToolStripMenuItem browseToEveAccountSite;
        private System.Windows.Forms.ToolStripMenuItem showBotLogs;
        private System.Windows.Forms.ToolStripMenuItem showBotXmlConfig;
        private System.Windows.Forms.ToolStripMenuItem hideToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem advancedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectProcessToProxyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem StartBrowserSOCKS5StripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem StartBrowserHTTPSStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem EVESSOLastStepManualCodeTorefreshTokenMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startInjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem statisticsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updateEVESharpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem windowsToolStripMenuItem;
        private TabControl tabControl2;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TabPage tabPage3;
        private TextBox textBoxPastebin;
        private Timer timer1;
        private ToolStripMenuItem clearCacheToolStripMenuItem;
	    private ToolStripMenuItem testImapEmailToolStripMenuItem;
        private ToolStripMenuItem columnsToolStripMenuItem;
        private ToolStripMenuItem openEveAccountCreatorToolStripMenuItem;
        private ToolStripMenuItem proxiesToolStripMenuItem;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripMenuItem showRealHardwareInfoToolStripMenuItem;

        /// <summary>
        /// Disposes resources used by the form.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.startInjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showBotXmlConfig = new System.Windows.Forms.ToolStripMenuItem();
            this.showBotLogs = new System.Windows.Forms.ToolStripMenuItem();
            this.StartBrowserSOCKS5StripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.EVESSOLastStepManualCodeTorefreshTokenMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.StartBrowserHTTPSStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.browseToWebmail = new System.Windows.Forms.ToolStripMenuItem();
            this.browseToEveSsoLogin = new System.Windows.Forms.ToolStripMenuItem();
            this.clearEveAccessTokens = new System.Windows.Forms.ToolStripMenuItem();
            this.clearRefreshTokens = new System.Windows.Forms.ToolStripMenuItem();
            this.debugShowAllCookies = new System.Windows.Forms.ToolStripMenuItem();
            this.browseToEveAccountSite = new System.Windows.Forms.ToolStripMenuItem();
            this.editAdapteveHWProfileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showEveClientLogs = new System.Windows.Forms.ToolStripMenuItem();
            this.columnsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.advancedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectProcessToProxyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearCacheToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testImapEmailToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonGenNewBeginEnd = new System.Windows.Forms.Button();
            this.buttonStopEveManger = new System.Windows.Forms.Button();
            this.buttonStartEveManger = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.notifyIconQL = new System.Windows.Forms.NotifyIcon(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.proxiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statisticsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.compileEveSharpCoreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateEVESharpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.windowsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openEveAccountCreatorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showRealHardwareInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hideToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.LastIPCIterationlbl = new System.Windows.Forms.Label();
            this.LastScheduleIterationlbl = new System.Windows.Forms.Label();
            this.StartEveForTheseAccountsQueuelbl = new System.Windows.Forms.Label();
            this.SharedComponentsCompileTimelbl = new System.Windows.Forms.Label();
            this.HookManagerCompileTimelbl = new System.Windows.Forms.Label();
            this.lblEVESharpLauncherIPC = new System.Windows.Forms.Label();
            this.lblEVESharpLauncherScheduler = new System.Windows.Forms.Label();
            this.lblTotalValue = new System.Windows.Forms.Label();
            this.lblHangarValue = new System.Windows.Forms.Label();
            this.lblLPISK = new System.Windows.Forms.Label();
            this.lblLP = new System.Windows.Forms.Label();
            this.EveSharpLauncherCompileTimelbl = new System.Windows.Forms.Label();
            this.EveSharpCompileTimelbl = new System.Windows.Forms.Label();
            this.lblCurrentEVETime = new System.Windows.Forms.Label();
            this.lblISK = new System.Windows.Forms.Label();
            this.textBoxPastebin = new System.Windows.Forms.TextBox();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.buttonCheckAccountLinks = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.button6 = new System.Windows.Forms.Button();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dataGridEveAccounts = new System.Windows.Forms.DataGridView();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.button7 = new System.Windows.Forms.Button();
            this.editPatternManagerSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tabControl2.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridEveAccounts)).BeginInit();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startInjectToolStripMenuItem,
            this.showBotXmlConfig,
            this.showBotLogs,
            this.StartBrowserSOCKS5StripMenuItem,
            this.StartBrowserHTTPSStripMenuItem,
            this.browseToWebmail,
            this.browseToEveSsoLogin,
            this.EVESSOLastStepManualCodeTorefreshTokenMenuItem,
            this.clearEveAccessTokens,
            this.clearRefreshTokens,
            this.editAdapteveHWProfileToolStripMenuItem,
            this.showEveClientLogs,
            this.columnsToolStripMenuItem,
            this.advancedToolStripMenuItem,
            this.deleteToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(260, 268);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuStrip1Opening);
            // 
            // startInjectToolStripMenuItem
            // 
            this.startInjectToolStripMenuItem.Name = "startInjectToolStripMenuItem";
            this.startInjectToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
            this.startInjectToolStripMenuItem.Text = "Start Eve";
            this.startInjectToolStripMenuItem.Click += new System.EventHandler(this.StartInjectToolStripMenuItemClick);
            // 
            // showBotXmlConfig
            // 
            this.showBotXmlConfig.Name = "showBotXmlConfig";
            this.showBotXmlConfig.Size = new System.Drawing.Size(259, 22);
            this.showBotXmlConfig.Text = "Edit bot xml config";
            this.showBotXmlConfig.Click += new System.EventHandler(this.BotXmlConfigToolStripMenuItemClick);
            // 
            // showBotLogs
            // 
            this.showBotLogs.Name = "showBotLogs";
            this.showBotLogs.Size = new System.Drawing.Size(259, 22);
            this.showBotLogs.Text = "Open bot logs directory";
            this.showBotLogs.Click += new System.EventHandler(this.BotLogsToolStripMenuItemClick);
            // 
            // StartBrowserSOCKS5StripMenuItem
            // 
            this.StartBrowserSOCKS5StripMenuItem.Name = "StartBrowserSOCKS5StripMenuItem";
            this.StartBrowserSOCKS5StripMenuItem.Size = new System.Drawing.Size(259, 22);
            this.StartBrowserSOCKS5StripMenuItem.Text = "Start Browser using SOCKS5 proxy";
            this.StartBrowserSOCKS5StripMenuItem.Click += new System.EventHandler(this.StartBrowserSOCKS5ToolStripMenuItem1Click);
            // 
            // StartBrowserSOCKS5StripMenuItem
            // 
            this.EVESSOLastStepManualCodeTorefreshTokenMenuItem.Name = "EVESSOLastStepManualCodeTorefreshTokenMenuItem";
            this.EVESSOLastStepManualCodeTorefreshTokenMenuItem.Size = new System.Drawing.Size(259, 22);
            this.EVESSOLastStepManualCodeTorefreshTokenMenuItem.Text = "Take Manually Entered Code and make refresh Token";
            this.EVESSOLastStepManualCodeTorefreshTokenMenuItem.Click += new System.EventHandler(this.EVESSOLastStepManualCodeTorefreshTokenToolStripMenuItem1Click);
            // 
            // StartBrowserHTTPSStripMenuItem
            // 
            this.StartBrowserHTTPSStripMenuItem.Name = "StartBrowserHTTPSStripMenuItem";
            this.StartBrowserHTTPSStripMenuItem.Size = new System.Drawing.Size(259, 22);
            this.StartBrowserHTTPSStripMenuItem.Text = "Start Browser using HTTPS proxy";
            this.StartBrowserHTTPSStripMenuItem.Click += new System.EventHandler(this.StartBrowserHTTPSToolStripMenuItem1Click);
            // 
            // browseToWebmail
            // 
            this.browseToWebmail.Name = "browseToWebmail";
            this.browseToWebmail.Size = new System.Drawing.Size(259, 22);
            this.browseToWebmail.Text = "Open email using proxy";
            this.browseToWebmail.Click += new System.EventHandler(this.OpenEmailUsingProxyItemClick);
            // 
            // browseToEveSsoLogin
            // 
            this.browseToEveSsoLogin.Name = "browseToEveSsoLogin";
            this.browseToEveSsoLogin.Size = new System.Drawing.Size(259, 22);
            this.browseToEveSsoLogin.Text = "Open Eve SSO Website using proxy";
            this.browseToEveSsoLogin.Click += new System.EventHandler(this.OpenEveSSOWebsiteUsingProxyItemClick);
            // 
            // clearEveAccessTokens
            // 
            this.clearEveAccessTokens.Name = "clearEveAccessTokens";
            this.clearEveAccessTokens.Size = new System.Drawing.Size(259, 22);
            this.clearEveAccessTokens.Text = "Clear EveAccessTokens - usually lasts 500 min";
            this.clearEveAccessTokens.Click += new System.EventHandler(this.clearEveAccessTokensItemClick);
            // 
            // clearEveAccessTokens
            // 
            this.clearRefreshTokens.Name = "clearRefreshTokens";
            this.clearRefreshTokens.Size = new System.Drawing.Size(259, 22);
            this.clearRefreshTokens.Text = "Clear RefreshTokens - usually lasts 90 days";
            this.clearRefreshTokens.Click += new System.EventHandler(this.clearRefreshTokensItemClick);
            
            // 
            // debugShowAllCookies
            // 
            this.debugShowAllCookies.Name = "debugShowAllCookies";
            this.debugShowAllCookies.Size = new System.Drawing.Size(259, 22);
            this.debugShowAllCookies.Text = "debug ShowAllCookies";
            this.debugShowAllCookies.Click += new System.EventHandler(this.debugShowAllCookiesItemClick);

            // 
            // browseToEveAccountSite
            // 
            this.browseToEveAccountSite.Name = "browseToEveAccountSite";
            this.browseToEveAccountSite.Size = new System.Drawing.Size(259, 22);
            this.browseToEveAccountSite.Text = "Open Eve Account Site using proxy";
            this.browseToEveAccountSite.Click += new System.EventHandler(this.OpenEveAccountSiteUsingProxyItemClick);
            // 
            // browseToEveAccountSite
            // 
            this.browseToEveAccountSite.Name = "duplicateThisAccountInLauncher";
            this.browseToEveAccountSite.Size = new System.Drawing.Size(259, 22);
            this.browseToEveAccountSite.Text = "Duplicate This Account In Launcher";
            this.browseToEveAccountSite.Click += new System.EventHandler(this.DuplicateThisAccountInLauncherItemClick);
            // 
            // editAdapteveHWProfileToolStripMenuItem
            // 
            this.editAdapteveHWProfileToolStripMenuItem.Name = "editAdapteveHWProfileToolStripMenuItem";
            this.editAdapteveHWProfileToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
            this.editAdapteveHWProfileToolStripMenuItem.Text = "Edit hardware profile";
            this.editAdapteveHWProfileToolStripMenuItem.Click += new System.EventHandler(this.EditAdapteveHWProfileToolStripMenuItemClick);
            // 
            // showEveClientLogs
            // 
            this.showEveClientLogs.Name = "showEveClientLogs";
            this.showEveClientLogs.Size = new System.Drawing.Size(259, 22);
            this.showEveClientLogs.Text = "Open Eve client logs directory";
            this.showEveClientLogs.Click += new System.EventHandler(this.EveClientLogsToolStripMenuItemClick);
            // 
            // columnsToolStripMenuItem
            // 
            this.columnsToolStripMenuItem.Name = "columnsToolStripMenuItem";
            this.columnsToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
            this.columnsToolStripMenuItem.Text = "Columns";
            // 
            // advancedToolStripMenuItem
            // 
            this.advancedToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectProcessToProxyToolStripMenuItem,
            this.clearCacheToolStripMenuItem,
            this.debugShowAllCookies,
            this.browseToEveAccountSite,
            });
            this.advancedToolStripMenuItem.Name = "advancedToolStripMenuItem";
            this.advancedToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
            this.advancedToolStripMenuItem.Text = "Advanced";
            // 
            // selectProcessToProxyToolStripMenuItem
            // 
            this.selectProcessToProxyToolStripMenuItem.Name = "selectProcessToProxyToolStripMenuItem";
            this.selectProcessToProxyToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
            this.selectProcessToProxyToolStripMenuItem.Text = "Start selected exe with proxy";
            this.selectProcessToProxyToolStripMenuItem.Click += new System.EventHandler(this.SelectProcessToProxyToolStripMenuItemClick);
            // 
            // clearCacheToolStripMenuItem
            // 
            this.clearCacheToolStripMenuItem.Name = "clearCacheToolStripMenuItem";
            this.clearCacheToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
            this.clearCacheToolStripMenuItem.Text = "Clear cache";
            this.clearCacheToolStripMenuItem.Click += new System.EventHandler(this.ClearCacheToolStripMenuItem_Click);
            // 
            // testImapEmailToolStripMenuItem
            // 
            this.testImapEmailToolStripMenuItem.Name = "testImapEmailToolStripMenuItem";
            this.testImapEmailToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
            this.testImapEmailToolStripMenuItem.Text = "Test IMAP Email";
            this.testImapEmailToolStripMenuItem.Click += new System.EventHandler(this.TestImapEmailToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteToolStripMenuItem1});
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            // 
            // deleteToolStripMenuItem1
            // 
            this.deleteToolStripMenuItem1.Name = "deleteToolStripMenuItem1";
            this.deleteToolStripMenuItem1.Size = new System.Drawing.Size(118, 22);
            this.deleteToolStripMenuItem1.Text = "Confirm";
            this.deleteToolStripMenuItem1.Click += new System.EventHandler(this.DeleteToolStripMenuItem1Click);
            // 
            // buttonGenNewBeginEnd
            // 
            this.buttonGenNewBeginEnd.Location = new System.Drawing.Point(18, 77);
            this.buttonGenNewBeginEnd.Name = "buttonGenNewBeginEnd";
            this.buttonGenNewBeginEnd.Size = new System.Drawing.Size(182, 20);
            this.buttonGenNewBeginEnd.TabIndex = 27;
            this.buttonGenNewBeginEnd.Text = "Generate new time spans";
            this.buttonGenNewBeginEnd.UseVisualStyleBackColor = true;
            this.buttonGenNewBeginEnd.Click += new System.EventHandler(this.ButtonGenNewBeginEndClick);
            // 
            // buttonStopEveManger
            // 
            this.buttonStopEveManger.Location = new System.Drawing.Point(110, 25);
            this.buttonStopEveManger.Name = "buttonStopEveManger";
            this.buttonStopEveManger.Size = new System.Drawing.Size(90, 20);
            this.buttonStopEveManger.TabIndex = 23;
            this.buttonStopEveManger.Text = "Stop";
            this.buttonStopEveManger.UseVisualStyleBackColor = true;
            this.buttonStopEveManger.Click += new System.EventHandler(this.ButtonStopEveMangerClick);
            // 
            // buttonStartEveManger
            // 
            this.buttonStartEveManger.Location = new System.Drawing.Point(18, 25);
            this.buttonStartEveManger.Name = "buttonStartEveManger";
            this.buttonStartEveManger.Size = new System.Drawing.Size(90, 20);
            this.buttonStartEveManger.TabIndex = 22;
            this.buttonStartEveManger.Text = "Start";
            this.buttonStartEveManger.UseVisualStyleBackColor = true;
            this.buttonStartEveManger.Click += new System.EventHandler(this.ButtonStartEveMangerClick);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(21, 22);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(182, 20);
            this.button2.TabIndex = 35;
            this.button2.Text = "Goto homestation and idle";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.Button2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(83, 183);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(56, 23);
            this.button1.TabIndex = 34;
            this.button1.Text = "test";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // notifyIconQL
            // 
            this.notifyIconQL.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIconQL.Icon")));
            this.notifyIconQL.Click += new System.EventHandler(this.NotifyIconQL_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.SystemColors.Window;
            this.menuStrip1.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.menuStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Visible;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.proxiesToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.statisticsToolStripMenuItem,
            this.updateToolStripMenuItem,
            this.windowsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1468, 24);
            this.menuStrip1.TabIndex = 21;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // proxiesToolStripMenuItem
            // 
            this.proxiesToolStripMenuItem.Name = "proxiesToolStripMenuItem";
            this.proxiesToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
            this.proxiesToolStripMenuItem.Text = "Proxies";
            this.proxiesToolStripMenuItem.Click += new System.EventHandler(this.ProxiesToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(64, 20);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.SettingsToolStripMenuItem_Click);
            // 
            // statisticsToolStripMenuItem
            // 
            this.statisticsToolStripMenuItem.Name = "statisticsToolStripMenuItem";
            this.statisticsToolStripMenuItem.Size = new System.Drawing.Size(67, 20);
            this.statisticsToolStripMenuItem.Text = "Statistics";
            this.statisticsToolStripMenuItem.Click += new System.EventHandler(this.StatisticsToolStripMenuItemClick);
            // 
            // updateToolStripMenuItem
            // 
            this.updateToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.compileEveSharpCoreToolStripMenuItem,
            this.updateEVESharpToolStripMenuItem});
            this.updateToolStripMenuItem.Name = "updateToolStripMenuItem";
            this.updateToolStripMenuItem.Size = new System.Drawing.Size(59, 20);
            this.updateToolStripMenuItem.Text = "Update";
            // 
            // compileEveSharpCoreToolStripMenuItem
            // 
            this.compileEveSharpCoreToolStripMenuItem.Name = "compileEveSharpCoreToolStripMenuItem";
            this.compileEveSharpCoreToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.compileEveSharpCoreToolStripMenuItem.Text = "Compile EveSharpCore";
            this.compileEveSharpCoreToolStripMenuItem.Click += new System.EventHandler(this.CompileEveSharpCoreToolStripMenuItemClick);
            // 
            // updateEVESharpToolStripMenuItem
            // 
            this.updateEVESharpToolStripMenuItem.Name = "updateEVESharpToolStripMenuItem";
            this.updateEVESharpToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.updateEVESharpToolStripMenuItem.Text = "Update EVESharp";
            this.updateEVESharpToolStripMenuItem.Click += new System.EventHandler(this.UpdateEVESharpToolStripMenuItemClick);
            // 
            // windowsToolStripMenuItem
            // 
            this.windowsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openEveAccountCreatorToolStripMenuItem,
            this.showRealHardwareInfoToolStripMenuItem,
            this.hideToolStripMenuItem,
            this.showToolStripMenuItem});
            this.windowsToolStripMenuItem.Name = "windowsToolStripMenuItem";
            this.windowsToolStripMenuItem.Size = new System.Drawing.Size(69, 20);
            this.windowsToolStripMenuItem.Text = "Windows";
            // 
            // openEveAccountCreatorToolStripMenuItem
            // 
            this.openEveAccountCreatorToolStripMenuItem.Name = "openEveAccountCreatorToolStripMenuItem";
            this.openEveAccountCreatorToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.openEveAccountCreatorToolStripMenuItem.Text = "Open account creator";
            this.openEveAccountCreatorToolStripMenuItem.Click += new System.EventHandler(this.OpenEveAccountCreatorToolStripMenuItem_Click);
            // 
            // showRealHardwareInfoToolStripMenuItem
            // 
            this.showRealHardwareInfoToolStripMenuItem.Name = "showRealHardwareInfoToolStripMenuItem";
            this.showRealHardwareInfoToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.showRealHardwareInfoToolStripMenuItem.Text = "Show real hardware info";
            this.showRealHardwareInfoToolStripMenuItem.Click += new System.EventHandler(this.ShowRealHardwareInfoToolStripMenuItem_Click);
            // 
            // hideToolStripMenuItem
            // 
            this.hideToolStripMenuItem.Name = "hideToolStripMenuItem";
            this.hideToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.A)));
            this.hideToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.hideToolStripMenuItem.Text = "Hide eve windows";
            this.hideToolStripMenuItem.Click += new System.EventHandler(this.HideToolStripMenuItemClick);
            // 
            // showToolStripMenuItem
            // 
            this.showToolStripMenuItem.Name = "showToolStripMenuItem";
            this.showToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
            this.showToolStripMenuItem.Size = new System.Drawing.Size(254, 22);
            this.showToolStripMenuItem.Text = "Show eve windows";
            this.showToolStripMenuItem.Click += new System.EventHandler(this.ShowToolStripMenuItemClick);
            // 
            // groupBox2
            // 
            this.groupBox2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox2.BackColor = System.Drawing.SystemColors.Menu;
            this.groupBox2.Controls.Add(this.LastIPCIterationlbl);
            this.groupBox2.Controls.Add(this.LastScheduleIterationlbl);
            this.groupBox2.Controls.Add(this.StartEveForTheseAccountsQueuelbl);
            this.groupBox2.Controls.Add(this.SharedComponentsCompileTimelbl);
            this.groupBox2.Controls.Add(this.HookManagerCompileTimelbl);
            this.groupBox2.Controls.Add(this.lblEVESharpLauncherIPC);
            this.groupBox2.Controls.Add(this.lblEVESharpLauncherScheduler);
            this.groupBox2.Controls.Add(this.lblTotalValue);
            this.groupBox2.Controls.Add(this.lblHangarValue);
            this.groupBox2.Controls.Add(this.lblLPISK);
            this.groupBox2.Controls.Add(this.lblLP);
            this.groupBox2.Controls.Add(this.EveSharpLauncherCompileTimelbl);
            this.groupBox2.Controls.Add(this.EveSharpCompileTimelbl);
            this.groupBox2.Controls.Add(this.lblCurrentEVETime);
            this.groupBox2.Controls.Add(this.lblISK);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(1089, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(368, 250);
            this.groupBox2.TabIndex = 23;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Info";
            // 
            // LastIPCIterationlbl
            // 
            this.LastIPCIterationlbl.AutoSize = true;
            this.LastIPCIterationlbl.Cursor = System.Windows.Forms.Cursors.Default;
            this.LastIPCIterationlbl.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LastIPCIterationlbl.ForeColor = System.Drawing.SystemColors.ControlText;
            this.LastIPCIterationlbl.Location = new System.Drawing.Point(26, 112);
            this.LastIPCIterationlbl.Name = "LastIPCIterationlbl";
            this.LastIPCIterationlbl.Size = new System.Drawing.Size(99, 13);
            this.LastIPCIterationlbl.TabIndex = 53;
            this.LastIPCIterationlbl.Text = "Last IPC Iteration: ";
            // 
            // LastScheduleIterationlbl
            // 
            this.LastScheduleIterationlbl.AutoSize = true;
            this.LastScheduleIterationlbl.Cursor = System.Windows.Forms.Cursors.Default;
            this.LastScheduleIterationlbl.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LastScheduleIterationlbl.ForeColor = System.Drawing.SystemColors.ControlText;
            this.LastScheduleIterationlbl.Location = new System.Drawing.Point(26, 99);
            this.LastScheduleIterationlbl.Name = "LastScheduleIterationlbl";
            this.LastScheduleIterationlbl.Size = new System.Drawing.Size(122, 13);
            this.LastScheduleIterationlbl.TabIndex = 52;
            this.LastScheduleIterationlbl.Text = "Last Schedule Iteration:";
            // 
            // StartEveForTheseAccountsQueuelbl
            // 
            this.StartEveForTheseAccountsQueuelbl.AutoSize = true;
            this.StartEveForTheseAccountsQueuelbl.Cursor = System.Windows.Forms.Cursors.Default;
            this.StartEveForTheseAccountsQueuelbl.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StartEveForTheseAccountsQueuelbl.ForeColor = System.Drawing.SystemColors.ControlText;
            this.StartEveForTheseAccountsQueuelbl.Location = new System.Drawing.Point(25, 200);
            this.StartEveForTheseAccountsQueuelbl.Name = "StartEveForTheseAccountsQueuelbl";
            this.StartEveForTheseAccountsQueuelbl.Size = new System.Drawing.Size(177, 13);
            this.StartEveForTheseAccountsQueuelbl.TabIndex = 51;
            this.StartEveForTheseAccountsQueuelbl.Text = "StartEveForTheseAccountsQueue: ";
            // 
            // SharedComponentsCompileTimelbl
            // 
            this.SharedComponentsCompileTimelbl.AutoSize = true;
            this.SharedComponentsCompileTimelbl.BackColor = System.Drawing.SystemColors.Menu;
            this.SharedComponentsCompileTimelbl.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SharedComponentsCompileTimelbl.Location = new System.Drawing.Point(26, 171);
            this.SharedComponentsCompileTimelbl.Name = "SharedComponentsCompileTimelbl";
            this.SharedComponentsCompileTimelbl.Size = new System.Drawing.Size(187, 13);
            this.SharedComponentsCompileTimelbl.TabIndex = 50;
            this.SharedComponentsCompileTimelbl.Text = "SharedComponents.dll Compile Time: ";
            this.SharedComponentsCompileTimelbl.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // HookManagerCompileTimelbl
            // 
            this.HookManagerCompileTimelbl.AutoSize = true;
            this.HookManagerCompileTimelbl.BackColor = System.Drawing.SystemColors.Menu;
            this.HookManagerCompileTimelbl.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HookManagerCompileTimelbl.Location = new System.Drawing.Point(26, 185);
            this.HookManagerCompileTimelbl.Name = "HookManagerCompileTimelbl";
            this.HookManagerCompileTimelbl.Size = new System.Drawing.Size(167, 13);
            this.HookManagerCompileTimelbl.TabIndex = 49;
            this.HookManagerCompileTimelbl.Text = "HookManager.exe Compile Time: ";
            this.HookManagerCompileTimelbl.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblEVESharpLauncherIPC
            // 
            this.lblEVESharpLauncherIPC.AutoSize = true;
            this.lblEVESharpLauncherIPC.BackColor = System.Drawing.SystemColors.Menu;
            this.lblEVESharpLauncherIPC.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEVESharpLauncherIPC.Location = new System.Drawing.Point(26, 229);
            this.lblEVESharpLauncherIPC.Name = "lblEVESharpLauncherIPC";
            this.lblEVESharpLauncherIPC.Size = new System.Drawing.Size(166, 13);
            this.lblEVESharpLauncherIPC.TabIndex = 48;
            this.lblEVESharpLauncherIPC.Text = "EVESharp Launcher IPC: Running";
            this.lblEVESharpLauncherIPC.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblEVESharpLauncherScheduler
            // 
            this.lblEVESharpLauncherScheduler.AutoSize = true;
            this.lblEVESharpLauncherScheduler.BackColor = System.Drawing.SystemColors.Menu;
            this.lblEVESharpLauncherScheduler.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEVESharpLauncherScheduler.Location = new System.Drawing.Point(26, 215);
            this.lblEVESharpLauncherScheduler.Name = "lblEVESharpLauncherScheduler";
            this.lblEVESharpLauncherScheduler.Size = new System.Drawing.Size(216, 13);
            this.lblEVESharpLauncherScheduler.TabIndex = 47;
            this.lblEVESharpLauncherScheduler.Text = "EVESharp Launcher Scheduler: Not Running";
            this.lblEVESharpLauncherScheduler.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblTotalValue
            // 
            this.lblTotalValue.AutoSize = true;
            this.lblTotalValue.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalValue.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblTotalValue.Location = new System.Drawing.Point(26, 77);
            this.lblTotalValue.Name = "lblTotalValue";
            this.lblTotalValue.Size = new System.Drawing.Size(91, 13);
            this.lblTotalValue.TabIndex = 46;
            this.lblTotalValue.Text = "Net Worth (ISK): ";
            // 
            // lblHangarValue
            // 
            this.lblHangarValue.AutoSize = true;
            this.lblHangarValue.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHangarValue.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblHangarValue.Location = new System.Drawing.Point(26, 62);
            this.lblHangarValue.Name = "lblHangarValue";
            this.lblHangarValue.Size = new System.Drawing.Size(102, 13);
            this.lblHangarValue.TabIndex = 45;
            this.lblHangarValue.Text = "Hangar value (ISK):";
            // 
            // lblLPISK
            // 
            this.lblLPISK.AutoSize = true;
            this.lblLPISK.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLPISK.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblLPISK.Location = new System.Drawing.Point(26, 47);
            this.lblLPISK.Name = "lblLPISK";
            this.lblLPISK.Size = new System.Drawing.Size(105, 13);
            this.lblLPISK.TabIndex = 44;
            this.lblLPISK.Text = "LoyaltyPoints (ISK): ";
            // 
            // lblLP
            // 
            this.lblLP.AutoSize = true;
            this.lblLP.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLP.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblLP.Location = new System.Drawing.Point(26, 31);
            this.lblLP.Name = "lblLP";
            this.lblLP.Size = new System.Drawing.Size(100, 13);
            this.lblLP.TabIndex = 43;
            this.lblLP.Text = "LoyaltyPoints (LP): ";
            // 
            // EveSharpLauncherCompileTimelbl
            // 
            this.EveSharpLauncherCompileTimelbl.AutoSize = true;
            this.EveSharpLauncherCompileTimelbl.BackColor = System.Drawing.SystemColors.Menu;
            this.EveSharpLauncherCompileTimelbl.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EveSharpLauncherCompileTimelbl.Location = new System.Drawing.Point(26, 157);
            this.EveSharpLauncherCompileTimelbl.Name = "EveSharpLauncherCompileTimelbl";
            this.EveSharpLauncherCompileTimelbl.Size = new System.Drawing.Size(191, 13);
            this.EveSharpLauncherCompileTimelbl.TabIndex = 42;
            this.EveSharpLauncherCompileTimelbl.Text = "EVESharpLauncher.exe Compile Time: ";
            this.EveSharpLauncherCompileTimelbl.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // EveSharpCompileTimelbl
            // 
            this.EveSharpCompileTimelbl.AutoSize = true;
            this.EveSharpCompileTimelbl.BackColor = System.Drawing.SystemColors.Menu;
            this.EveSharpCompileTimelbl.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EveSharpCompileTimelbl.Location = new System.Drawing.Point(26, 144);
            this.EveSharpCompileTimelbl.Name = "EveSharpCompileTimelbl";
            this.EveSharpCompileTimelbl.Size = new System.Drawing.Size(147, 13);
            this.EveSharpCompileTimelbl.TabIndex = 41;
            this.EveSharpCompileTimelbl.Text = "EVESharp.exe Compile Time: ";
            this.EveSharpCompileTimelbl.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblCurrentEVETime
            // 
            this.lblCurrentEVETime.AutoSize = true;
            this.lblCurrentEVETime.BackColor = System.Drawing.SystemColors.Menu;
            this.lblCurrentEVETime.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lblCurrentEVETime.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCurrentEVETime.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblCurrentEVETime.Location = new System.Drawing.Point(26, 131);
            this.lblCurrentEVETime.Name = "lblCurrentEVETime";
            this.lblCurrentEVETime.Size = new System.Drawing.Size(107, 13);
            this.lblCurrentEVETime.TabIndex = 40;
            this.lblCurrentEVETime.Text = "Current EVE Time is: ";
            // 
            // lblISK
            // 
            this.lblISK.AutoSize = true;
            this.lblISK.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblISK.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblISK.Location = new System.Drawing.Point(26, 16);
            this.lblISK.Name = "lblISK";
            this.lblISK.Size = new System.Drawing.Size(71, 13);
            this.lblISK.TabIndex = 28;
            this.lblISK.Text = "Wallet (ISK): ";
            // 
            // textBoxPastebin
            // 
            this.textBoxPastebin.AcceptsReturn = true;
            this.textBoxPastebin.AcceptsTab = true;
            this.textBoxPastebin.AllowDrop = true;
            this.textBoxPastebin.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxPastebin.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxPastebin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxPastebin.Location = new System.Drawing.Point(3, 3);
            this.textBoxPastebin.Multiline = true;
            this.textBoxPastebin.Name = "textBoxPastebin";
            this.textBoxPastebin.Size = new System.Drawing.Size(225, 218);
            this.textBoxPastebin.TabIndex = 1;
            this.textBoxPastebin.WordWrap = false;
            this.textBoxPastebin.TextChanged += new System.EventHandler(this.TextBoxPastebin_TextChanged);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "warning.png");
            this.imageList1.Images.SetKeyName(1, "green.png");
            this.imageList1.Images.SetKeyName(2, "red.png");
            // 
            // tabControl1
            // 
            this.tabControl1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl1.ImageList = this.imageList1;
            this.tabControl1.Location = new System.Drawing.Point(3, 3);
            this.tabControl1.Multiline = true;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(835, 250);
            this.tabControl1.TabIndex = 24;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(21, 48);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(182, 20);
            this.button3.TabIndex = 36;
            this.button3.Text = "Pause after next dock";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.Button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(21, 74);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(182, 20);
            this.button4.TabIndex = 37;
            this.button4.Text = "Goto Jita";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.Button4_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(21, 162);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(182, 20);
            this.button5.TabIndex = 38;
            this.button5.Text = "Restart questor";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Visible = false;
            // 
            // tabControl2
            // 
            this.tabControl2.Controls.Add(this.tabPage1);
            this.tabControl2.Controls.Add(this.tabPage2);
            this.tabControl2.Controls.Add(this.tabPage3);
            this.tabControl2.Dock = System.Windows.Forms.DockStyle.Left;
            this.tabControl2.Location = new System.Drawing.Point(844, 3);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(239, 250);
            this.tabControl2.TabIndex = 36;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.Menu;
            this.tabPage1.Controls.Add(this.buttonCheckAccountLinks);
            this.tabPage1.Controls.Add(this.buttonGenNewBeginEnd);
            this.tabPage1.Controls.Add(this.buttonStopEveManger);
            this.tabPage1.Controls.Add(this.button1);
            this.tabPage1.Controls.Add(this.buttonStartEveManger);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(231, 224);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Control";
            // 
            // buttonCheckAccountLinks
            // 
            this.buttonCheckAccountLinks.Location = new System.Drawing.Point(18, 103);
            this.buttonCheckAccountLinks.Name = "buttonCheckAccountLinks";
            this.buttonCheckAccountLinks.Size = new System.Drawing.Size(182, 20);
            this.buttonCheckAccountLinks.TabIndex = 35;
            this.buttonCheckAccountLinks.Text = "Check account links";
            this.buttonCheckAccountLinks.UseVisualStyleBackColor = true;
            this.buttonCheckAccountLinks.Click += new System.EventHandler(this.ButtonCheckHWProfiles_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.button7);
            this.tabPage2.Controls.Add(this.button6);
            this.tabPage2.Controls.Add(this.button5);
            this.tabPage2.Controls.Add(this.button4);
            this.tabPage2.Controls.Add(this.button2);
            this.tabPage2.Controls.Add(this.button3);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(231, 224);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Global commands";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(21, 100);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(182, 20);
            this.button6.TabIndex = 39;
            this.button6.Text = "Delete cache next launch";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.Button6_Click);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.textBoxPastebin);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(231, 224);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Pastebin";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.BackColor = System.Drawing.SystemColors.Menu;
            this.flowLayoutPanel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.flowLayoutPanel1.Controls.Add(this.tabControl1);
            this.flowLayoutPanel1.Controls.Add(this.tabControl2);
            this.flowLayoutPanel1.Controls.Add(this.groupBox2);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 378);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1468, 294);
            this.flowLayoutPanel1.TabIndex = 37;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.SystemColors.Window;
            this.groupBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.groupBox1.Controls.Add(this.dataGridEveAccounts);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBox1.Location = new System.Drawing.Point(0, 24);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1468, 354);
            this.groupBox1.TabIndex = 38;
            this.groupBox1.TabStop = false;
            // 
            // dataGridEveAccounts
            // 
            this.dataGridEveAccounts.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridEveAccounts.BackgroundColor = System.Drawing.SystemColors.Menu;
            this.dataGridEveAccounts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridEveAccounts.ContextMenuStrip = this.contextMenuStrip1;
            this.dataGridEveAccounts.Dock = System.Windows.Forms.DockStyle.Top;
            this.dataGridEveAccounts.EnableHeadersVisualStyles = false;
            this.dataGridEveAccounts.GridColor = System.Drawing.SystemColors.Window;
            this.dataGridEveAccounts.Location = new System.Drawing.Point(3, 17);
            this.dataGridEveAccounts.MultiSelect = false;
            this.dataGridEveAccounts.Name = "dataGridEveAccounts";
            this.dataGridEveAccounts.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridEveAccounts.Size = new System.Drawing.Size(1462, 334);
            this.dataGridEveAccounts.TabIndex = 26;
            this.dataGridEveAccounts.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.DataGridEveAccounts_CellBeginEdit);
            this.dataGridEveAccounts.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridEveAccounts_CellEndEdit);
            this.dataGridEveAccounts.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.DataGridEveAccounts_CellFormatting);
            this.dataGridEveAccounts.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridEveAccounts_CellValueChanged);
            this.dataGridEveAccounts.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.DataGridEveAccounts_DataError);
            this.dataGridEveAccounts.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.DataGridEveAccounts_EditingControlShowing);
            this.dataGridEveAccounts.SelectionChanged += new System.EventHandler(this.DataGridEveAccounts_SelectionChanged);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 2000;
            this.timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(21, 126);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(182, 20);
            this.button7.TabIndex = 40;
            this.button7.Text = "Sync Static Fleet Info";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.ButtonStartUpdateLeaderAndSlaveStaticInfoClick);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(21, 126);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(182, 20);
            this.button7.TabIndex = 40;
            this.button7.Text = "Sync Static Fleet Info";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.ButtonStartUpdateLeaderAndSlaveStaticInfoClick);
            this.editPatternManagerSettingsToolStripMenuItem.Name = "editPatternManagerSettingsToolStripMenuItem";
            this.editPatternManagerSettingsToolStripMenuItem.Size = new System.Drawing.Size(230, 22);
            this.editPatternManagerSettingsToolStripMenuItem.Text = "Edit Pattern Manager Settings";
            this.editPatternManagerSettingsToolStripMenuItem.Click += new System.EventHandler(this.editPatternManagerSettingsToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ClientSize = new System.Drawing.Size(1468, 672);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "EVESharp - " + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).ToString();
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainFormFormClosed);
            this.Load += new System.EventHandler(this.MainFormLoad);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.Resize += new System.EventHandler(this.MainFormResize);
            this.contextMenuStrip1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tabControl2.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridEveAccounts)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

        private Label HookManagerCompileTimelbl;
        private Label SharedComponentsCompileTimelbl;
        private Label StartEveForTheseAccountsQueuelbl;
        public Label LastIPCIterationlbl;
        public Label LastScheduleIterationlbl;
        private Button button7;
        private ToolStripMenuItem editPatternManagerSettingsToolStripMenuItem;
    }
}
