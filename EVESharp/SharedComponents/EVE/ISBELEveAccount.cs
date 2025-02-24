//#define REFRESH_TOKENS

using Microsoft.IdentityModel.Tokens;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Web;
using System.Xml.Serialization;
using SharedComponents.ISBELExtensions;
using SharedComponents.Security;
using SharedComponents.Utility;
using SharedComponents.Web;
using Newtonsoft.Json;
using SharedComponents.BrowserAutomation;
using System.Diagnostics;
using System.Threading;
using OpenQA.Selenium.Support.PageObjects;
using System.Linq;
using System.Security;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;

namespace SharedComponents.EVE
{
    /// <summary>
    /// An EVE Online account and related data
    /// </summary>
    public class IsbelEveAccount : ViewModelBase //INotifyPropertyChanged, IDisposable, ISBoxerEVELauncher.Launchers.ILaunchTarget
    {
        public IsbelEveAccount(string eveAccountName,
            bool connectToTestServer,
            string tranquilityEveAccessTokenString,
            DateTime tranquilityEveAccessTokenValidUntil,
            string tranquilityRefreshTokenString,
            DateTime tranquilityRefreshTokenValidUntil,
            string sisiEveAccessTokenString,
            DateTime sisiEveAccessTokenValidUntil,
            string sisiRefreshTokenString,
            DateTime sisiRefreshTokenValidUntil)
        {
            EveAccountName = eveAccountName;
            ConnectToTestServer = connectToTestServer;
            //myEveAccount = _myEveAccount;
        }

        //private static EveAccount myEveAccount;
        bool ConnectToTestServer;
        string EveAccountName;

        private Guid _challengeCodeSource = Guid.Empty;

        //[XmlIgnore]
        //[NonSerialized]
        //public SeleniumAutomation SeleniumBrowser = new SeleniumAutomation();

        [XmlIgnore]
        [Browsable(false)]
        public Guid ChallengeCodeSource
        {
            get
            {
                if (_challengeCodeSource == Guid.Empty)
                {
                    _challengeCodeSource = Guid.NewGuid();
                    return _challengeCodeSource;
                }

                return _challengeCodeSource;
            }
        }

        [XmlIgnore] private byte[] _challengeCode = null;

        [XmlIgnore]
        [Browsable(false)]
        private byte[] ChallengeCode
        {
            get
            {
                if (_challengeCode == null)
                {
                    _challengeCode = Encoding.UTF8.GetBytes(ChallengeCodeSource.ToString().Replace("-", ""));
                    return _challengeCode;
                }

                return _challengeCode;
            }
        }

        [XmlIgnore] private string _challengeHash = string.Empty;

        [XmlIgnore]
        [Browsable(false)]
        public string ChallengeHash
        {
            get
            {
                if (string.IsNullOrEmpty(_challengeHash))
                {
                    _challengeHash = Base64UrlEncoder.Encode(Crypto.GenerateSHA256Hash(Base64UrlEncoder.Encode(ChallengeCode)));
                    return _challengeHash;
                }

                return _challengeHash;
            }
        }

        [XmlIgnore] private Guid _state = Guid.Empty;

        [XmlIgnore]
        [Browsable(false)]
        private Guid State
        {
            get
            {
                if (_state == Guid.Empty)
                {
                    _state = Guid.NewGuid();
                    return _state;
                }

                return _state;
            }
        }


        /// <summary>
        /// An Outh2 Access Token
        /// </summary>
        public class Token
        {
            //private AuthObj _authObj;

            public Token()
            {
            }

            /// <summary>
            /// We usually just need to parse a Uri for the Access Token details. So here is the constructor that does it for us.
            /// </summary>
            /// <param name="fromUri"></param>
            [Browsable(false)]
            public Token(AuthObj resp)
            {
                _authObj = resp;
            }

            [Browsable(false)]
            public AuthObj _authObj { get; set; }
        }

        [Browsable(false)]
        private CookieContainer _Cookies;

        /// <summary>
        /// The EVE login process requires cookies; this will ensure we maintain the same cookies for the account
        /// </summary>
        [XmlIgnore]
        [Browsable(false)]
        public CookieContainer Cookies
        {
            get
            {
                if (_Cookies == null)
                {
                    if (!string.IsNullOrEmpty(NewCookieStorage))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();

                        using (Stream s = new MemoryStream(Convert.FromBase64String(NewCookieStorage)))
                        {
                            _Cookies = (CookieContainer)formatter.Deserialize(s);
                        }
                    }
                    else
                    {
                        _Cookies = new CookieContainer();
                    }
                }

                return _Cookies;
            }
            set
            {
                _Cookies = value;
            }
        }

