/*
 * ---------------------------------------
 * User: duketwo
 * Date: 22.03.2014
 * Time: 20:41
 *
 * ---------------------------------------
 */

using SharedComponents.CurlUtil;
using SharedComponents.EVE;
using SharedComponents.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SharedComponents.BrowserAutomation
{
    /// <summary>
    ///     Description of EveAccountCreator.
    /// </summary>
    public class EveAccountCreatorImpl
    {
        #region Constructors

        public EveAccountCreatorImpl(int id)
        {
            Id = id;
        }

        #endregion Constructors

        #region Fields

        private readonly List<Tuple<string, int>> DiscardEmailDomainsList = new List<Tuple<string, int>>
        {
            //new Tuple<string, int>("@discard.email", 15),
            new Tuple<string, int>("@mail-easy.fr", 496),
            new Tuple<string, int>("@hulapla.de", 75),
            new Tuple<string, int>("@hartbot.de", 1410),
            new Tuple<string, int>("@knol-power.nl", 1254)
        };

        private readonly int Id;

        #endregion Fields

        #region Methods

        public bool CreateEveAccount(string url, string email, string username, string password, string proxyPort, string proxyUserPassword,
            CurlWorker curlWoker)
        {
            if (string.IsNullOrEmpty(url)) url = "https://secure.eveonline.com/signup/";
            string pagesource = curlWoker.GetPostPage(url, string.Empty, proxyPort, proxyUserPassword);
            Tuple<string, string> tupel = GetViewStateAndEventValidation(pagesource);
            string viewState = tupel.Item1;
            string eventValidation = tupel.Item2;
            viewState = HttpUtility.UrlEncode(viewState);
            email = HttpUtility.UrlEncode(email);
            eventValidation = HttpUtility.UrlEncode(eventValidation);

            string postData = "__EVENTTARGET=" +
                              "&__EVENTARGUMENT=" +
                              "&__VIEWSTATE=" + viewState +
                              "&__VIEWSTATEGENERATOR=B2E4F60D" +
                              "&__VIEWSTATEENCRYPTED=" +
                              "&__EVENTVALIDATION=" + eventValidation +
                              "&ctl00%24ContentPlaceHolder1%24ccpTrialSignupInfo%24emailInput%24validatedtextbox=" + email +
                              "&ctl00%24ContentPlaceHolder1%24ccpTrialSignupInfo%24ctrlUsernamePassword%24txtUserName%24validatedtextbox=" + username +
                              "&ctl00%24ContentPlaceHolder1%24ccpTrialSignupInfo%24ctrlUsernamePassword%24newPasswordFields%24txtPassword%24validatedtextbox=" +
                              password +
                              "&ctl00%24ContentPlaceHolder1%24submitPersonalInformation=Kostenlos+spielen" +
                              "&thirdPartyLogin=";

            string response = curlWoker.GetPostPage(url, postData, proxyPort, proxyUserPassword);
            return response.ToLower().Contains("verify your account");
        }

        public async Task<Tuple<bool, string, string, string>> CreateEveAlphaAccountAndValidate(string eveRegLink, string accountName, string password,
            string proxyPort,
            string proxyUserPassword, CancellationToken cToken)
        {
            using (CurlWorker curlWorker = new CurlWorker())
            {
                try
                {
                    Tuple<string, int> discardEmailDomainsListItem = DiscardEmailDomainsList.RandomPermutation().First();

                    Task<bool> checkValidateTask = CheckValidateEmail(accountName, password, curlWorker, proxyPort, proxyUserPassword, cToken,
                        discardEmailDomainsListItem);
                    return new Tuple<bool, string, string, string>(await checkValidateTask, accountName, password,
                        accountName + discardEmailDomainsListItem.Item1);
                }
                catch (Exception e)
                {
                    Log(string.Format("[{0}] Exception: {1}", Id, e));
                    return new Tuple<bool, string, string, string>(false, accountName, password, string.Empty);
                }
            }
        }

        private async Task<bool> CheckValidateEmail(string accountName, string password, CurlWorker curlWorker, string proxyPort, string proxyUserPassword,
            CancellationToken cToken, Tuple<string, int> t, int timeout = 5)
        {
            /**
            string postData = "LocalPart=" + accountName +
                              "&DomainType=public&DomainId=" + t.Item2 + "&PrivateDomain=&Password=&LoginButton=Postfach+abrufen&CopyAndPaste=";
            string response = curlWorker.GetPostPage("https://tempr.email/", postData, proxyPort, proxyUserPassword);
            DateTime started = DateTime.UtcNow;
            bool eveAccCreated = false;
            while (!(response = curlWorker.GetPostPage("https://tempr.email/inbox.htm", string.Empty, proxyPort, proxyUserPassword)).Contains("ccpgames"))
            {
                if (!eveAccCreated)
                    if (CreateEveAccount(string.Empty, accountName + t.Item1, accountName, password, proxyPort, proxyUserPassword,
                        curlWorker))
                    {
                        eveAccCreated = true;
                        Log(string.Format("[{0}] Eve account created. [{1}] [{2}] [{3}]", Id, accountName, password, accountName + t.Item1));
                    }
                    else
                    {
                        return false;
                    }

                if (cToken.IsCancellationRequested)
                    return false;

                if (started.AddMinutes(timeout) < DateTime.UtcNow)
                    return false;

                Log(string.Format("[{0}] Checking for registration confirmation email. [{1}]", Id, t));
                await Task.Delay(2500);
            }

            Log($"Email received.");

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);
            HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes("//a[contains(@href, 'https://tempr.email/message-')]");

            if (nodes.Count <= 0)
                return false;

            string messageUrl = nodes.FirstOrDefault().Attributes["href"].Value.Replace(".htm", "-mailVersion=plain.htm");

            Log(string.Format("[{0}] Message url found [{1}]", Id, messageUrl));
            response = curlWorker.GetPostPage(messageUrl, string.Empty, proxyPort, proxyUserPassword);
            htmlDoc.LoadHtml(response);
            nodes = htmlDoc.DocumentNode.SelectNodes("//a[contains(@href, 'https://cis.ccpgames.com/email/verify?')]");

            if (nodes.Count <= 0)
                return false;

            string verificationUrl = nodes.FirstOrDefault().Attributes["href"].Value;
            Log(string.Format("[{0}] Verifcation url found [{1}]", Id, verificationUrl));
            response = curlWorker.GetPostPage(verificationUrl, string.Empty, proxyPort, proxyUserPassword);
            return response.ToLower().Contains("email");
            **/
            return false;
        }

        private Tuple<string, string> GetViewStateAndEventValidation(string htmlsource)
        {
            /**
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlsource);
            HtmlNode.ElementsFlags.Remove("form");
            string viewState = string.Empty;
            string eventValidation = string.Empty;

            if (htmlDoc.DocumentNode != null)
                foreach (HtmlNode text in htmlDoc.DocumentNode.SelectNodes("//@value"))
                {
                    if (text.Id == "__VIEWSTATE")
                        viewState = text.GetAttributeValue("value", "");
                    if (text.Id == "__EVENTVALIDATION")
                        eventValidation = text.GetAttributeValue("value", "");
                }
            return new Tuple<string, string>(viewState, eventValidation);**/
            return null;
        }

        private void Log(string s)
        {
            Debug.WriteLine(s);
            Cache.Instance.Log(s);
        }

        #endregion Methods
    }
}