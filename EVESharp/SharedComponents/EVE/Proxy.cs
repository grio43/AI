using EasyHook;
using SharedComponents.CurlUtil;
using SharedComponents.IPC;
using SharedComponents.Utility;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text.RegularExpressions;
using System.Threading;

namespace SharedComponents.EVE
{
    [Serializable]
    public class Proxy : ViewModelBase
    {
        #region Constructors

        public Proxy(string ip, string port, string username, string password, ConcurrentBindingList<Proxy> list)
        {
            Id = list.Count > 0 ? list.Max(p => p.Id) + 1 : 1;
            Ip = ip;
            Socks5Port = port;
            Username = username;
            Password = password;
            Description = Username + "@" + Ip;
        }

        public Proxy()
        {
        }

        #endregion Constructors

        #region Properties

        public string Description
        {
            get { return GetValue(() => Description); }
            set { SetValue(() => Description, value); }
        }

        [ReadOnly(true)]
        public string ExtIp
        {
            get { return GetValue(() => ExtIp); }
            set { SetValue(() => ExtIp, value); }
        }

        public string HttpProxyPort
        {
            get { return GetValue(() => HttpProxyPort); }
            set { SetValue(() => HttpProxyPort, value); }
        }

        [ReadOnly(true)]
        public int Id
        {
            get { return GetValue(() => Id); }
            set { SetValue(() => Id, value); }
        }

        public string Ip
        {
            get { return GetValue(() => Ip); }
            set { SetValue(() => Ip, value); }
        }

        [ReadOnly(true)]
        public bool IsAlive
        {
            get { return GetValue(() => IsAlive); }
            set { SetValue(() => IsAlive, value); }
        }