        /// <summary>
        /// It prints all cookies in a CookieContainer. Only for testing.
        /// </summary>
        /// <param name="cookieJar">A cookie container</param>
        public void PrintCookies(CookieContainer cookieJar)
        {
            try
            {
                Hashtable table = (Hashtable)cookieJar
                    .GetType().InvokeMember("m_domainTable",
                    BindingFlags.NonPublic |
                    BindingFlags.GetField |
                    BindingFlags.Instance,
                    null,
                    cookieJar,
                    new object[] { });


                foreach (var key in table.Keys)
                {
                    // Look for http cookies.
                    if (cookieJar.GetCookies(
                        new Uri(string.Format("http://{0}/", key))).Count > 0)
                    {
                        Cache.Instance.Log(cookieJar.Count + " HTTP COOKIES FOUND:");
                        Cache.Instance.Log("----------------------------------");
                        foreach (Cookie cookie in cookieJar.GetCookies(
                            new Uri(string.Format("http://{0}/", key))))
                        {
                            Cache.Instance.Log("Name [" + cookie.Name + "] Value [" + cookie.Value + "] Domain [" + cookie.Domain + "]");
                        }
                    }

                    // Look for https cookies
                    if (cookieJar.GetCookies(
                        new Uri(string.Format("https://{0}/", key))).Count > 0)
                    {
                        Cache.Instance.Log(cookieJar.Count + " HTTPS COOKIES FOUND:");
                        Cache.Instance.Log("----------------------------------");
                        foreach (Cookie cookie in cookieJar.GetCookies(
                            new Uri(string.Format("https://{0}/", key))))
                        {
                            Cache.Instance.Log("Name [" + cookie.Name + "] Value [" + cookie.Value + "] Domain [" + cookie.Domain + "]");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
            }
        }

        [Browsable(false)]
        public void UpdateCookieStorage()
        {
            if (Cookies == null)
            {
                NewCookieStorage = null;
                return;
            }

            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, Cookies);
                ms.Flush();
                ms.Seek(0, SeekOrigin.Begin);

                NewCookieStorage = Convert.ToBase64String(ms.ToArray());
            }
        }

        [Browsable(false)]
        [XmlIgnore]
        public string NewCookieStorage
        {
            get => Web.CookieStorage.GetCookies(EveAccountName);
            set => Web.CookieStorage.SetCookies(EveAccountName, value);
        }

        [XmlIgnore]
        [Browsable(false)]
        private Token _TranquilityToken;

        /// <summary>
        /// AccessToken for Tranquility. Lasts up to 11 hours?
        /// </summary>
        [XmlIgnore]
        [Browsable(false)]
        public Token TranquilityToken { get { return _TranquilityToken; } set { _TranquilityToken = value; OnPropertyChanged("TranquilityToken"); } }

        [XmlIgnore]
        [Browsable(false)]
        private Token _SisiToken;

        /// <summary>
        /// AccessToken for Singularity. Lasts up to 11 hours?
        /// </summary>
        [XmlIgnore]
        [Browsable(false)]
        public Token SisiToken { get { return _SisiToken; } set { _SisiToken = value; OnPropertyChanged("SisiToken"); } }

        [Browsable(false)]
        public LoginResult GetSecurityWarningChallenge(EveAccount myEveAccount, IsbelEveAccount myIsbelEveAccount, string responseBody, Uri referer)
        {
            var uri = RequestResponse.GetSecurityWarningChallenge(myEveAccount.ConnectToTestServer, State.ToString(), ChallengeHash);
            var req = RequestResponse.CreateGetRequest(uri, myEveAccount.ConnectToTestServer, true, referer.ToString(), Cookies, myEveAccount.HWSettings.Proxy.Ip, myEveAccount.HWSettings.Proxy.Ip);
            return GetAccessToken_EveAccountIsbelEveAccountHttpWebRequest(myEveAccount, myIsbelEveAccount, req);
        }

        [Browsable(false)]
        public LoginResult GetEmailChallenge(bool sisi, string responseBody)
        {
            try
            {
                Windows.EmailChallengeWindow emailWindow = new Windows.EmailChallengeWindow(responseBody);
                emailWindow.ShowDialog();
                if (!emailWindow.DialogResult.HasValue || !emailWindow.DialogResult.Value)
                {
                    return LoginResult.EmailVerificationRequired;
                }

                return LoginResult.EmailVerificationRequired;
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
                return LoginResult.InvalidEmailVerificationChallenge;
            }
        }

        [Browsable(false)]
        public LoginResult GetEULAChallenge(EveAccount myEveAccount, IsbelEveAccount myIsbelEveAccount, string responseBody, Uri referer)
        {
            Windows.EVEEULAWindow eulaWindow = new Windows.EVEEULAWindow(responseBody);
            eulaWindow.ShowDialog();
            if (!eulaWindow.DialogResult.HasValue || !eulaWindow.DialogResult.Value)
            {
                return LoginResult.EULADeclined;
            }

            var uri = RequestResponse.GetEulaUri(myEveAccount.ConnectToTestServer, State.ToString(), ChallengeHash);
            HttpWebRequest req = RequestResponse.CreatePostRequest(uri, myEveAccount.ConnectToTestServer, true, referer.ToString(), Cookies, myEveAccount.HWSettings.Proxy.Ip, myEveAccount.HWSettings.Proxy.HttpProxyPort);

            using (SecureBytesWrapper body = new SecureBytesWrapper())
            {
                string eulaHash = RequestResponse.GetEulaHashFromBody(responseBody);
                string returnUrl = RequestResponse.GetEulaReturnUrlFromBody(responseBody);

                string formattedString = String.Format("eulaHash={0}&returnUrl={1}&action={2}", Uri.EscapeDataString(eulaHash), Uri.EscapeDataString(returnUrl), "Accept");
                body.Bytes = Encoding.ASCII.GetBytes(formattedString);

                req.ContentLength = body.Bytes.Length;
                try
                {
                    using (Stream reqStream = req.GetRequestStream())
                    {
                        reqStream.Write(body.Bytes, 0, body.Bytes.Length);
                    }
                }
                catch (System.Net.WebException e)
                {
                    switch (e.Status)
                    {
                        case WebExceptionStatus.Timeout:
                            {
                                return LoginResult.Timeout;
                            }
                        default:
                            throw;
                    }
                }
            }
            LoginResult result;

            try
            {
                result = GetAccessToken_EveAccountIsbelEveAccountHttpWebRequest(myEveAccount, myIsbelEveAccount, req);
            }
            catch (System.Net.WebException)
            {
                //result = GetAccessToken(myEveAccount, req, out accessToken);
            }

            result = GetAccessToken_EveAccountIsbelEveAccountHttpWebRequest(myEveAccount, myIsbelEveAccount, req);
            if (result == LoginResult.Success)
            {
                // successful verification code challenge, make sure we save the cookies.
            }
            return result;
        }

        [Browsable(false)]
        public LoginResult GetEmailCodeChallenge(EveAccount myEveAccount, IsbelEveAccount myIsbelEveAccount, string responseBody)
        {
            Windows.VerificationCodeChallengeWindow acw = new Windows.VerificationCodeChallengeWindow(myEveAccount);
            acw.ShowDialog();
            if (!acw.DialogResult.HasValue || !acw.DialogResult.Value)
            {
                Cache.Instance.Log("GetEmailCodeChallenge: if (!acw.DialogResult.HasValue || !acw.DialogResult.Value)");
                return LoginResult.InvalidEmailVerificationChallenge;
            }

            var uri = RequestResponse.GetVerifyTwoFactorUri(myEveAccount.ConnectToTestServer, State.ToString(), ChallengeHash);
            var req = RequestResponse.CreatePostRequest(uri, myEveAccount.ConnectToTestServer, true, null, Cookies, myEveAccount.HWSettings.Proxy.Ip, myEveAccount.HWSettings.Proxy.HttpProxyPort);

            using (SecureBytesWrapper body = new SecureBytesWrapper())
            {
                body.Bytes = Encoding.ASCII.GetBytes(String.Format("Challenge={0}&command={1}", Uri.EscapeDataString(acw.VerificationCode), "Continue"));

                req.ContentLength = body.Bytes.Length;
                try
                {
                    using (Stream reqStream = req.GetRequestStream())
                    {
                        reqStream.Write(body.Bytes, 0, body.Bytes.Length);
                    }
                }
                catch (System.Net.WebException e)
                {
                    switch (e.Status)
                    {
                        case WebExceptionStatus.Timeout:
                            {
                                Cache.Instance.Log("GetEmailCodeChallenge: WebExceptionStatus.Timeout");
                                return LoginResult.Timeout;
                            }
                        default:
                            throw;
                    }
                }
            }
            LoginResult result = GetAccessToken_EveAccountIsbelEveAccountHttpWebRequest(myEveAccount, myIsbelEveAccount, req);
            if (result == LoginResult.Success)
            {
                // successful verification code challenge, make sure we save the cookies.
            }
            return result;
        }

        [Browsable(false)]
        public LoginResult GetAuthenticatorChallenge(EveAccount myEveAccount, IsbelEveAccount myIsbelEveAccount)
        {
            Windows.AuthenticatorChallengeWindow acw = new Windows.AuthenticatorChallengeWindow(myEveAccount);
            acw.ShowDialog();
            if (!acw.DialogResult.HasValue || !acw.DialogResult.Value)
            {
                return LoginResult.InvalidAuthenticatorChallenge;
            }

            var uri = RequestResponse.GetAuthenticatorUri(myEveAccount.ConnectToTestServer, State.ToString(), ChallengeHash);
            var req = RequestResponse.CreatePostRequest(uri, myEveAccount.ConnectToTestServer, true, uri.ToString(), Cookies, myEveAccount.HWSettings.Proxy.Ip, myEveAccount.HWSettings.Proxy.HttpProxyPort);

            using (SecureBytesWrapper body = new SecureBytesWrapper())
            {
                body.Bytes = Encoding.ASCII.GetBytes(String.Format("Challenge={0}&RememberTwoFactor={1}&command={2}", Uri.EscapeDataString(acw.AuthenticatorCode), "true", "Continue"));

                req.ContentLength = body.Bytes.Length;
                try
                {
                    using (Stream reqStream = req.GetRequestStream())
                    {
                        reqStream.Write(body.Bytes, 0, body.Bytes.Length);
                    }
                }
                catch (System.Net.WebException e)
                {
                    switch (e.Status)
                    {
                        case WebExceptionStatus.Timeout:
                            {
                                return LoginResult.Timeout;
                            }
                        default:
                            throw;
                    }
                }
            }

            LoginResult result = GetAccessToken_EveAccountIsbelEveAccountHttpWebRequest(myEveAccount, myIsbelEveAccount, req);
            if (result == LoginResult.Success)
            {
                // successful authenticator challenge, make sure we save the cookies.
            }
            return result;
        }

        [Browsable(false)]
        public LoginResult GetCharacterChallenge(EveAccount myEveAccount, IsbelEveAccount myIsbelEveAccount)
        {
            if (string.IsNullOrWhiteSpace(myEveAccount.CharacterName))
            {
                // CharacterName is required, sorry dude
                Cache.Instance.Log("GetCharacterChallenge: CharacterName is required");
                return LoginResult.InvalidCharacterChallenge;
            }

            SecureString SecureCharacterName = new SecureString();
            foreach (char c in myEveAccount.CharacterName)
            {
                SecureCharacterName.AppendChar(c);
            }
            SecureCharacterName.MakeReadOnly();

            var uri = RequestResponse.GetCharacterChallengeUri(myEveAccount.ConnectToTestServer, State.ToString(), ChallengeHash);
            var req = RequestResponse.CreatePostRequest(uri, myEveAccount.ConnectToTestServer, true, uri.ToString(), Cookies, myEveAccount.HWSettings.Proxy.Ip, myEveAccount.HWSettings.Proxy.HttpProxyPort);

            using (SecureBytesWrapper body = new SecureBytesWrapper())
            {
                byte[] body1 = Encoding.ASCII.GetBytes(String.Format("RememberCharacterChallenge={0}&Challenge=", "true"));
                using (SecureStringWrapper ssw = new SecureStringWrapper(SecureCharacterName, Encoding.ASCII))
                {
                    using (SecureBytesWrapper escapedCharacterName = new SecureBytesWrapper())
                    {
                        escapedCharacterName.Bytes = System.Web.HttpUtility.UrlEncodeToBytes(ssw.ToByteArray());

                        body.Bytes = new byte[body1.Length + escapedCharacterName.Bytes.Length];
                        System.Buffer.BlockCopy(body1, 0, body.Bytes, 0, body1.Length);
                        System.Buffer.BlockCopy(escapedCharacterName.Bytes, 0, body.Bytes, body1.Length, escapedCharacterName.Bytes.Length);
                    }
                }

                req.ContentLength = body.Bytes.Length;
                try
                {
                    using (Stream reqStream = req.GetRequestStream())
                    {
                        reqStream.Write(body.Bytes, 0, body.Bytes.Length);
                    }
                }
                catch (System.Net.WebException e)
                {
                    switch (e.Status)
                    {
                        case WebExceptionStatus.Timeout:
                            {
                                Cache.Instance.Log("GetCharacterChallenge: WebExceptionStatus.Timeout");
                                return LoginResult.Timeout;
                            }
                        default:
                            throw;
                    }
                }
            }
            return GetAccessToken_EveAccountIsbelEveAccountHttpWebRequest(myEveAccount, myIsbelEveAccount, req);
        }

        [Browsable(false)]
        public LoginResult GetAccessToken_EveAccountIsbelEveAccountHttpWebRequest(EveAccount myEveAccount, IsbelEveAccount myIsbelEveAccount, HttpWebRequest req)
        {
            Response response = null;
            try
            {
                System.Threading.Thread.Sleep(Util.GetRandom(300, 500));
                //if (myLB.strHTML_RequestVerificationToken == "")
                //{
                Cache.Instance.Log("GetAccessToken: if (myLB.strHTML_RequestVerificationToken == string.Empty)");
                response = new Response(req);
                //}
                //else
                //{
                //    Cache.Instance.Log("GetAccessToken: response = new Response(req, WebRequestType.Result, myIsbelEveAccount);");
                //    response = new Response(req, WebRequestType.Result, myEveAccount);
                //}

                //Cache.Instance.Log("GetAccessToken: response = new Response(req);");
                //response = new Response(req);

                string responseBody = response.Body;
                UpdateCookieStorage();

                if (responseBody.Contains("Incorrect character name entered"))
                {
                    Cache.Instance.Log("GetAccessToken: Incorrect character name entered");
                    return LoginResult.InvalidCharacterChallenge;
                }

                if (responseBody.Contains("Invalid username / password"))
                {
                    Cache.Instance.Log("GetAccessToken: Invalid username / password");
                    return LoginResult.InvalidUsernameOrPassword;
                }

                // I'm just guessing on this one at the moment.
                if (responseBody.Contains("Invalid authenticat")
                    || (responseBody.Contains("Verification code mismatch") && responseBody.Contains("/account/authenticator"))
                )
                {
                    Cache.Instance.Log("GetAccessToken: InvalidAuthenticatorChallenge");
                    return LoginResult.InvalidAuthenticatorChallenge;
                }
                //The 2FA page now has "Character challenge" in the text but it is hidden. This should fix it from
                //Coming up during 2FA challenge
                if (responseBody.Contains("Character challenge") && !responseBody.Contains("visuallyhidden"))
                {
                    Cache.Instance.Log("GetAccessToken: Character challenge");
                    return GetCharacterChallenge(myEveAccount, myIsbelEveAccount);
                }

                if (responseBody.Contains("Email verification required"))
                {
                    Cache.Instance.Log("GetAccessToken: Email verification required");
                    return GetEmailChallenge(myEveAccount.ConnectToTestServer, responseBody);
                }

                if (responseBody.Contains("Authenticator is enabled"))
                {
                    Cache.Instance.Log("GetAccessToken: Authenticator is enabled");
                    return GetAuthenticatorChallenge(myEveAccount, myIsbelEveAccount);
                }

                if (responseBody.Contains("Please enter the verification code "))
                {
                    Cache.Instance.Log("GetAccessToken: Please enter the verification code");
                    return GetEmailCodeChallenge(myEveAccount, myIsbelEveAccount, responseBody);
                }

                if (responseBody.Contains("Security Warning"))
                {
                    Cache.Instance.Log("GetAccessToken: Security Warning");
                    return GetSecurityWarningChallenge(myEveAccount, myIsbelEveAccount, responseBody, response.ResponseUri);
                }

                if (responseBody.ToLower().Contains("form action=\"/oauth/eula\""))
                {
                    Cache.Instance.Log("GetAccessToken: EULA Challenge");
                    return GetEULAChallenge(myEveAccount, myIsbelEveAccount, responseBody, response.ResponseUri);
                }

                try
                {
                    myEveAccount.CodeForEVESSO = HttpUtility.ParseQueryString(response.ResponseUri.ToString()).Get("code");
                    if (myEveAccount.CodeForEVESSO == null)
                    {
                        Cache.Instance.Log("GetAccessToken_EveAccountIsbelEveAccountHttpWebRequest: if (_code == null)");
                        return LoginResult.Error;
                    }

                    TakeCodeAsInputAndRequestRefreshToken(myEveAccount, this);
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log("Unrecognized Response: " + responseBody);
                    Cache.Instance.Log("Exception [" + ex + "]");
                    // can't get the token
                    return LoginResult.TokenFailure;
                }

                return LoginResult.Success;
            }
            catch (System.Net.WebException we)
            {
                switch (we.Status)
                {
                    case WebExceptionStatus.Timeout:
                        {
                            Cache.Instance.Log("GetAccessToken_EveAccountIsbelEveAccountHttpWebRequest: WebExceptionStatus.Timeout");
                            return LoginResult.Timeout;
                        }
                    default:
                        string responseBody = we.Response.GetResponseBody();
                        Cache.Instance.Log("Unrecognized Response: " + responseBody);
                        Cache.Instance.Log("Exception [" + we + "]");
                        return LoginResult.Error;
                }
            }
        }

        [Browsable(false)]
        public LoginResult GetAccessToken_EveAccountIsbelEveAccountHttpWebRequestUsingRefreshToken(EveAccount myEveAccount, IsbelEveAccount myIsbelEveAccount, HttpWebRequest req)
        {
            Response response = null;
            try
            {
                System.Threading.Thread.Sleep(Util.GetRandom(300, 500));
                //if (myLB.strHTML_RequestVerificationToken == "")
                //{
                Cache.Instance.Log("GetAccessToken: if (myLB.strHTML_RequestVerificationToken == string.Empty)");
                response = new Response(req);
                //}
                //else
                //{
                //    Cache.Instance.Log("GetAccessToken: response = new Response(req, WebRequestType.Result, myIsbelEveAccount);");
                //    response = new Response(req, WebRequestType.Result, myEveAccount);
                //}

                //Cache.Instance.Log("GetAccessToken: response = new Response(req);");
                //response = new Response(req);

                string responseBody = response.Body;
                UpdateCookieStorage();

                if (responseBody.Contains("Incorrect character name entered"))
                {
                    Cache.Instance.Log("GetAccessToken: Incorrect character name entered");
                    return LoginResult.InvalidCharacterChallenge;
                }

                if (responseBody.Contains("Invalid username / password"))
                {
                    Cache.Instance.Log("GetAccessToken: Invalid username / password");
                    return LoginResult.InvalidUsernameOrPassword;
                }

                // I'm just guessing on this one at the moment.
                if (responseBody.Contains("Invalid authenticat")
                    || (responseBody.Contains("Verification code mismatch") && responseBody.Contains("/account/authenticator"))
                )
                {
                    Cache.Instance.Log("GetAccessToken: InvalidAuthenticatorChallenge");
                    return LoginResult.InvalidAuthenticatorChallenge;
                }
                //The 2FA page now has "Character challenge" in the text but it is hidden. This should fix it from
                //Coming up during 2FA challenge
                if (responseBody.Contains("Character challenge") && !responseBody.Contains("visuallyhidden"))
                {
                    Cache.Instance.Log("GetAccessToken: Character challenge");
                    return GetCharacterChallenge(myEveAccount, myIsbelEveAccount);
                }

                if (responseBody.Contains("Email verification required"))
                {
                    Cache.Instance.Log("GetAccessToken: Email verification required");
                    return GetEmailChallenge(myEveAccount.ConnectToTestServer, responseBody);
                }

                if (responseBody.Contains("Authenticator is enabled"))
                {
                    Cache.Instance.Log("GetAccessToken: Authenticator is enabled");
                    return GetAuthenticatorChallenge(myEveAccount, myIsbelEveAccount);
                }

                if (responseBody.Contains("Please enter the verification code "))
                {
                    Cache.Instance.Log("GetAccessToken: Please enter the verification code");
                    return GetEmailCodeChallenge(myEveAccount, myIsbelEveAccount, responseBody);
                }

                if (responseBody.Contains("Security Warning"))
                {
                    Cache.Instance.Log("GetAccessToken: Security Warning");
                    return GetSecurityWarningChallenge(myEveAccount, myIsbelEveAccount, responseBody, response.ResponseUri);
                }

                if (responseBody.ToLower().Contains("form action=\"/oauth/eula\""))
                {
                    Cache.Instance.Log("GetAccessToken: EULA Challenge");
                    return GetEULAChallenge(myEveAccount, myIsbelEveAccount, responseBody, response.ResponseUri);
                }

                try
                {
                    myEveAccount.CodeForEVESSO = HttpUtility.ParseQueryString(response.ResponseUri.ToString()).Get("code");
                    if (myEveAccount.CodeForEVESSO == null)
                    {
                        Cache.Instance.Log("GetAccessToken_EveAccountIsbelEveAccountHttpWebRequest: if (_code == null)");
                        return LoginResult.Error;
                    }

                    TakeCodeAsInputAndRequestRefreshToken(myEveAccount, this);
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log("Unrecognized Response: " + responseBody);
                    Cache.Instance.Log("Exception [" + ex + "]");
                    // can't get the token
                    return LoginResult.TokenFailure;
                }

                return LoginResult.Success;
            }
            catch (System.Net.WebException we)
            {
                switch (we.Status)
                {
                    case WebExceptionStatus.Timeout:
                        {
                            Cache.Instance.Log("GetAccessToken_EveAccountIsbelEveAccountHttpWebRequest: WebExceptionStatus.Timeout");
                            return LoginResult.Timeout;
                        }
                    default:
                        string responseBody = we.Response.GetResponseBody();
                        Cache.Instance.Log("Unrecognized Response: " + responseBody);
                        Cache.Instance.Log("Exception [" + we + "]");
                        return LoginResult.Error;
                }
            }
        }


        public class AuthObj
        {
            private int _expiresIn;
            public string access_token { get; set; }

            public string error { get; set; }

            public string sso_status { get; set; }

            public int Expires_in
            {
                get
                {
                    return _expiresIn;
                }
                set
                {
                    _expiresIn = value;
                    Expiration = DateTime.Now.AddSeconds(_expiresIn);
                }
            }

            public string Token_type { get; set; }
            public string Refresh_token { get; set; }

            public DateTime Expiration { get; private set; }
        }

        [Browsable(false)]
        public LoginResult GetAccessToken_CodeEveAccountIsbelEveAccount(EveAccount myEveAccount, IsbelEveAccount myIsbelEveAccount, out Response response)
        {
            Cache.Instance.Log("Attempting to GetAccessToken: ConnectToTestServer [" + myEveAccount.ConnectToTestServer + "] CodeForEVESSO [" + myEveAccount.CodeForEVESSO + "]");
            if (string.IsNullOrEmpty(myEveAccount.CodeForEVESSO))
            {
                Cache.Instance.Log("CodeForEVESSO is empty; aborting;");
                response = new Response(null);
                return LoginResult.Error;
            }

            HttpWebRequest req2 = RequestResponse.CreatePostRequest(new Uri(RequestResponse.token, UriKind.Relative), myEveAccount.ConnectToTestServer, true, RequestResponse.refererUri, Cookies, myEveAccount.HWSettings.Proxy.Ip, myEveAccount.HWSettings.Proxy.HttpProxyPort);
            Thread.Sleep(Util.GetRandom(100, 300));
            Cache.Instance.Log("Attempting to GetSsoTokenRequestBody: ConnectToTestServer [" + myEveAccount.ConnectToTestServer + "] code [" + myEveAccount.CodeForEVESSO + "] challengeCode [" + ChallengeCode + "]");
            req2.SetBody(RequestResponse.GetSsoTokenRequestBodyGrantTypeAuthorizationCode(myEveAccount.ConnectToTestServer, myEveAccount.CodeForEVESSO, ChallengeCode));
            return RequestResponse.GetHttpWebResponseAccessToken(req2, UpdateCookieStorage, myEveAccount, myIsbelEveAccount, out response);
        }

        /**
        /// <summary>
        /// Starts obtaining a new access token from the refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="callback">A callback to receive the new token.</param>
        public void GetNewToken(String myRefreshToken)
        {
            //
            // refreshToken has to exist
            //

            string data;
            if (string.IsNullOrEmpty(m_secret))
                // PKCE
                data = string.Format(NetworkConstants.PostDataRefreshPKCE, WebUtility.
                    UrlEncode(myRefreshToken), m_clientID);
            else
                data = string.Format(NetworkConstants.PostDataRefreshToken, WebUtility.
                    UrlEncode(myRefreshToken));
            FetchToken(data, callback, false);
        }
        **/

        [Browsable(false)]
        public LoginResult GetAccessToken_CodeEveAccountIsbelEveAccountRefreshToken(string code, EveAccount myEveAccount, IsbelEveAccount myIsbelEveAccount, out Response response)
        {
            Cache.Instance.Log("Attempting to GetAccessToken with a refresh_token: ConnectToTestServer [" + myEveAccount.ConnectToTestServer + "] code [" + code + "]");
            HttpWebRequest req2 = RequestResponse.CreatePostRequest(new Uri(RequestResponse.token, UriKind.Relative), myEveAccount.ConnectToTestServer, true, RequestResponse.refererUri, Cookies, myEveAccount.HWSettings.Proxy.Ip, myEveAccount.HWSettings.Proxy.HttpProxyPort);
            //Thread.Sleep(Util.GetRandom(100, 300));
            Cache.Instance.Log("Attempting to GetSsoTokenRequestBody: ConnectToTestServer [" + myEveAccount.ConnectToTestServer + "] code [" + code + "] challengeCode [" + ChallengeCode + "]");
            req2.SetBody(RequestResponse.GetSsoTokenRequestBodyGrantTypeRefreshCode(myEveAccount.ConnectToTestServer, code, ChallengeCode));
            return RequestResponse.GetHttpWebResponseRefreshToken(req2, UpdateCookieStorage, myEveAccount, myIsbelEveAccount, out response);
        }

        [Browsable(false)]
        public LoginResult GetRequestVerificationToken(Uri uri, EveAccount myEveAccount, IsbelEveAccount myIsbelEveAccount, out string verificationToken)
        {
            Cache.Instance.Log("GetRequestVerificationToken: started");
            Response response;
            verificationToken = null;

            var req = RequestResponse.CreateGetRequest(uri, myEveAccount.ConnectToTestServer, true, RequestResponse.refererUri, Cookies, myEveAccount.HWSettings.Proxy.Ip, myEveAccount.HWSettings.Proxy.HttpProxyPort);
            req.ContentLength = 0;
            Thread.Sleep(Util.GetRandom(100, 300));
            var result = RequestResponse.GetHttpWebResponseAccessToken(req, UpdateCookieStorage, myEveAccount, myIsbelEveAccount, out response);

            if (result == LoginResult.Success)
            {
                Thread.Sleep(Util.GetRandom(100, 300));
                verificationToken = RequestResponse.GetRequestVerificationTokenResponse(response);
                if (verificationToken == null)
                {
                    Cache.Instance.Log("GetRequestVerificationToken: Error: if (verificationToken == null)");
                    Cache.Instance.Log("GetRequestVerificationToken: Disable Schedule for AccountName [" + myEveAccount.MaskedAccountName + "] CharacterName [" + myEveAccount.MaskedCharacterName + "]");
                    myEveAccount.UseScheduler = false;
                    return LoginResult.Error;
                }
            }

            if (result == LoginResult.Error)
            {
                Cache.Instance.Log("GetRequestVerificationToken: if (result == LoginResult.Error)");
                Cache.Instance.Log("GetRequestVerificationToken: Disable Schedule for AccountName [" + myEveAccount.MaskedAccountName + "] CharacterName [" + myEveAccount.MaskedCharacterName + "]");
                myEveAccount.UseScheduler = false;
                return result;
            }

            Cache.Instance.Log("GetRequestVerificationToken: finished");
            return result;
        }

        [Browsable(false)]
        public LoginResult GetRequestVerificationRefreshToken(Uri uri, EveAccount myEveAccount, IsbelEveAccount myIsbelEveAccount, out string verificationToken)
        {
            Cache.Instance.Log("GetRequestVerificationToken: started");
            Response response;
            verificationToken = null;

            var req = RequestResponse.CreateGetRequest(uri, myEveAccount.ConnectToTestServer, true, RequestResponse.refererUri, Cookies, myEveAccount.HWSettings.Proxy.Ip, myEveAccount.HWSettings.Proxy.HttpProxyPort);
            req.ContentLength = 0;
            Thread.Sleep(Util.GetRandom(100, 300));
            var result = RequestResponse.GetHttpWebResponseRefreshToken(req, UpdateCookieStorage, myEveAccount, myIsbelEveAccount, out response);

            if (result == LoginResult.Success)
            {
                Thread.Sleep(Util.GetRandom(100, 300));
                verificationToken = RequestResponse.GetRequestVerificationTokenResponse(response);
                if (verificationToken == null)
                {
                    Cache.Instance.Log("GetRequestVerificationToken: Error: if (verificationToken == null)");
                    Cache.Instance.Log("GetRequestVerificationToken: Disable Schedule for AccountName [" + myEveAccount.MaskedAccountName + "] CharacterName [" + myEveAccount.MaskedCharacterName + "]");
                    myEveAccount.UseScheduler = false;
                    return LoginResult.Error;
                }
            }

            if (result == LoginResult.Error)
            {
                Cache.Instance.Log("GetRequestVerificationToken: if (result == LoginResult.Error)");
                Cache.Instance.Log("GetRequestVerificationToken: Disable Schedule for AccountName [" + myEveAccount.MaskedAccountName + "] CharacterName [" + myEveAccount.MaskedCharacterName + "]");
                myEveAccount.UseScheduler = false;
                return result;
            }

            Cache.Instance.Log("GetRequestVerificationToken: finished");
            return result;
        }


        public Token MyRefreshAndAccessTokensIfAny
        {
            get
            {
                if (ConnectToTestServer)
                    return SisiToken;

                return TranquilityToken;
            }
            set
            {
                if (ConnectToTestServer)
                {
                    SisiToken = value;
                    return;
                }

                TranquilityToken = value;
                return;
            }
        }

        public void TakeCodeAsInputAndRequestRefreshToken(EveAccount myEveAccount, IsbelEveAccount myIsbelEveAccount)
        {
            Response response = null;
            GetAccessToken_CodeEveAccountIsbelEveAccount(myEveAccount, myIsbelEveAccount, out response);

            if (!string.IsNullOrEmpty(response.Body))
            {
                var tempToken = new Token(JsonConvert.DeserializeObject<AuthObj>(response.Body));
                Cache.Instance.Log("Raw EveAccessToken as JSON: [" + response.Body + "]");
                Cache.Instance.Log("EveAccessTokenString-[" + tempToken._authObj.access_token + "]");
                Cache.Instance.Log("RefreshTokenString-[" + tempToken._authObj.Refresh_token + "]");

                myEveAccount.EveAccessTokenString = tempToken._authObj.access_token;
                myEveAccount.EveAccessTokenValidUntil = tempToken._authObj.Expiration;
                myEveAccount.RefreshTokenString = tempToken._authObj.Refresh_token;
                myEveAccount.RefreshTokenValidUntil = DateTime.Now.AddDays(90);
            }
        }

        private void ProcessEveSSOLoginWebPage(Uri uri, EveAccount myEveAccount, IsbelEveAccount myIsbelEveAccount, SeleniumAutomation seleniumAutomationImpl)
        {
            try
            {
                new Thread(() =>
                {
                    try
                    {
                        //SeleniumAutomation.KillChromeGeckoDrivers();

                        Cache.Instance.Log("Selenium: Open WebPage: " + uri.AbsoluteUri);
                        //seleniumAutomationImpl.OpenSeleniumBrowser(seleniumAutomationImpl.MyHTTPProxyChromeOptions(myEveAccount.HWSettings), uri.AbsoluteUri);


                        //seleniumAutomationImpl.OpenSeleniumChromeBrowser(seleniumAutomationImpl.MySocks5NoJavascriptChromeOptions(myEveAccount.HWSettings.Proxy), uri.AbsoluteUri);
                        seleniumAutomationImpl.OpenSeleniumChromeBrowser(seleniumAutomationImpl.MySocks5ChromeOptions(myEveAccount.HWSettings.Proxy), uri.AbsoluteUri);
                        //seleniumAutomationImpl.OpenSeleniumEdgeBrowser(seleniumAutomationImpl.MySocks5EdgeOptions(myEveAccount.HWSettings.Proxy), uri.AbsoluteUri);

                        //accessToken = null;

                        Cache.Instance.Log("Selenium: Waiting up to 300 seconds!");

                        /**
                        //SeleniumBrowserForEveSSO.WaitForElementPresentAndEnabled(new ByIdOrName("UserName"));

                        while (SeleniumBrowserForEveSSO.WaitForElementPresentAndEnabled(new ByIdOrName("UserName")).Displayed)
                        {
                            Cache.Instance.Log("Selenium: Found ID [ UserName ] waiting for human to complete it"); //make noise?
                            //Cache.Instance.Log("Selenium: Eve UserName should be entered as: [" + AccountName + "]");
                        }

                        while (SeleniumBrowserForEveSSO.WaitForElementPresentAndEnabled(new ByIdOrName("Password")).Displayed)
                        {
                            Cache.Instance.Log("Selenium: Found ID [ UserName ] waiting for human to complete it"); //make noise?
                            //Cache.Instance.Log("Selenium: password should be entered as: [" + Password + "]");
                        }

                        while (SeleniumBrowserForEveSSO.WaitForElementPresentAndEnabled(new ByIdOrName("submitButton")).Displayed)
                        {
                            Cache.Instance.Log("Selenium: Found ID [ submitButton ] waiting for human to complete it"); //make noise?
                            //Cache.Instance.Log("Selenium: Press Complete button when ready");
                        }

                        try
                        {
                            while (SeleniumBrowserForEveSSO.WaitForElementPresentAndEnabled(new ByIdOrName("main"), 2).Text.ToLower().Contains("One More Step".ToLower()))
                            {
                                Cache.Instance.Log("Selenium: Found hCapcha via ID [ main ] and text [ One More Step ] waiting for human to complete it"); //make noise?
                            }

                            while (SeleniumBrowserForEveSSO.WaitForElementPresentAndEnabled(new ByIdOrName("new-account"), 2).Text != "")
                            {
                                Cache.Instance.Log("Selenium: Found hCapcha via ID [ new-account ] waiting for human to complete it");
                            }

                            while (SeleniumBrowserForEveSSO.WaitForElementPresentAndEnabled(new ByIdOrName("logo"), 2).Text != "")
                            {
                                Cache.Instance.Log("Selenium: Found hCapcha via ID: logo: waiting for human to complete it"); //make noise?
                            }
                        }
                        catch (Exception ex)
                        {
                            Cache.Instance.Log("Exception [" + ex + "]");
                        }


                        Cache.Instance.Log("Selenium: Waiting for Login Success");
                        Thread.Sleep(Util.GetRandom(1800, 3000));
                        //SeleniumBrowserForEveSSO.WaitForElementPresentAndEnabled(new ByIdOrName("retryButton"), 300);
                        **/

                        seleniumAutomationImpl.WaitForElementPresentAndEnabled(seleniumAutomationImpl.chromeDriver, new ByIdOrName("login-context"), 300);

                        string currentUrl = seleniumAutomationImpl.chromeDriver.Url;
                        Cache.Instance.Log("SeleniumBrowserForEveSSO: Logged in? URL is [" + currentUrl + "]");
                        myEveAccount.CodeForEVESSO = HttpUtility.ParseQueryString(currentUrl).Get("code");
                        if (myEveAccount.CodeForEVESSO == null)
                        {
                            Cache.Instance.Log("SeleniumBrowserForEveSSO: _code is not part of the url [ " + currentUrl + " ]");
                            return;
                        }

                        TakeCodeAsInputAndRequestRefreshToken(myEveAccount, myIsbelEveAccount);

                        foreach (OpenQA.Selenium.Cookie cook in seleniumAutomationImpl.chromeDriver.Manage().Cookies.AllCookies)
                        {
                            System.Net.Cookie cookie = new System.Net.Cookie();
                            cookie.Name = cook.Name;
                            cookie.Value = cook.Value;
                            cookie.Domain = cook.Domain;
                            myIsbelEveAccount.Cookies.Add(cookie);
                        }

                        seleniumAutomationImpl.chromeDriver.Close();
                        seleniumAutomationImpl.chromeDriver.Quit();
                    }
                    catch (Exception ex)
                    {
                        Cache.Instance.Log("ProcessEveSSOLoginWebPage: Exception [" + ex + "]");
                    }
                }
                ).Start();
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("ProcessEveSSOLoginWebPage: Exception [" + ex + "]!");
                return;
            }
        }

        private void ProcessEveSSOLoginWebPageAuto(Uri uri, EveAccount myEveAccount, IsbelEveAccount myIsbelEveAccount, SeleniumAutomation seleniumAutomationImpl)
        {
            try
            {
                new Thread(() =>
                {
                    try
                    {
                        //SeleniumAutomation.KillChromeGeckoDrivers();

                        Cache.Instance.Log("Selenium: Open WebPage: " + uri.AbsoluteUri);
                        seleniumAutomationImpl.OpenSeleniumChromeBrowser(seleniumAutomationImpl.MySocks5ChromeOptions(myEveAccount.HWSettings.Proxy), uri.AbsoluteUri);
                        //accessToken = null;

                        Cache.Instance.Log("Selenium: Waiting up to 300 seconds!");
                        //todo: add and test auto login (we cant do the captcha though!)

                        //bool EveSSOLoggedIn = false;
                        //bool CredentialsEntered = false;

                        /**
                        while (!EveSSOLoggedIn)
                        {
                            Cache.Instance.Log("Selenium: Waiting...");
                            Thread.Sleep(Util.GetRandom(1800, 3000));
                            if (CredentialsEntered)
                            {
                                Cache.Instance.Log("Selenium: Waiting... additional 2 sec for login process?");
                                Thread.Sleep(Util.GetRandom(1800, 3000));
                            }

                            //
                            // we need to detect the hcaptcha and wait for the user to handle it!
                            //


                            if (seleniumAutomationImpl.driver.PageSource.Contains("Please stand by, while we are checking your browser..."))
                            {
                                Cache.Instance.Log("if (seleniumAutomationImpl.driver.PageSource.Contains(Please stand by, while we are checking your browser...)): Refresh Page");
                                seleniumAutomationImpl.driver.Navigate().GoToUrl(uri.AbsoluteUri);
                                continue;
                            }
                            else if (seleniumAutomationImpl.driver.PageSource.Contains("Be sure to click the prompt above to login to the EVE Online launcher"))
                            {
                                Cache.Instance.Log("if (seleniumAutomationImpl.driver.PageSource.Contains(Be sure to click the prompt above to login to the EVE Online launcher))");
                                EveSSOLoggedIn = true;
                                break;
                            }
                            else if (seleniumAutomationImpl.driver.PageSource.Contains("<html><head></head><body></body></html>"))
                            {
                                Cache.Instance.Log("if (seleniumAutomationImpl.driver.PageSource.Contains(<html><head></head><body></body></html>");
                                seleniumAutomationImpl.driver.Close();
                                seleniumAutomationImpl.driver.Quit();
                                break;
                            }
                            else if (seleniumAutomationImpl.driver.PageSource.Contains("Please enter the verification code"))
                            {
                                //
                                // in this case keep waiting for manual input!
                                //
                                Cache.Instance.Log("Please manually enter the verification code (from email?)");
                                Thread.Sleep(Util.GetRandom(4000, 6000));
                                break;
                            }
                            else if (seleniumAutomationImpl.driver.PageSource.Contains("Log in to your account"))
                            {
                                if (seleniumAutomationImpl.driver.PageSource.Contains("Invalid username / password"))
                                {
                                    //
                                    // in this case keep waiting for manual login!
                                    //
                                    Cache.Instance.Log("Please manually login: auto login failed");
                                    Thread.Sleep(Util.GetRandom(4000, 6000));
                                    break;
                                }

                                Cache.Instance.Log("if (seleniumAutomationImpl.driver.PageSource.Contains(Log in to your account))");
                                seleniumAutomationImpl.WaitForElementPresentAndEnabled(new ByIdOrName("UserName")).SendKeys(myEveAccount.AccountName);
                                Thread.Sleep(Util.GetRandom(1800, 3000));
                                seleniumAutomationImpl.WaitForElementPresentAndEnabled(new ByIdOrName("Password")).SendKeys(myEveAccount.Password);
                                Thread.Sleep(Util.GetRandom(1800, 3000));
                                seleniumAutomationImpl.WaitForElementPresentAndEnabled(new ByIdOrName("RememberMe")).Click();
                                Thread.Sleep(Util.GetRandom(1800, 3000));
                                CredentialsEntered = true;
                                continue;
                            }

                            if (CredentialsEntered)
                            {
                                Cache.Instance.Log("CredentialsEntered = true");
                                break;
                            }
                        }
                        **/

                        seleniumAutomationImpl.WaitForElementPresentAndEnabled(seleniumAutomationImpl.chromeDriver, new ByIdOrName("login-context"), 300);

                        string currentUrl = seleniumAutomationImpl.chromeDriver.Url;
                        Cache.Instance.Log("SeleniumBrowserForEveSSO: Logged in? URL is [" + currentUrl + "]");
                        myEveAccount.CodeForEVESSO = HttpUtility.ParseQueryString(currentUrl).Get("code");
                        if (myEveAccount.CodeForEVESSO == null)
                        {
                            Cache.Instance.Log("SeleniumBrowserForEveSSO: _code is not part of the url [ " + currentUrl + " ]");
                            return;
                        }

                        TakeCodeAsInputAndRequestRefreshToken(myEveAccount, this);

                        foreach (OpenQA.Selenium.Cookie cook in seleniumAutomationImpl.chromeDriver.Manage().Cookies.AllCookies)
                        {
                            System.Net.Cookie cookie = new System.Net.Cookie();
                            cookie.Name = cook.Name;
                            cookie.Value = cook.Value;
                            cookie.Domain = cook.Domain;
                            myIsbelEveAccount.Cookies.Add(cookie);
                        }

                        seleniumAutomationImpl.chromeDriver.Close();
                        seleniumAutomationImpl.chromeDriver.Quit();
                    }
                    catch (Exception ex)
                    {
                        Cache.Instance.Log("ProcessEveSSOLoginWebPage: Exception [" + ex + "]");
                    }
                }
                ).Start();
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("ProcessEveSSOLoginWebPage: Exception [" + ex + "]!");
                return;
            }
        }


        [Browsable(false)]
        public LoginResult LogIntoEveAccountWebSiteForSSOToken(EveAccount myEveAccount, IsbelEveAccount myIsbelEveAccount, SeleniumAutomation seleniumAutomationImpl)
        {
            try
            {
                //FIXME
                //if (MyRefreshAndAccessTokensIfAny != null && !MyRefreshAndAccessTokensIfAny.IsEveAccesToken_ShortTerm_Expired)
                //{
                //    Cache.Instance.Log("EveAccessToken: IsExpired [" + MyRefreshAndAccessTokensIfAny.IsEveAccesToken_ShortTerm_Expired + "] Expiration [" + MyRefreshAndAccessTokensIfAny.RefreshToken_Expiration + "] TokenString [" + MyRefreshAndAccessTokensIfAny.RefreshTokenString + "]");
                //    myEveAccount.EveAccessTokenString = tempToken._authObj.access_token;
                //    myEveAccount.EveAccessTokenValidUntil = tempToken._authObj.Expiration;
                //
                //    return LoginResult.Success;
                //}

                // need SecurePassword.

                SecureString SecurePassword = new System.Security.SecureString();
                foreach (char c in myEveAccount.Password)
                {
                    SecurePassword.AppendChar(c);
                }
                SecurePassword.MakeReadOnly();

                if (SecurePassword.Length == 0)
                {
                    // password is required, sorry dude
                    return LoginResult.InvalidUsernameOrPassword;
                }

                var uri = RequestResponse.GetLoginUri(myEveAccount.ConnectToTestServer, State.ToString(), ChallengeHash);

                if (!uri.IsAbsoluteUri)
                    uri = new Uri(string.Concat(myEveAccount.ConnectToTestServer ? RequestResponse.sisiBaseUri : RequestResponse.tqBaseUri, uri.ToString()));

                Cache.Instance.Log("CreateGetRequest: uri is [" + uri.AbsoluteUri + "]");

                ProcessEveSSOLoginWebPage(uri, myEveAccount, myIsbelEveAccount, seleniumAutomationImpl);

                //SSO gathered where?
                Cache.Instance.Log("Selenium (Chrome): Has been started and is running in another thread");
                return LoginResult.Success;
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception: " + ex);
                return LoginResult.Error;
            }
        }

        public LoginResult GetRefreshToken(EveAccount myEveAccount, IsbelEveAccount myIsbelEveAccount)
        {
            Cache.Instance.Log("GetRefreshToken: GetLoginUri");
            var uri = RequestResponse.GetLoginUri(myEveAccount.ConnectToTestServer, State.ToString(), ChallengeHash);

            string RequestVerificationToken = string.Empty;
            LoginResult loginResult = GetRequestVerificationRefreshToken(uri, myEveAccount, myIsbelEveAccount, out RequestVerificationToken);
            if (loginResult == LoginResult.Error)
            {
                Cache.Instance.Log("GetRefreshToken: if (result == LoginResult.Error)");
                return loginResult;
            }

            Cache.Instance.Log("GetRefreshToken: completed: loginResult is [" + loginResult.ToString() + "]");
            var req = RequestResponse.CreatePostRequest(uri, myEveAccount.ConnectToTestServer, true, RequestResponse.refererUri, Cookies, myEveAccount.HWSettings.Proxy.Ip, myEveAccount.HWSettings.Proxy.HttpProxyPort);
            return LoginResult.Success;
        }

        [Browsable(false)]
        public LoginResult GetAccessToken_EveAccountIsbelEveAccount(EveAccount myEveAccount, IsbelEveAccount myIsbelEveAccount)
        {
            Cache.Instance.Log("GetAccessToken: started");

            //FIXME
            //if (MyRefreshAndAccessTokensIfAny != null && !MyRefreshAndAccessTokensIfAny.IsEveAccesToken_ShortTerm_Expired)
            //{
            //    Cache.Instance.Log("GetAccessToken_EveAccountIsbelEveAccount: EveAccessToken: IsExpired [" + MyRefreshAndAccessTokensIfAny.IsEveAccesToken_ShortTerm_Expired + "] TokenString [" + MyRefreshAndAccessTokensIfAny.RefreshTokenString + "]");
            //    return LoginResult.Success;
            //}

            //if (MyRefreshAndAccessTokensIfAny != null && MyRefreshAndAccessTokensIfAny.IsEveAccesToken_ShortTerm_Expired && MyRefreshAndAccessTokensIfAny.RefreshTokenExists)
            //{
            //    Cache.Instance.Log("GetAccessToken_EveAccountIsbelEveAccount: EveAccessToken: IsExpired [" + MyRefreshAndAccessTokensIfAny.IsEveAccesToken_ShortTerm_Expired + "] RefreshTokenExists [" + MyRefreshAndAccessTokensIfAny.RefreshTokenExists + "] RefreshTokenString [" + MyRefreshAndAccessTokensIfAny.RefreshTokenString + "] EveAccessTokenString [" + MyRefreshAndAccessTokensIfAny.EveAccessTokenString + "]");
            //    GetRefreshToken(myEveAccount, myIsbelEveAccount);
            //    return LoginResult.Success;
            //}
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
                return LoginResult.InvalidUsernameOrPassword;
            }

            Cache.Instance.Log("GetAccessToken: GetLoginUri");
            var uri = RequestResponse.GetLoginUri(myEveAccount.ConnectToTestServer, State.ToString(), ChallengeHash);

            string RequestVerificationToken = string.Empty;
            LoginResult loginResult = GetRequestVerificationToken(uri, myEveAccount, myIsbelEveAccount, out RequestVerificationToken);
            if (loginResult == LoginResult.Error)
            {
                Cache.Instance.Log("GetRequestVerificationToken: if (result == LoginResult.Error)");
                return loginResult;
            }

            Cache.Instance.Log("GetRequestVerificationToken: completed: loginResult is [" + loginResult.ToString() + "]");
            var req = RequestResponse.CreatePostRequest(uri, myEveAccount.ConnectToTestServer, true, RequestResponse.refererUri, Cookies, myEveAccount.HWSettings.Proxy.Ip, myEveAccount.HWSettings.Proxy.HttpProxyPort);
            System.Threading.Thread.Sleep(Util.GetRandom(100, 300));

            using (SecureBytesWrapper body = new SecureBytesWrapper())
            {
                byte[] body1 = Encoding.ASCII.GetBytes(String.Format("__RequestVerificationToken={1}&UserName={0}&Password=", Uri.EscapeDataString(myEveAccount.AccountName), Uri.EscapeDataString(RequestVerificationToken)));
                using (SecureStringWrapper ssw = new SecureStringWrapper(SecurePassword, Encoding.ASCII))
                {
                    using (SecureBytesWrapper escapedPassword = new SecureBytesWrapper())
                    {
                        escapedPassword.Bytes = System.Web.HttpUtility.UrlEncodeToBytes(ssw.ToByteArray());

                        body.Bytes = new byte[body1.Length + escapedPassword.Bytes.Length];
                        System.Buffer.BlockCopy(body1, 0, body.Bytes, 0, body1.Length);
                        System.Buffer.BlockCopy(escapedPassword.Bytes, 0, body.Bytes, body1.Length, escapedPassword.Bytes.Length);
                        req.SetBody(body);
                    }
                }
            }

            Cache.Instance.Log("GetAccessToken: finished");
            return GetAccessToken_EveAccountIsbelEveAccountHttpWebRequest(myEveAccount, myIsbelEveAccount, req);
        }

        [Browsable(false)]
        public LoginResult GetSSOToken_EveAccount(EveAccount myEveAccount)
        {
            LoginResult lr = GetAccessToken_EveAccountIsbelEveAccount(myEveAccount, this);

            return lr;
        }

        #region INotifyPropertyChanged
        public new event PropertyChangedEventHandler PropertyChanged;

        public void FirePropertyChanged(string value)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(value));
            }
        }

        public new void OnPropertyChanged(string value)
        {
            FirePropertyChanged(value);
        }
        #endregion
        public void Dispose()
        {
        }
    }
}