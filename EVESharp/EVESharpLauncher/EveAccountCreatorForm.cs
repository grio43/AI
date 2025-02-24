using SharedComponents.EVE;
using SharedComponents.BrowserAutomation;
using SharedComponents.BrowserAutomation.Curl;
//using SharedComponents.IMAP;
using SharedComponents.Socks5.Socks5Relay;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
//using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EVESharpLauncher
{
    public partial class EveAccountCreatorForm : Form
    {
        #region Fields

        private CancellationTokenSource cTokenSource;

        private KeyValuePair<EmailProvider, Tuple<string, string>> CurrentEmailProvider;
        private List<IDisposable> drivers = new List<IDisposable>();

        private Dictionary<EmailProvider, Tuple<string, string>> EmailProviders = new Dictionary<EmailProvider, Tuple<string, string>>()
        {
            {EmailProvider.Google, new Tuple<string, string>("imap.gmail.com", "@gmail.com")},
            {EmailProvider.Yandex, new Tuple<string, string>("imap.yandex.com", "@yandex.com")},
            {EmailProvider.Outlook, new Tuple<string, string>("outlook.office365.com", "@outlook.com")},
        };

        private List<Task<Tuple<bool, string, string, string>>> Tasks;

        #endregion Fields

        #region Constructors

        public EveAccountCreatorForm()
        {
            InitializeComponent();
            Tasks = new List<Task<Tuple<bool, string, string, string>>>();
            cTokenSource = new CancellationTokenSource();
            CurrentEmailProvider = EmailProviders.Last();
        }

        #endregion Constructors

        #region Enums

        private enum EmailProvider
        {
            Google,
            Yandex,
            Outlook
        }

        #endregion Enums

        #region Properties

        private bool IsIndicatingProgress { get; set; }

        #endregion Properties

        #region Methods

        private void button1_Click(object sender, EventArgs e)
        {
            StopAllTasks();
        }

        private void ButtonAbortEmailValidation_Click(object sender, EventArgs e)
        {
            StopAllTasks();
        }

        private void ButtonAddTrial_Click(object sender, EventArgs e)
        {
            cTokenSource = new CancellationTokenSource();
            var upDownValue = (int)numericUpDown1.Value;
            Tasks = new List<Task<Tuple<bool, string, string, string>>>();

            new Thread(() =>
            {
                try
                {
                    Invoke(new Action(() => buttonStartAlphaCreation.Enabled = false));

                    var prx = GetProxy();

                    if (prx == null)
                    {
                        Cache.Instance.Log("Have you selected a proxy?! [ prx == null ]");
                        return;
                    }

                    if (!prx.IsValid)
                    {
                        Cache.Instance.Log("Is the selected proxy properly setup? [ !prx.IsValid ]");
                        return;
                    }

                    if (!prx.CheckSocks5InternetConnectivity())
                    {
                        Cache.Instance.Log("Internet connectivity seems to be unavailable through the proxy.");
                        return;
                    }

                    RunProgressbar();

                    var eveAccounts = new List<Tuple<string, string, string>>();

                    for (var i = 0; i < upDownValue; i++)
                    {
                        var eveUsername = UserPassGen.Instance.GenerateUsername();
                        var evePassword = UserPassGen.Instance.GeneratePassword();
                        var n = i;
                        var t =
                            Task.Run(
                                async () =>
                                {
                                    return await new EveAccountCreatorImpl(n).CreateEveAlphaAccountAndValidate(string.Empty, eveUsername, evePassword,
                                        prx.GetSocks5IpPort(), prx.GetUserPassword(), cTokenSource.Token);
                                }, cTokenSource.Token);

                        Tasks.Add(t);
                    }

                    foreach (var task in Tasks)
                        try
                        {
                            task.Wait();
                        }
                        catch (AggregateException)
                        {
                            continue;
                        }

                    foreach (var task in Tasks)
                    {
                        if (task.Exception != null)
                            continue;
                        if (task.Result != null && task.Result.Item1)
                            eveAccounts.Add(new Tuple<string, string, string>(task.Result.Item2, task.Result.Item3, task.Result.Item4));
                    }

                    foreach (var a in eveAccounts)
                    {
                        var eA = new EveAccount(a.Item1, a.Item1, a.Item2, DateTime.UtcNow, DateTime.UtcNow, "NotMyEmailAddress@Temp.local", "NotMyEmailPassword");
                        eA.HWSettings = new HWSettings();
                        eA.HWSettings.GenerateRandomProfile();
                        eA.HWSettings.Proxy = prx;
                        eA.Email = a.Item3;
                        Invoke(new Action(() => Cache.Instance.EveAccountSerializeableSortableBindingList.List.Add(eA)));
                    }
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log(ex.ToString());
                    Debug.WriteLine(ex.ToString());
                }
                finally
                {
                    try
                    {
                        IsIndicatingProgress = false;
                        Invoke(new Action(() => buttonStartAlphaCreation.Enabled = true));
                    }
                    catch (Exception exception)
                    {
                        Cache.Instance.Log(exception.ToString());
                        Debug.WriteLine(exception);
                    }
                }
            }).Start();
        }

        private void ButtonCreateEmailAccount_Click(object sender, EventArgs e)
        {
            var myProxy = GetProxy();
            if (myProxy == null)
            {
                Cache.Instance.Log("Select a proxy.");
                return;
            }

            if (!myProxy.CheckSocks5InternetConnectivity())
            {
                Cache.Instance.Log("Internet connectivity seems to be unavailable through the proxy.");
                return;
            }

            switch (CurrentEmailProvider.Key)
            {
                case EmailProvider.Google:
                    break;

                case EmailProvider.Yandex:
                    //var yandexImpl = new YandexCurl();
                    //yandexImpl.CreateYandexEmail(textBoxEmailAddress.Text, textBoxEmailPassword.Text, GetProxy());
                    break;

                case EmailProvider.Outlook:
                    var seleniumAutomationImpl = new SeleniumAutomation();
                    drivers.Add(seleniumAutomationImpl);
                    new Task(() =>
                    {
                        try
                        {
                            seleniumAutomationImpl.CreateOutlookEmail(textBoxEmailAddress.Text.Split('@')[0], textBoxEmailPassword.Text, myProxy);
                        }
                        catch (Exception ex)
                        {
                            Cache.Instance.Log("Exception: " + ex);
                        }
                    }).Start();
                    break;
            }
        }

        private void ButtonCreateEveAccount_Click(object sender, EventArgs e)
        {
            if (GetProxy() != null)
            {
                Proxy myProxy = GetProxy();
                if (!myProxy.CheckSocks5InternetConnectivity() || !GetProxy().IsValid)
                {
                    Cache.Instance.Log("Internet connectivity seems to be unavailable through the proxy.");
                    return;
                }

                var seleniumAutomationImpl = new SeleniumAutomation();
                drivers.Add(seleniumAutomationImpl);
                new Task(() =>
                {
                    try
                    {
                        if (string.IsNullOrEmpty(textBoxEveAccountName.Text))
                        {
                            Cache.Instance.Log("Eve account name canot be empty. aborting");
                            return;
                        }

                        if (string.IsNullOrEmpty(textBoxEvePassword.Text))
                        {
                            Cache.Instance.Log("Eve account name canot be empty. aborting");
                        }

                        seleniumAutomationImpl.CreateEveAccount(textBoxEveAccountName.Text.ToLower(), textBoxEvePassword.Text, textBoxEmailAddress.Text.ToLower(), myProxy);
                    }
                    catch (Exception ex)
                    {
                        Cache.Instance.Log("Exception: " + ex);
                    }
                }).Start();
            }
        }

        private void ButtonGenerateRandom_Click(object sender, EventArgs e)
        {
            GenerateRandom();
        }

        private void ButtonOpenBrowser_Click(object sender, EventArgs e)
        {
            var p = (Proxy)comboBoxProxies.SelectedItem;
            if (p == null)
            {
                Cache.Instance.Log("Proxy == null. Please select a proxy from the list. Aborting");
                return;
            }

            var seleniumAutomationImpl = new SeleniumAutomation();
            drivers.Add(seleniumAutomationImpl);
            new Task(() =>
            {
                try
                {
                    string myURL = "https://www.google.com/search?q=what+is+my+ip";
                    seleniumAutomationImpl.OpenSeleniumChromeBrowser(seleniumAutomationImpl.MyHTTPProxyChromeOptions(p), myURL);
                    while (Process.GetProcesses().Any(x => x.ProcessName.ToLower().Contains("geckodriver")))
                        Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log("Exception: " + ex);
                }
            }).Start();
        }

        private void AddNewAccount(Proxy myProxy)
        {
            Cache.Instance.Log($"Added account {textBoxEveAccountName.Text}");
            EveAccount eA = new EveAccount(textBoxEveAccountName.Text,
                textBoxEveAccountName.Text,
                textBoxEvePassword.Text,
                DateTime.UtcNow,
                DateTime.UtcNow,
                textBoxEmailAddress.Text,
                textBoxEmailPassword.Text);
            eA.HWSettings = new HWSettings();
            eA.HWSettings.GenerateRandomProfile();
            eA.HWSettings.Proxy = myProxy;
            eA.Email = textBoxEmailAddress.Text;
            eA.IMAPHost = textBoxIMAPHost.Text;
            Cache.Instance.EveAccountSerializeableSortableBindingList.List.Add(eA);
            Cache.Instance.EveAccountSerializeableSortableBindingList.List.XmlSerialize(Cache.Instance.EveAccountSerializeableSortableBindingList.FilePathName);
        }

        /**
        private void ButtonValidateEveAccount_Click(object sender, EventArgs e)
        {
            Proxy myProxy = GetProxy();

            if (myProxy == null)
            {
                Cache.Instance.Log("No proxy was selected.");
                return;
            }

            if (myProxy != null)
                if (!GetProxy().CheckSocks5InternetConnectivity() || !GetProxy().IsValid)
                {
                    Cache.Instance.Log("Internet connectivity seems to be unavailable through the proxy.");
                    return;
                }

            cTokenSource = new CancellationTokenSource();

            new Thread(() =>
            {
                try
                {
                    var seleniumAutomationImpl = new SeleniumAutomation();
                    drivers.Add(seleniumAutomationImpl);

                    Invoke(new Action(() => buttonValidateEveAccount.Enabled = false));

                    RunProgressbar();

                    Task t = new Task(() =>
                    {
                        if (seleniumAutomationImpl.ValidateEmailFromCCP(textBoxIMAPHost.Text, 993, myProxy, textBoxEmailAddress.Text, textBoxEmailPassword.Text, cTokenSource))
                            AddNewAccount(myProxy);
                    });

                    t.Start();
                    t.Wait();
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log(ex.ToString());
                    Debug.WriteLine(ex.ToString());
                }
                finally
                {
                    try
                    {
                        IsIndicatingProgress = false;
                        Invoke(new Action(() => buttonValidateEveAccount.Enabled = true));
                    }
                    catch (Exception exception)
                    {
                        Cache.Instance.Log(exception.ToString());
                        Debug.WriteLine(exception);
                    }
                }
            }).Start();
        }
        **/
        private void ComboBoxProxies_SelectedIndexChanged(object sender, EventArgs e)
        {
            var p = GetProxy();
            if (p != null)
                try
                {
                    var s = p.GetUserPassword() + "@" + p.GetSocks5IpPort();
                    Debug.WriteLine("Relaying args: " + s);
                    //DsocksHandler.StartChain(new string[] { s });
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log(ex.ToString());
                }
        }

        private void EveAccountCreatorForm_FormClosed(object sender, FormClosedEventArgs e)
        {
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
        }

        private void EveAccountCreatorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopAllTasks();
            //DsocksHandler.Dispose();
            SeleniumAutomation.KillChromeGeckoDrivers();
        }

        private void EveAccountCreatorForm_Load(object sender, EventArgs e)
        {
            SeleniumAutomation.KillChromeGeckoDrivers();

            comboBoxProxies.DataSource = Cache.Instance.EveSettings.Proxies;
            comboBoxProxies.DisplayMember = "Description";
            comboBoxProxies.SelectedIndex = -1;
        }

        private void EveAccountCreatorForm_Shown(object sender, EventArgs e)
        {
            GenerateRandom();
            comboBoxProxies.Select();
        }

        private void GenerateRandom()
        {
            string UserName = UserPassGen.Instance.GenerateUsername().ToLower();
            string EmailAddress = UserName + CurrentEmailProvider.Value.Item2;
            string EmailPassword = UserPassGen.Instance.GeneratePassword();
            string EveUsername = UserName;
            string EvePassword = EmailPassword;

            textBoxEmailAddress.Text = EmailAddress.ToLower();
            textBoxEmailPassword.Text = EmailPassword;
            textBoxEveAccountName.Text = EveUsername;
            textBoxEvePassword.Text = EvePassword;

            Cache.Instance.Log("-------------------------------------------------------------");
            Cache.Instance.Log("-------------------------------------------------------------");
            Cache.Instance.Log("GenerateRandom: EmailAddress: [" + EmailAddress + "]");
            Cache.Instance.Log("GenerateRandom: EmailPassword: [" + EmailPassword + "]");
            Cache.Instance.Log("GenerateRandom: EveUsername: [" + EveUsername + "]");
            Cache.Instance.Log("GenerateRandom: EvePassword: [" + EvePassword + "]");
            Cache.Instance.Log("-------------------------------------------------------------");
            Cache.Instance.Log("-------------------------------------------------------------");
        }

        private Proxy GetProxy()
        {
            return (Proxy)comboBoxProxies.SelectedItem;
        }

        private void RadioButtonGmail_CheckedChanged(object sender, EventArgs e)
        {
            CurrentEmailProvider = EmailProviders.First(em => em.Key == EmailProvider.Google);
            textBoxIMAPHost.Text = CurrentEmailProvider.Value.Item1;
            GenerateRandom();
        }

        private void RadioButtonOutlook_CheckedChanged(object sender, EventArgs e)
        {
            CurrentEmailProvider = EmailProviders.First(em => em.Key == EmailProvider.Outlook);
            textBoxIMAPHost.Text = CurrentEmailProvider.Value.Item1;
            GenerateRandom();
        }

        private void RadioButtonYandex_CheckedChanged(object sender, EventArgs e)
        {
            CurrentEmailProvider = EmailProviders.First(em => em.Key == EmailProvider.Yandex);
            textBoxIMAPHost.Text = CurrentEmailProvider.Value.Item1;
            GenerateRandom();
        }

        private void RunProgressbar()
        {
            if (IsIndicatingProgress)
                return;

            IsIndicatingProgress = true;
            new Thread(() =>
            {
                while (IsIndicatingProgress)
                {
                    Thread.Sleep(1000);
                    try
                    {
                        progressBar1.Invoke(new Action(() =>
                        {
                            if (progressBar1.Value == progressBar1.Maximum)
                                progressBar1.Value = 0;
                            progressBar1.PerformStep();
                        }));
                    }
                    catch (Exception)
                    {
                        //ignore this exception
                    }
                }

                progressBar1.Invoke(new Action(() => { progressBar1.Value = 0; }));
            }).Start();
        }

        private void StopAllTasks()
        {
            Cache.Instance.Log("Cancellation of all tasks requested.");
            cTokenSource.Cancel();
        }

        #endregion Methods

        private void ButtonCreateEveAccountManually_Click(object sender, EventArgs e)
        {
            Proxy myProxy = (Proxy)comboBoxProxies.SelectedItem;
            if (myProxy == null)
            {
                Cache.Instance.Log("Proxy == null. Please select a proxy from the list. Aborting");
                return;
            }

            var seleniumAutomationImpl = new SeleniumAutomation();
            drivers.Add(seleniumAutomationImpl);
            new Task(() =>
            {
                try
                {
                    string myURL = "https://secure.eveonline.com/signup/";
                    seleniumAutomationImpl.OpenSeleniumChromeBrowser(seleniumAutomationImpl.MyHTTPProxyChromeOptions(myProxy), myURL);
                    while (Process.GetProcesses().Any(x => x.ProcessName.ToLower().Contains("geckodriver")))
                        Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log("Exception: " + ex);
                }
            }).Start();
        }

        private void ButtonCreateEmailAlias_Click(object sender, EventArgs e)
        {
            Proxy myProxy = (Proxy)comboBoxProxies.SelectedItem;
            if (myProxy == null)
            {
                Cache.Instance.Log("Proxy == null. Please select a proxy from the list. Aborting");
                return;
            }

            var seleniumAutomationImpl = new SeleniumAutomation();
            drivers.Add(seleniumAutomationImpl);
            new Task(() =>
            {
                try
                {
                    //ToDo: Impliment
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log("Exception: " + ex);
                }
            }).Start();
        }

        private void ButtonCreateEmailAccountManually_Click(object sender, EventArgs e)
        {
            var myProxy = GetProxy();
            if (myProxy == null)
            {
                Cache.Instance.Log("Select a proxy.");
                return;
            }

            if (!myProxy.CheckSocks5InternetConnectivity())
            {
                Cache.Instance.Log("Internet connectivity seems to be unavailable through the proxy.");
                return;
            }

            switch (CurrentEmailProvider.Key)
            {
                case EmailProvider.Google:
                    break;

                case EmailProvider.Yandex:
                    var yandexImpl = new YandexCurl();
                    yandexImpl.CreateYandexEmail(textBoxEmailAddress.Text, textBoxEmailPassword.Text, GetProxy());
                    break;

                case EmailProvider.Outlook:
                    var seleniumAutomationImpl = new SeleniumAutomation();
                    drivers.Add(seleniumAutomationImpl);
                    new Task(() =>
                    {
                        try
                        {
                            string myURL = "https://signup.live.com/signup.aspx";
                            seleniumAutomationImpl.OpenSeleniumChromeBrowser(seleniumAutomationImpl.MyHTTPProxyChromeOptions(myProxy), myURL);
                            while (Process.GetProcesses().Any(x => x.ProcessName.ToLower().Contains("geckodriver")))
                                Thread.Sleep(100);
                        }
                        catch (Exception ex)
                        {
                            Cache.Instance.Log("Exception: " + ex);
                        }
                    }).Start();
                    break;
            }
        }

        private void ButtonOpenEmail_Click(object sender, EventArgs e)
        {
            Cache.Instance.Log("User Clicked on [ OpenEmailUsingProxy ]");

            var myProxy = GetProxy();
            if (myProxy == null)
            {
                Cache.Instance.Log("Select a proxy.");
                return;
            }

            if (string.IsNullOrEmpty(textBoxEmailAddress.Text))
            {
                Cache.Instance.Log("Email address is blank!");
                return;
            }

            if (string.IsNullOrEmpty(textBoxEmailPassword.Text))
            {
                Cache.Instance.Log("Email password is blank!");
                return;
            }

            var seleniumAutomationImpl = new SeleniumAutomation();
            drivers.Add(seleniumAutomationImpl);
            seleniumAutomationImpl.CreateTaskToOpenEmail(textBoxEmailAddress.Text, textBoxEmailPassword.Text, myProxy);
        }

        private void ButtonCreateLauncherEveAccountEntry_Click(object sender, EventArgs e)
        {
            var myProxy = GetProxy();
            if (myProxy == null)
            {
                Cache.Instance.Log("Select a proxy.");
                return;
            }

            if (string.IsNullOrEmpty(textBoxEmailAddress.Text))
            {
                Cache.Instance.Log("Email address is blank!");
                return;
            }

            if (string.IsNullOrEmpty(textBoxEmailPassword.Text))
            {
                Cache.Instance.Log("Email password is blank!");
                return;
            }

            if (string.IsNullOrEmpty(textBoxEveAccountName.Text))
            {
                Cache.Instance.Log("Eve account name canot be empty. aborting");
                return;
            }

            if (string.IsNullOrEmpty(textBoxEvePassword.Text))
            {
                Cache.Instance.Log("Eve account name canot be empty. aborting");
            }

            AddNewAccount(myProxy);
        }
    }
}