        public bool IsValid
        {
            get
            {
                try
                {
                    if (Description.Contains("local"))
                    {
                        return true;
                    }

                    if (!Regex.Match(Ip, @"\b(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\b").Success)
                        return false;

                    if (string.IsNullOrEmpty(Socks5Port))
                    {
                        Cache.Instance.Log("Proxy: Socks5Port [" + Socks5Port + "] is empty! IsValid [" + false + "]");
                        return false;
                    }

                    if (string.IsNullOrEmpty(HttpProxyPort))
                    {
                        Cache.Instance.Log("Proxy: HttpProxyPort [" + HttpProxyPort + "] is empty! IsValid [" + false + "]");
                        return false;
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log("Exception [" + ex + "]");
                    return true;
                }
            }
        }

        [ReadOnly(true)]
        public DateTime LastCheck
        {
            get { return GetValue(() => LastCheck); }
            set { SetValue(() => LastCheck, value); }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime LastEveAccountAuthenticationAttempt
        {
            get { return GetValue(() => LastEveAccountAuthenticationAttempt); }
            set { SetValue(() => LastEveAccountAuthenticationAttempt, value); }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public DateTime LastFail
        {
            get { return GetValue(() => LastFail); }
            set { SetValue(() => LastFail, value); }
        }

        [ReadOnly(true)]
        public string LinkedAccounts
        {
            get { return GetValue(() => LinkedAccounts); }
            set { SetValue(() => LinkedAccounts, value); }
        }

        [ReadOnly(true)]
        public string LinkedCharacters
        {
            get { return GetValue(() => LinkedCharacters); }
            set { SetValue(() => LinkedCharacters, value); }
        }

        public string Password
        {
            get { return GetValue(() => Password); }
            set { SetValue(() => Password, value); }
        }

        public string Socks5Port
        {
            get { return GetValue(() => Socks5Port); }
            set { SetValue(() => Socks5Port, value); }
        }

        public string Username
        {
            get { return GetValue(() => Username); }
            set { SetValue(() => Username, value); }
        }

        #endregion Properties

        #region Methods

        private DateTime LastInternetConnectivityCheck = DateTime.MinValue;

        private bool? _internetConnectivityTest = null;

        public bool InternetConnectivityTest(EveAccount eA = null)
        {
            if (eA == null)
                eA = new EveAccount("TestAccount", "TestChar", "TestPassword", DateTime.UtcNow, DateTime.UtcNow, "test@gmail.com", "password");

            if (DateTime.UtcNow > LastInternetConnectivityCheck.AddSeconds(6) || _internetConnectivityTest == null || !(bool)_internetConnectivityTest)
            {
                if (_internetConnectivityTest == null)
                {
                    //if (!CheckHttpProxyInternetConnectivity(eA))
                    //    _internetConnectivityTest = false;

                    if (!CheckSocks5InternetConnectivity(eA))
                        _internetConnectivityTest = false;

                    if (_internetConnectivityTest == null)
                        _internetConnectivityTest = true;

                    return _internetConnectivityTest ?? true;
                }

                return _internetConnectivityTest ?? false;
            }

            return _internetConnectivityTest ?? false;
        }

        public bool CheckSocks5InternetConnectivity(EveAccount eA = null)
        {
            if (eA == null)
                eA = new EveAccount("TestAccount", "TestChar", "TestPassword", DateTime.UtcNow, DateTime.UtcNow, "test@gmail.com", "password");

            if (Ip == "127.0.0.1" && Socks5Port == "0")
            {
                Cache.Instance.Log("[" + eA.MaskedAccountName + "][" + eA.MaskedCharacterName + "] Proxy IP [" + Ip + "] SOCKS5 Proxy Port [" + Socks5Port + "] means we should use the local connection (no proxy!)");
                return true;
            }

            if (string.IsNullOrEmpty(Ip) && !Description.Contains("local"))
            {
                Cache.Instance.Log("[" + eA.MaskedAccountName + "][" + eA.MaskedCharacterName + "] Proxy IP [" + Ip + "] is blank. Fill it in with a valid IP address where your SOCKS5 proxy is listening");
                return false;
            }

            if (!IsValid)
            {
                Cache.Instance.Log("[" + eA.MaskedAccountName + "][" + eA.MaskedCharacterName + "] Proxy IP [" + Ip + "] is not in the correct format. IsValid [false]");
                return false;
            }

            const bool areWeConnected = true;
            if (!areWeConnected)
            {
                Cache.Instance.Log("[" + eA.MaskedAccountName + "][" + eA.MaskedCharacterName + "] SOCKS5 Proxy [" + Description + "] Not Responding: Do you have the SOCKS5 server available on port [" + Socks5Port + "] of the host [" + Ip + "]? If using SSH and/or autossh is SSH connected?");
            }

            return areWeConnected;
        }

        public bool CheckHttpProxyInternetConnectivity(EveAccount eA = null)
        {
            if (eA == null)
                eA = new EveAccount("TestAccount", "TestChar", "TestPassword", DateTime.UtcNow, DateTime.UtcNow, "test@gmail.com", "password");

            if (Ip == "127.0.0.1" && HttpProxyPort == "0")
            {
                Cache.Instance.Log("[" + eA.MaskedAccountName + "][" + eA.MaskedCharacterName + "] Proxy IP [" + Ip + "] HTTP Proxy Port [" + HttpProxyPort + "] means we should use the local connection (no proxy!)");
                return true;
            }

            if (string.IsNullOrEmpty(Ip))
            {
                Cache.Instance.Log("[" + eA.MaskedAccountName + "][" + eA.MaskedCharacterName + "] Proxy IP [" + Ip + "] is blank. Fill it in with a valid IP address where your HTTP proxy is listening");
                return false;
            }

            if (!IsValid)
            {
                Cache.Instance.Log("[" + eA.MaskedAccountName + "][" + eA.MaskedCharacterName + "] Proxy IP [" + Ip + "] is not in the correct format. IsValid [" + IsValid + "]");
                return false;
            }

            //ToDo: this is not yet checking the connectivity through the proxy!
            Cache.Instance.Log("This HTTPS test is currently broken and needs TLC: FIXME");
            //const bool areWeConnected = true;
            //if (!areWeConnected) Cache.Instance.Log("[" + eA.AccountName + "][" + eA.MaskedCharacterName + "] Http Proxy [" + Description + "] Not Responding: Do you have the HTTP Proxy Server available on port [" + HttpProxyPort + "] of the host [" + Ip + "]? FYI: you can test this with FoxyProxy (Firefox addon) and add a HTTP Proxy with the appropriate IP/Port");
            return false;
        }

        public void Clear()
        {
            IsAlive = false;
            LastCheck = DateTime.MinValue;
            LastFail = DateTime.MinValue;
            ExtIp = string.Empty;
            LinkedAccounts = string.Empty;
            LinkedCharacters = string.Empty;
        }

        public string GetExternalIp(long timeout = 60L)
        {
            //TODO: Add ability to use HTTP proxy?!
            return GetExternalIpViaSOCKS5(timeout);
        }

        public string GetExternalIpViaSOCKS5(long timeout = 60L)
        {
            if (!IsValid)
                return string.Empty;

            using (CurlWorker cw = new CurlWorker())
            {
                return cw.GetPostPage("https://whoer.net/ip", string.Empty, GetSocks5IpPort(), GetUserPassword());
            }
        }

        public string GetExternalIpViaHTTPS()
        {
            //TODO: this needs testing and likely doesnt work right now
            if (!IsValid)
                return string.Empty;

            using (CurlWorker cw = new CurlWorker())
            {
                return cw.GetPostPage("https://whoer.net/ip", string.Empty, GetHttpProxyIpPort(), GetUserPassword());
            }
        }

        public string GetWgetStartParameter(string url = "whoer.net/ip")
        {
            string startParams = string.Empty;
            startParams += "\"-o c:\\eveoffline\\wget.log " + url;
            return startParams;
        }

        public string GetHashcode()
        {
            return GetSocks5IpPort() + "@" + GetUserPassword();
        }

        public string GetSocks5IpPort()
        {
            if (Description.Contains("local"))
            {
                return string.Empty;
            }

            return Ip + ":" + Socks5Port;
        }

        public string GetHttpProxyIpPort()
        {

            if (Description.Contains("local"))
            {
                return string.Empty;
            }

            return Ip + ":" + Socks5Port;
        }

        public string GetUserPassword()
        {

            if (Description.Contains("local"))
            {
                return string.Empty;
            }

            return Username + ":" + Password;
        }

        public void StartWGetInject(string url = "whoer.net/ip")
        {
            Cache.Instance.Log("Proxy: [ " + Description + " ][ " + Ip + ":" + Socks5Port + " ]");

            string[] args = { Id.ToString(), WCFServer.Instance.GetPipeName };
            int wgetprocessId1 = -1;

            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string injectionFile = Path.Combine(path, "DomainHandler.dll");
            string ChannelName = null;

            if (!File.Exists(injectionFile))
            {
                Cache.Instance.Log("[" + injectionFile + "] does not exist or correct file permissions are not set to allow this user to read it");
                return;
            }

            if (!File.Exists("C:\\admin\\wget.exe"))
            {
                Cache.Instance.Log("[ C:\\admin\\wget.exe ] does not exist or correct file permissions are not set to allow this user to read it");
                return;
            }

            if (!Directory.Exists("C:\\eveoffline"))
                Directory.CreateDirectory("C:\\eveoffline");

            try
            {
                RemoteHooking.IpcCreateServer<EVESharpInterface>(ref ChannelName, WellKnownObjectMode.SingleCall);
                Cache.Instance.Log("Proxy: Launching Wget [ C:\\admin\\wget.exe " + GetWgetStartParameter(url) + " ]");
                RemoteHooking.CreateAndInject("C:\\admin\\wget.exe", GetWgetStartParameter(url), (int)InjectionOptions.Default, injectionFile, injectionFile, out wgetprocessId1, ChannelName, args);

                if (wgetprocessId1 != -1 && wgetprocessId1 != 0)
                    Cache.Instance.Log("ProcessId: [" + wgetprocessId1 + "] Started Wget Successfully.");
                else
                    Cache.Instance.Log("ProcessId: [" + wgetprocessId1 + "] Error.");
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
            }
        }

        public DateTime LastEveSsoAttempt = DateTime.MinValue;

        [Browsable(false)]
        public bool SafeToAttemptEveSso
        {
            get
            {
                if (LastEveSsoAttempt.AddSeconds(15) > DateTime.UtcNow)
                    return false;

                return true;
            }
        }

        public void StartProxyScript(string ProxyStartupScriptPath, string ProxyDescription)
        {
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo(ProxyStartupScriptPath);
                Process.Start(processStartInfo);
                Cache.Instance.Log("Starting [" + ProxyStartupScriptPath + "] for Proxy [" + ProxyDescription + "]");
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
            }
        }

        #endregion Methods
    }
}