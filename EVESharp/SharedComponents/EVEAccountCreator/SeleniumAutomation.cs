using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;
using SharedComponents.EVE;
using SharedComponents.Utility;
using SharedComponents.Web;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace SharedComponents.EVEAccountCreator
{
    public class SeleniumAutomation : IDisposable
    {
        #region Fields

        private volatile ChromeDriver driver;

        #endregion Fields

        #region IDisposable

        #region Methods

        public static void KillGeckoDrivers()
        {
            try
            {
                foreach (Process p in Process.GetProcesses().Where(x => x.ProcessName.ToLower().Contains("geckodriver")))
                {
                    if (p != null)
                        Util.TaskKill(p.Id, true);
                    Thread.Sleep(1000);
                }

                while (Process.GetProcesses().Any(x => x.ProcessName.ToLower().Contains("geckodriver")))
                    Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
            }
        }

        public void CreateOutlookEmail(string username, string password, EVE.Proxy myProxy)
        {
            KillGeckoDrivers();

            try
            {
                const string myURL = "https://signup.live.com/signup.aspx";
                OpenSeleniumBrowser(MySocks5ChromeOptions(myProxy), myURL);

                Random rnd = new Random();
                string firstName = UserPassGen.Instance.GenerateFirstname();
                string lastName = UserPassGen.Instance.GenerateFirstname();

                string emailAddress = username + "@outlook.com";
                Cache.Instance.Log("Selenium: EmailAddress will be entered as: [" + emailAddress + "]");
                WaitForElementPresentAndEnabled(new ByIdOrName("MemberName")).SendKeys(emailAddress);
                Cache.Instance.Log("Selenium: done entering EmailAddress");

                Cache.Instance.Log("Selenium: Next Button");
                WaitForElementPresentAndEnabled(new ByIdOrName("iSignupAction")).Submit();
                Cache.Instance.Log("Selenium: Done Pressing Next Button");

                Cache.Instance.Log("Selenium: password will be entered as: [" + password + "]");
                WaitForElementPresentAndEnabled(new ByIdOrName("PasswordInput")).SendKeys(password);
                Cache.Instance.Log("Selenium: done entering password");

                Cache.Instance.Log("Selenium: Next Button");
                WaitForElementPresentAndEnabled(new ByIdOrName("iSignupAction")).Submit();
                Cache.Instance.Log("Selenium: Done Pressing Next Button");

                Cache.Instance.Log("Selenium: FirstName will be entered as: [" + firstName + "]");
                WaitForElementPresentAndEnabled(new ByIdOrName("FirstName")).SendKeys(firstName);
                Cache.Instance.Log("Selenium: done entering FirstName");

                Cache.Instance.Log("Selenium: Lastname will be entered as: [" + lastName + "]");
                WaitForElementPresentAndEnabled(new ByIdOrName("LastName")).SendKeys(lastName);
                Cache.Instance.Log("Selenium: done entering LastName");

                Cache.Instance.Log("Selenium: Next Button");
                WaitForElementPresentAndEnabled(new ByIdOrName("iSignupAction")).Submit();
                Cache.Instance.Log("Selenium: Done Pressing Next Button");

                SelectElement birthDaySelect = new SelectElement(WaitForElementPresentAndEnabled(new ByIdOrName("BirthDay")));
                birthDaySelect.SelectByIndex(rnd.Next(1, birthDaySelect.Options.Count - 1));
                SelectElement birthMonthSelect = new SelectElement(WaitForElementPresentAndEnabled(new ByIdOrName("BirthMonth")));
                birthMonthSelect.SelectByIndex(rnd.Next(1, birthMonthSelect.Options.Count - 1));
                SelectElement birthYearSelect = new SelectElement(WaitForElementPresentAndEnabled(new ByIdOrName("BirthYear")));
                birthYearSelect.SelectByIndex(rnd.Next(30, birthYearSelect.Options.Count - 1));
                //
                //todo: save birthday info in case we need it later!
                //

                Cache.Instance.Log("Selenium: Next Button");
                WaitForElementPresentAndEnabled(new ByIdOrName("iSignupAction")).Submit();
                Cache.Instance.Log("Selenium: Done Pressing Next Button");
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception: " + ex);
            }
        }

        private void LogIntoWebOutlookEmail(string myEmailAddress, string myEmailPassword, EVE.Proxy myProxy)
        {
            KillGeckoDrivers();

            try
            {
                const string myURL = "https://outlook.live.com/owa/?nlp=1";
                OpenSeleniumBrowser(MySocks5ChromeOptions(myProxy), myURL);

                Cache.Instance.Log("Selenium: EmailAddress will be entered as: [" + myEmailAddress + "]");
                WaitForElementPresentAndEnabled(new ByIdOrName("loginfmt")).SendKeys(myEmailAddress);
                Cache.Instance.Log("Selenium: done entering EmailAddress");

                Cache.Instance.Log("Selenium: Next Button");
                WaitForElementPresentAndEnabled(new ByIdOrName("idSIButton9")).Click();
                Cache.Instance.Log("Selenium: Done Pressing Next Button");

                Cache.Instance.Log("Selenium: password will be entered as: [" + myEmailPassword + "]");
                WaitForElementPresentAndEnabled(new ByIdOrName("passwd")).SendKeys(myEmailPassword);
                Cache.Instance.Log("Selenium: done entering password");

                Cache.Instance.Log("Selenium: Next Button");
                WaitForElementPresentAndEnabled(new ByIdOrName("idSIButton9")).Click();
                Cache.Instance.Log("Selenium: Done Pressing Next Button");

                Cache.Instance.Log("Selenium: Done");
                while (Process.GetProcesses().Any(x => x.ProcessName.ToLower().Contains("geckodriver")))
                    Thread.Sleep(50);
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception: " + ex);
            }
        }

        private void LogIntoEveAccountWebSite(string myEveUserName, string myEvePassword, EVE.Proxy myProxy)
        {
            KillGeckoDrivers();

            try
            {
                const string myURL = "https://secure.eveonline.com/login";
                OpenSeleniumBrowser(MySocks5ChromeOptions(myProxy), myURL);

                Cache.Instance.Log("Selenium: Press Log In Button");
                WaitForElementPresentAndEnabled(new ByIdOrName("ctl00_header_loginwidget_LoginLinkButton")).Click();
                Cache.Instance.Log("Selenium: Done Pressing Log In Button");

                Cache.Instance.Log("Selenium: Eve UserName will be entered as: [" + myEveUserName + "]");
                WaitForElementPresentAndEnabled(new ByIdOrName("UserName")).SendKeys(myEveUserName);
                Cache.Instance.Log("Selenium: done entering Eve UserName");

                Cache.Instance.Log("Selenium: password will be entered as: [" + myEvePassword + "]");
                WaitForElementPresentAndEnabled(new ByIdOrName("Password")).SendKeys(myEvePassword);
                Cache.Instance.Log("Selenium: done entering password");

                Cache.Instance.Log("Selenium: Submit Button");
                WaitForElementPresentAndEnabled(new ByIdOrName("submitButton")).Click();
                Cache.Instance.Log("Selenium: Done Pressing Submit Button");

                Cache.Instance.Log("Selenium: Done");
                while (Process.GetProcesses().Any(x => x.ProcessName.ToLower().Contains("geckodriver")))
                    Thread.Sleep(50);
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception: " + ex);
            }
        }

        /**
        private LoginResult LogIntoEveAccountWebSiteForSSOToken(EveAccount myEveAccount, bool sisi, out IsbelEveAccount.Token accessToken)
        {
            KillGeckoDrivers();
            Uri myLoginUrl = null;

            try
            {
                IsbelEveAccount.Token checkToken = sisi ? myEveAccount.SisiToken : myEveAccount.TranquilityToken;
                if (checkToken != null && !checkToken.IsExpired)
                {
                    accessToken = checkToken;
                    myEveAccount.EveAccessToken = accessToken.TokenString;
                    myEveAccount.EveAccessTokenValidUntil = accessToken.Expiration;
                    return LoginResult.Success;
                }

                if (!string.IsNullOrEmpty(myEveAccount.EveAccessToken) && myEveAccount.EveAccessTokenValidUntil != null && myEveAccount.EveAccessTokenValidUntil >= DateTime.UtcNow)
                {
                    accessToken = new IsbelEveAccount.Token();
                    accessToken.TokenString = myEveAccount.EveAccessToken;
                    accessToken.Expiration = myEveAccount.EveAccessTokenValidUntil;
                    return LoginResult.Success;
                }

                // need SecurePassword.

                System.Security.SecureString SecurePassword = new System.Security.SecureString();
                foreach (char c in myEveAccount.Password)
                {
                    SecurePassword.AppendChar(c);
                }
                SecurePassword.MakeReadOnly();

                if (SecurePassword.Length == 0)
                {
                    // password is required, sorry dude
                    accessToken = null;
                    return LoginResult.InvalidUsernameOrPassword;
                }

                myLoginUrl = RequestResponse.GetLoginUri(sisi, myEveAccount.myIsbelEveAccount.State.ToString(), myEveAccount.myIsbelEveAccount.ChallengeHash);

                string RequestVerificationToken = string.Empty;
                //GetRequestVerificationToken(myLoginUrl, sisi, proxyIp, proxyHttpPort, out RequestVerificationToken);
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception: " + ex);
                accessToken = null;
            }

            try
            {
                Cache.Instance.Log("Selenium: Open WebPage: " + myLoginUrl.AbsoluteUri);

                OpenSeleniumBrowser(MySocks5ChromeOptions(myEveAccount.HWSettings.Proxy), myLoginUrl.AbsoluteUri);
                accessToken = null;

                Cache.Instance.Log("Selenium: Press Log In Button");
                //WaitForElementPresentAndEnabled(new ByIdOrName("ctl00_header_loginwidget_LoginLinkButton")).Click();
                //Cache.Instance.Log("Selenium: Done Pressing Log In Button");

                Cache.Instance.Log("Selenium: Eve UserName will be entered as: [" + myEveAccount.AccountName + "]");
                WaitForElementPresentAndEnabled(new ByIdOrName("UserName")).SendKeys(myEveAccount.AccountName);
                Cache.Instance.Log("Selenium: done entering Eve UserName");

                Cache.Instance.Log("Selenium: password will be entered as: [" + myEveAccount.MaskedPassword + "] (Masked)");
                WaitForElementPresentAndEnabled(new ByIdOrName("Password")).SendKeys(myEveAccount.Password);
                Cache.Instance.Log("Selenium: done entering password");

                Cache.Instance.Log("Selenium: Submit Button");
                WaitForElementPresentAndEnabled(new ByIdOrName("submitButton")).Click();
                Cache.Instance.Log("Selenium: Done Pressing Submit Button");

                Cache.Instance.Log("Selenium: Done");
                while (Process.GetProcesses().Any(x => x.ProcessName.ToLower().Contains("geckodriver")))
                    Thread.Sleep(50);
                return LoginResult.Error;
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception: " + ex);
                accessToken = null;
                return LoginResult.Error;
            }
        }
        **/

        public string GetPostPage(string url, string postData, string proxyPort, string userPassword, bool followLocation = true, bool includeHeader = false)
        {
            return String.Empty;
        }

        public void CreateEveAccount(string EveUsername, string EvePassword, string EmailAddress, EVE.Proxy myProxy)
        {
            KillGeckoDrivers();
            Random rnd = new Random();

            try
            {
                const string myURL = "https://secure.eveonline.com/signup/";
                OpenSeleniumBrowser(MySocks5ChromeOptions(myProxy), myURL);

                Cache.Instance.Log("Selenium: EmailAddress will be entered as: [" + EmailAddress + "]");
                WaitForElementPresentAndEnabled(new ByIdOrName("ctl00_ContentPlaceHolder1_ccpTrialSignupInfo_emailInput_validatedtextbox")).SendKeys(EmailAddress);
                Cache.Instance.Log("Selenium: done entering EmailAddress");

                Thread.Sleep(rnd.Next(1137, 2256));

                Cache.Instance.Log("Selenium: EveUsername will be entered as: [" + EveUsername + "]");
                WaitForElementPresentAndEnabled(new ByIdOrName("ctl00_ContentPlaceHolder1_ccpTrialSignupInfo_ctrlUsernamePassword_txtUserName_validatedtextbox")).SendKeys(EveUsername);
                Cache.Instance.Log("Selenium: done entering password");

                Thread.Sleep(rnd.Next(1137, 2256));

                Cache.Instance.Log("Selenium: password will be entered as: [" + EvePassword + "]");
                WaitForElementPresentAndEnabled(new ByIdOrName("ctl00_ContentPlaceHolder1_ccpTrialSignupInfo_ctrlUsernamePassword_newPasswordFields_txtPassword_validatedtextbox")).SendKeys(EvePassword);
                Cache.Instance.Log("Selenium: done entering password");

                Thread.Sleep(rnd.Next(1137, 2256));

                Cache.Instance.Log("Selenium: Manually Agree to terms of service and check the Recaptcha");

                Cache.Instance.Log("Selenium: Done");
                while (Process.GetProcesses().Any(x => x.ProcessName.ToLower().Contains("geckodriver")))
                    Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception: " + ex);
            }
        }

        public void CreateEmailAlias(string AliasEmailAddress, EVE.Proxy myProxy)
        {
            KillGeckoDrivers();

            try
            {
                //
                //https://support.office.com/en-us/article/add-or-remove-an-email-alias-in-outlook-com-459b1989-356d-40fa-a689-8f285b13f1f2
                //
                const string myURL = "https://go.microsoft.com/fwlink/p/?linkid=864833";
                OpenSeleniumBrowser(MySocks5ChromeOptions(myProxy), myURL);

                Cache.Instance.Log("Selenium: EmailAddress will be entered as: [" + AliasEmailAddress + "]");
                WaitForElementPresentAndEnabled(new ByIdOrName("AssociatedIdLive")).SendKeys(AliasEmailAddress);
                Cache.Instance.Log("Selenium: done entering EmailAddress");

                Cache.Instance.Log("Selenium: Add alias Button");
                WaitForElementPresentAndEnabled(new ByIdOrName("SubmitYes")).Click();
                Cache.Instance.Log("Selenium: Done Pressing Add alias Button");

                Cache.Instance.Log("Selenium: Done");
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception: " + ex);
            }
        }

        public void CreateTaskToOpenEmail(string EmailAddress, string EmailPassword, EVE.Proxy myProxy)
        {
            new Task(() =>
            {
                try
                {
                    LogIntoWebOutlookEmail(EmailAddress, EmailPassword, myProxy);
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log("Exception: " + ex);
                }
            }).Start();
        }

        public void CreateTaskToOpenEveAccountSite(string myEveUserName, string myEvePassword, EVE.Proxy myProxy)
        {
            new Task(() =>
            {
                try
                {
                    LogIntoEveAccountWebSite(myEveUserName, myEvePassword, myProxy);
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log("Exception: " + ex);
                }
            }).Start();
        }

        /**
        public bool ValidateEmailFromCCP(string IMAPHost, int ImapPort, EVE.Proxy myProxy, string EmailAddress, string EmailPassword, CancellationTokenSource cTokenSource)
        {
            var doneLookingForEmail = false;
            while (!doneLookingForEmail)
            {
                Cache.Instance.Log("Validation task running.");
                try
                {
                    if (cTokenSource.Token.IsCancellationRequested)
                        return false;

                    ImapX.Collections.MessageCollection emails = null;

                    try
                    {
                        emails = IMAP.Imap.GetInboxEmails(IMAPHost, ImapPort, SslProtocols.Default, true, myProxy.Ip, Convert.ToInt32(myProxy.Socks5Port),
                        myProxy.Username, myProxy.Password,
                        EmailAddress, EmailPassword);
                    }
                    catch (Exception ex)
                    {
                        Cache.Instance.Log("Exception [" + ex + "]");
                    }

                    if (emails != null && emails.Any())
                    {
                        Cache.Instance.Log("Found [" + emails.Count() + "] emails via IMAP");
                    }
                    else
                    {
                        Cache.Instance.Log("Found no emails via IMAP!");
                        doneLookingForEmail = true;
                        return false;
                    }

                    foreach (ImapX.Message email in emails.OrderByDescending(i => i.Date))
                    {
                        Cache.Instance.Log(email.Body.Text);
                        Cache.Instance.Log(email.Body.Html);
                        try
                        {
                            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
                            htmlDoc.LoadHtml(email.Body.Html);
                            var nodes = htmlDoc.DocumentNode.SelectNodes("//a[contains(@href, 'http://click.service.ccpgames.com/?')]");

                            if (nodes.Count > 0 && nodes.Count == 2)
                            {
                                Cache.Instance.Log("We found a node with the link. Nodes: " + nodes.Count);
                                var node = nodes.FirstOrDefault();

                                if (node != null)
                                {
                                    var myURL = node.Attributes["href"].Value;
                                    Cache.Instance.Log(string.Format("Verification url found [{0}]", myURL));

                                    OpenSeleniumBrowser(MySocks5ChromeOptions(myProxy), myURL);
                                    doneLookingForEmail = true;
                                    return true;
                                }

                                Cache.Instance.Log("Node is null.");
                                continue;
                            }

                            Cache.Instance.Log("No nodes found.");
                            continue;
                        }
                        catch (Exception ex)
                        {
                            Cache.Instance.Log("Exception: " + ex);
                        }

                        continue;
                    }

                    Cache.Instance.Log("Done processing [" + emails.Count() + "] emails via IMAP");
                    Thread.Sleep(2500);
                }
                catch (Exception exception)
                {
                    Cache.Instance.Log("Exception:" + exception);
                    return false;
                }
                finally
                {
                    if (!doneLookingForEmail)
                        Task.Delay(500);
                }
            }

            return false;
        }
        **/
        public ChromeOptions MySocks5ChromeOptions(EVE.Proxy myProxy)
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--proxy-server=socks5://" + myProxy.Ip + ":" + myProxy.Socks5Port);
            Cache.Instance.Log("Selenium: Using Proxy [" + myProxy.Description + "] SOCKS5 Proxy [" + myProxy.Ip + "]:[" + myProxy.Socks5Port + "]");
            options.AddArgument("ignore-certificate-errors");
            return options;
        }

        public ChromeOptions MyHTTPProxyChromeOptions(EVE.Proxy myProxy)
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--proxy-server=http://" + myProxy.Ip + ":" + myProxy.HttpProxyPort);
            Cache.Instance.Log("Selenium: Using Proxy [" + myProxy.Description + "] HTTP Proxy [" + myProxy.Ip + "]:[" + myProxy.HttpProxyPort + "]");
            options.AddArgument("ignore-certificate-errors");
            return options;
        }

        public void OpenSeleniumBrowser(ChromeOptions myChromeOptions ,string myURL)
        {
            KillGeckoDrivers();

            try
            {
                if (driver == null)
                {
                    ChromeDriverService driverService = ChromeDriverService.CreateDefaultService();
                    driverService.HideCommandPromptWindow = false;
                    driver = new ChromeDriver(driverService, myChromeOptions, TimeSpan.FromSeconds(60));
                }

                driver.Url = myURL;
                Cache.Instance.Log("Selenium: Navigating [" + driver.Url + "]");
                driver.Navigate();
                Cache.Instance.Log("Selenium: Page loaded: [" + driver.Url + "]");
                Cache.Instance.Log("Selenium: Done waiting");
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception: " + ex);
            }
        }

        public void Dispose()
        {
            if (driver != null)
                driver.Dispose();

            #endregion Methods
        }

        public IWebElement WaitForElementPresentAndEnabled(By locator, int secondsToWait = 30)
        {
            WaitForJSandJQueryToLoad();
            new WebDriverWait(driver, new TimeSpan(0, 0, secondsToWait))
                .Until(d => d.FindElement(locator).Enabled
                            && d.FindElement(locator).Displayed
                            && d.FindElement(locator).GetAttribute("aria-disabled") == null
                );

            return driver.FindElement(locator);
        }

        public IWebElement WaitForElementPresentAndChecked(By locator, int secondsToWait = 30)
        {
            WaitForJSandJQueryToLoad();
            new WebDriverWait(driver, new TimeSpan(0, 0, secondsToWait))
                .Until(d => d.FindElement(locator).Enabled
                            && d.FindElement(locator).Displayed
                            && d.FindElement(locator).GetAttribute("aria-checked") == "true"
                );

            return driver.FindElement(locator);
        }

        public void WaitForJSandJQueryToLoad()
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            wait.Until(d =>
            {
                bool r = d.ExecuteJavaScript<string>("return document.readyState").Equals("complete");
                string state = d.ExecuteJavaScript<string>("return document.readyState");
                Debug.WriteLine("Page state: " + state);
                return r;
            });

            wait.Until(d =>
            {
                try
                {
                    bool r = d.ExecuteJavaScript<long>("return jQuery.active") == 0;
                    Debug.WriteLine("Jquery non active: " + r);
                    return r;
                }
                catch (Exception)
                {
                    return true;
                }
            });
        }

        #endregion IDisposable
    }
}