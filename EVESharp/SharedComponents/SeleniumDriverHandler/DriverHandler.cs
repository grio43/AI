using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumUndetectedChromeDriver;
using SharedComponents.EVE;
using SharedComponents.Socks5.Socks5Relay.SocksRelay;
using Tor.Proxy;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using WebDriverManager.Helpers;

namespace SharedComponents.SeleniumDriverHandler
{
    public class DriverHandler : IDisposable
    {
        private SocksRelay _socksRelay;
        private UndetectedChromeDriver _driver;
        private const string IpAddressUrl = @"http://whoer.net/ip";
        private static List<UndetectedChromeDriver> _allDrivers = new List<UndetectedChromeDriver>();

        public EveAccount EA { get; }
        public bool Headless { get; }

        public DriverHandler(EveAccount eA, bool headless = false)
        {
            EA = eA;
            Headless = headless;
            Initialize();
        }

        public static void DisposeAllDrivers()
        {
            foreach (var driver in _allDrivers)
            {
                try
                {
                    driver.Quit();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        public MailInboxHandler MailInboxHandler => new MailInboxHandler(EA, this);

        public UndetectedChromeDriver GetDriver() => _driver;

        public void WaitForPage()
        {
            Debug.WriteLine("Wait for page...");
            new WebDriverWait(_driver, TimeSpan.FromSeconds(60)).Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
            Debug.WriteLine("Wait for page finished.");
        }

        public void WaitForElement(Action a, int timeout = 8)
        {
            var to = DateTime.UtcNow.AddSeconds(timeout);
            while (true)
            {

                if (to < DateTime.UtcNow)
                    break;

                try
                {
                    a();
                }
                catch (StaleElementReferenceException ex)
                {
                    continue;
                }
                catch (ElementNotInteractableException ex)
                {
                    continue;
                }
                catch (NoSuchElementException)
                {
                    continue;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    break;
                }
                break;
            }
        }

        private async void Initialize()
        {

            _socksRelay = new SocksRelay(
                EA.HWSettings.Proxy.Ip,
                ushort.Parse(EA.HWSettings.Proxy.Port),
                EA.HWSettings.Proxy.Username,
                EA.HWSettings.Proxy.Password);

            var socksPort = _socksRelay.StartListening().Result;

            Debug.WriteLine($"SocksRelay listening on [{socksPort}]");

            var unauthenticatedProxyAddress = $"socks5://{"127.0.0.1"}:{socksPort}";

            if (String.IsNullOrEmpty(EA.HWSettings.Proxy.Username) && String.IsNullOrEmpty(EA.HWSettings.Proxy.Password))
            {
                unauthenticatedProxyAddress = $"socks5://{EA.HWSettings.Proxy.Ip}:{EA.HWSettings.Proxy.Port}";
            }

            var cfg = new ChromeConfig();
            var driverManager = new DriverManager();
            var driverPath = driverManager.SetUpDriver(
                cfg,
                VersionResolveStrategy.MatchingBrowser);

            Debug.WriteLine($"ChromeDriverPath [{driverPath}]");

            var options = new ChromeOptions();
            if (Headless)
                options.AddArgument(@"--window-position=""20000,20000""");
            options.AddArguments("--window-size=1920,1080");
            options.AddArguments("--start-maximized");
            options.AddArguments("--lang=en");
            options.AddArguments("--incognito");
            options.AddArguments($"--proxy-server={unauthenticatedProxyAddress}");
            _driver = UndetectedChromeDriver.Create(
                driverExecutablePath: driverPath, hideCommandPromptWindow: true, suppressWelcome: true,
                options: options);

            //_driver = UndetectedChromeDriver.Create(
            //    driverExecutablePath: driverPath, headless: Headless, hideCommandPromptWindow: true, suppressWelcome: true,
            //    options: options);

            _allDrivers.Add(_driver);

            _ = Task.Run(async () =>
            {
                try
                {
                    var check = await CheckIpAddress();
                    if (!check)
                    {
                        throw new Exception("Error: Proxy not valid.");
                    }
                    else
                    {
                        var msg = $"Proxy [{EA.HWSettings.Proxy.Ip}] is working and the IP was verified.";
                        Debug.WriteLine(msg);
                        Cache.Instance.Log(msg);
                    }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    Cache.Instance.Log(ex.ToString());
                }

            });
        }

        private async Task<bool> CheckIpAddress()
        {
            _driver.GoToUrl(IpAddressUrl);
            var proxyIp = _driver.FindElement(By.TagName("body")).Text.Trim();
            return proxyIp == EA.HWSettings.Proxy.Ip;
        }

        public void Close()
        {
            _driver.Quit();
            _socksRelay.Dispose();
        }

        public bool IsAlive()
        {
            try
            {

                Debug.WriteLine($"DriverWindowHandleCount [{_driver.WindowHandles.Count}]");
                return _driver.WindowHandles.Count > 0;
            }
            catch (Exception)
            {

                return false;
            }
        }
        public void Dispose()
        {
            Close();
        }
    }
}
