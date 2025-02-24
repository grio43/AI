using System;
using System.Net;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SharedComponents.EVE;
using SharedComponents.ISBELExtensions;
using SharedComponents.Utility;
using SharedComponents;
using Newtonsoft.Json;
using SharedComponents.BrowserAutomation;

namespace SharedComponents.Web
{
    public static class RequestResponse
    {
        //public const string logoff = "/account/logoff";
        public const string auth = "/v2/oauth/authorize";
        public const string eula = "/v2/oauth/eula";
        public const string logon = "/account/logon";
        public const string launcher = "launcher";
        public const string token = "/v2/oauth/token";
        public const string tqBaseUri = "https://login.eveonline.com";
        public const string sisiBaseUri = "https://sisilogin.testeveonline.com";
        public const string verifyTwoFactor = "/account/verifytwofactor";

        //public const string logonRetURI = "ReturnUrl=/v2/oauth/authorize?client_id=eveLauncherTQ&response_type=code&scope=eveClientLogin%20cisservice.customerRead.v1%20cisservice.customerWrite.v1";
        //public const string logonRedirectURI = "redirect_uri={0}/launcher?client_id=eveLauncherTQ&state={1}&code_challenge_method=S256&code_challenge={2}&showRemember=true";

        public const string originUri = "https://launcher.eveonline.com";
        public const string refererUri = "https://launcher.eveonline.com/6-0-x/6.4.15/";

        public static Uri GetLoginUri(bool sisi, string state, string challengeHash)
        {
            return new Uri(logon, UriKind.Relative)
                .AddQuery("ReturnUrl",
                    new Uri(auth, UriKind.Relative)
                        .AddQuery("client_id", "eveLauncherTQ")
                        .AddQuery("response_type", "code")
                        .AddQuery("scope", "eveClientLogin cisservice.customerRead.v1 cisservice.customerWrite.v1")
                        .AddQuery("redirect_uri", new Uri(new Uri(sisi ? sisiBaseUri : tqBaseUri), launcher)
                            .AddQuery("client_id", "eveLauncherTQ").ToString())
                        .AddQuery("state", state)
                        .AddQuery("code_challenge_method", "S256")
                        .AddQuery("code_challenge", challengeHash)
                        .AddQuery("showRemember", "true").ToString());
        }

        public static Uri GetRefreshCodeUri(bool sisi, string state, string challengeHash)
        {
            return new Uri(logon, UriKind.Relative)
                .AddQuery("ReturnUrl",
                    new Uri(auth, UriKind.Relative)
                        .AddQuery("client_id", "eveLauncherTQ")
                        .AddQuery("response_type", "code")
                        .AddQuery("scope", "eveClientLogin cisservice.customerRead.v1 cisservice.customerWrite.v1")
                        .AddQuery("redirect_uri", new Uri(new Uri(sisi ? sisiBaseUri : tqBaseUri), launcher)
                            .AddQuery("client_id", "eveLauncherTQ").ToString())
                        .AddQuery("state", state)
                        .AddQuery("code_challenge_method", "S256")
                        .AddQuery("code_challenge", challengeHash)
                        .AddQuery("showRemember", "true").ToString());
        }


        public static Uri GetSecurityWarningChallenge(bool sisi, string state, string challengeHash)
        {
            //https://login.eveonline.com/v2/oauth/authorize?
            //client_id =eveLauncherTQ
            //&amp;response_type=code
            //&amp;scope=eveClientLogin%20cisservice.customerRead.v1%20cisservice.customerWrite.v1
            //&amp;redirect_uri=https%3A%2F%2Flogin.eveonline.com%2Flauncher%3Fclient_id%3DeveLauncherTQ
            //&amp;state=5617f90c-efdb-41a1-b00d-6f4f24bbeee4
            //&amp;code_challenge_method=S256
            //&amp;code_challenge=nC-B19HKX8ZZYfOEN_bg-YZSjVAMieqEB3nJXFyfQQc
            //&amp;showRemember=true

            return new Uri(auth, UriKind.Relative)
                .AddQuery("client_id", "eveLauncherTQ")
                .AddQuery("response_type", "code")
                .AddQuery("scope", "eveClientLogin cisservice.customerRead.v1 cisservice.customerWrite.v1")
                .AddQuery("redirect_uri", new Uri(new Uri(sisi ? sisiBaseUri : tqBaseUri), launcher)
                    .AddQuery("client_id", "eveLauncherTQ").ToString())
                .AddQuery("state", state)
                .AddQuery("code_challenge_method", "S256")
                .AddQuery("code_challenge", challengeHash)
                .AddQuery("showRemember", "true");
        }

        public static HttpWebRequest CreateGetRequest(Uri uri, bool sisi, bool origin, string referer, CookieContainer cookies, string proxyIp, string proxyHttpPort)
        {
            System.Threading.Thread.Sleep(Util.GetRandom(100, 300));

            if (!uri.IsAbsoluteUri)
                uri = new Uri(string.Concat(sisi ? sisiBaseUri : tqBaseUri, uri.ToString()));

            Cache.Instance.Log("CreateGetRequest: uri is [" + uri.AbsoluteUri + "]");

            return CreateHttpWebRequest(uri, "GET", sisi, origin, referer, cookies, proxyIp, proxyHttpPort);
        }

		public static HttpWebRequest CreatePostRequest(Uri uri, bool sisi, bool origin, string referer, CookieContainer cookies, string proxyIp, string proxyHttpPort)
        {
            System.Threading.Thread.Sleep(Util.GetRandom(100, 300));

            if (!uri.IsAbsoluteUri)
                uri = new Uri(string.Concat(sisi ? sisiBaseUri : tqBaseUri, uri.ToString()));
            return CreateHttpWebRequest(uri, "POST", sisi, origin, referer, cookies, proxyIp, proxyHttpPort);
        }

        public static byte[] GetSsoTokenRequestBodyGrantTypeAuthorizationCode(bool sisi, string authCode, byte[] challengeCode)
        {
            return
                Encoding.UTF8.GetBytes(new Uri("/", UriKind.Relative)
                .AddQuery("grant_type", "authorization_code")
                .AddQuery("client_id", "eveLauncherTQ")
                .AddQuery("redirect_uri", new Uri(new Uri(sisi ? sisiBaseUri : tqBaseUri), launcher)
                    .AddQuery("client_id", "eveLauncherTQ").ToString())
                .AddQuery("code", authCode)
                .AddQuery("code_verifier", Base64UrlEncoder.Encode(challengeCode)).SafeQuery());
        }

        public static byte[] GetSsoTokenRequestBodyGrantTypeRefreshCode(bool sisi, string savedRefreshToken, byte[] challengeCode)
        {
            return
                Encoding.UTF8.GetBytes(new Uri("/", UriKind.Relative)
                .AddQuery("grant_type", "refresh_token")
                .AddQuery("refresh_token", savedRefreshToken)
                .AddQuery("client_id", "eveLauncherTQ")
                .AddQuery("redirect_uri", new Uri(new Uri(sisi ? sisiBaseUri : tqBaseUri), launcher)
                    .AddQuery("client_id", "eveLauncherTQ").ToString())
                //.AddQuery("code", authCode)
                .AddQuery("code_verifier", Base64UrlEncoder.Encode(challengeCode)).SafeQuery());
        }

        public static Uri GetVerifyTwoFactorUri(bool sisi, string state, string challengeHash)
        {
            return new Uri(verifyTwoFactor, UriKind.Relative)
                .AddQuery("ReturnUrl",
                    new Uri(auth, UriKind.Relative)
                        .AddQuery("client_id", "eveLauncherTQ")
                        .AddQuery("response_type", "code")
                        .AddQuery("scope", "eveClientLogin cisservice.customerRead.v1 cisservice.customerWrite.v1")
                        .AddQuery("redirect_uri", new Uri(new Uri(sisi ? sisiBaseUri : tqBaseUri), launcher)
                            .AddQuery("client_id", "eveLauncherTQ").ToString())
                        .AddQuery("state", state)
                        .AddQuery("code_challenge_method", "S256")
                        .AddQuery("code_challenge", challengeHash)
                        .AddQuery("showRemember", "true").ToString());
        }

        public static Uri GetAuthenticatorUri(bool sisi, string state, string challengeHash)
        {
            return new Uri("/account/authenticator", UriKind.Relative)
                .AddQuery("ReturnUrl",
                    new Uri(auth, UriKind.Relative)
                        .AddQuery("client_id", "eveLauncherTQ")
                        .AddQuery("response_type", "code")
                        .AddQuery("scope", "eveClientLogin cisservice.customerRead.v1 cisservice.customerWrite.v1")
                        .AddQuery("redirect_uri", new Uri(new Uri(sisi ? sisiBaseUri : tqBaseUri), launcher)
                            .AddQuery("client_id", "eveLauncherTQ").ToString())
                        .AddQuery("state", state)
                        .AddQuery("code_challenge_method", "S256")
                        .AddQuery("code_challenge", challengeHash)
                        .AddQuery("showRemember", "true").ToString());
        }

        public static Uri GetEulaUri(bool sisi, string state, string challengeHash)
        {
            return new Uri("/account/eula", UriKind.Relative)
                .AddQuery("ReturnUrl",
                    new Uri(auth, UriKind.Relative)
                        .AddQuery("client_id", "eveLauncherTQ")
                        .AddQuery("response_type", "code")
                        .AddQuery("scope", "eveClientLogin cisservice.customerRead.v1 cisservice.customerWrite.v1")
                        .AddQuery("redirect_uri", new Uri(new Uri(sisi ? sisiBaseUri : tqBaseUri), launcher)
                            .AddQuery("client_id", "eveLauncherTQ").ToString())
                        .AddQuery("state", state)
                        .AddQuery("code_challenge_method", "S256")
                        .AddQuery("code_challenge", challengeHash)
                        .AddQuery("showRemember", "true").ToString());
        }

        public static Uri GetCharacterChallengeUri(bool sisi, string state, string challengeHash)
        {
            return new Uri("/account/character", UriKind.Relative)
                .AddQuery("ReturnUrl",
                    new Uri(auth, UriKind.Relative)
                        .AddQuery("client_id", "eveLauncherTQ")
                        .AddQuery("response_type", "code")
                        .AddQuery("scope", "eveClientLogin cisservice.customerRead.v1 cisservice.customerWrite.v1")
                        .AddQuery("redirect_uri", new Uri(new Uri(sisi ? sisiBaseUri : tqBaseUri), launcher)
                            .AddQuery("client_id", "eveLauncherTQ").ToString())
                        .AddQuery("state", state)
                        .AddQuery("code_challenge_method", "S256")
                        .AddQuery("code_challenge", challengeHash)
                        .AddQuery("showRemember", "true").ToString());

            //"POST /account/verifytwofactor?ReturnUrl=%2Fv2%2Foauth%2Fauthorize%3Fclient_id%3DeveLauncherTQ%26response_type%3Dcode%26scope%3DeveClientLogin%2520cisservice.customerRead.v1%2520cisservice.customerWrite.v1%26redirect_uri%3Dhttps%253A%252F%252Fsisilogin.testeveonline.com%252Flauncher%253Fclient_id%253DeveLauncherTQ%26state%3D1043d900-ab13-42f3-a741-285cce0c8b47%26code_challenge_method%3DS256%26code_challenge%3DC0emnYPGUFfgXiyQx9d47zMM3uUXb6H9JB-PLptvtZ4%26showRemember%3Dtrue HTTP/1.1"
        }

        private static HttpWebRequest CreateHttpWebRequest(Uri uri, string methodType, bool sisi, bool origin, string referer, CookieContainer cookies, string proxyIp, string proxyHttpPort)
        {
            System.Threading.Thread.Sleep(Util.GetRandom(100, 300));
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(uri);
            req.Method = methodType;
            req.Timeout = 30000;
            req.AllowAutoRedirect = true;

            var t = req.Headers.GetType();

            var customHeaders = new CustomWebHeaderCollection(new System.Collections.Generic.Dictionary<string, string>()
            {
                ["User-Agent"] = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.81 Safari/537.36",
            });
            req.SetCustomheaders(customHeaders);

            if (!string.IsNullOrEmpty(proxyHttpPort) && proxyHttpPort != "0")
            {
                Cache.Instance.Log("CreateHttpWebRequest: [" + methodType + "] Using HTTP Proxy on [" + proxyIp + "] port [" + proxyHttpPort + "]");
                req.Proxy = new WebProxy(proxyIp, int.Parse(proxyHttpPort));
            }

            if (req.Proxy == null)
            {
                Cache.Instance.Log("CreateHttpWebRequest: Proxy == null!");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
                return (HttpWebRequest)HttpWebRequest.Create(new Uri("http://www.google.com"));
            }

            if (origin)
            {
                if (referer == refererUri)
                {
                    req.Headers.Add("Origin", originUri);
                }
                else
                {
                    if (!sisi)
                    {
                        req.Headers.Add("Origin", tqBaseUri);
                    }
                    else
                    {
                        req.Headers.Add("Origin", sisiBaseUri);
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(referer))
            {
                if (referer == "URL")
                {
                    req.Referer = req.RequestUri.ToString();
                }
                else
                {
                    req.Referer = referer;
                }
            }

            if (cookies != null)
            {
                req.CookieContainer = cookies;
            }
            else Cache.Instance.Log("CreateHttpWebRequest: cookies == null!?");

            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = 0;
            Cache.Instance.Log("CreateHttpWebRequest: req [" + req.RequestUri + "]");
            return req;
        }

        public static LoginResult GetHttpWebResponseAccessToken(HttpWebRequest webRequest, Action updateCookies, EveAccount myEveAccount, IsbelEveAccount myIsbelEveAccount, out Response response)
        {
            response = null;
            //bool useSeleniumn = false;

            try
            {
                //
                // "Normal" EVE SSO login method
                //
                response = new Response(webRequest);
                if (!string.IsNullOrEmpty(response.Body) && response.IsJson())
                {
                    var tempToken = new IsbelEveAccount.Token(JsonConvert.DeserializeObject<IsbelEveAccount.AuthObj>(response.Body));
                    myEveAccount.EveAccessTokenString = tempToken._authObj.access_token;
                    myEveAccount.EveAccessTokenValidUntil = tempToken._authObj.Expiration;

                    Cache.Instance.Log("GetHttpWebResponseAccessToken: EveAccesToken Retrieved Successfully");
                    Cache.Instance.Log("EveAccessTokenString [" + myEveAccount.EveAccessTokenString + "]");
                    Cache.Instance.Log("EveAccessTokenValidUntil [" + myEveAccount.EveAccessTokenValidUntil + "]");
                    //Cache.Instance.Log("RefreshTokenString [" + myEveAccount.RefreshTokenString + "]");
                    //Cache.Instance.Log("RefreshTokenValidUntil [" + myEveAccount.RefreshTokenValidUntil + "]");

                    if (updateCookies != null)
                    {
                        Cache.Instance.Log("GetHttpWebResponseAccessToken: updateCookies: Storing Cookies for later use");
                        updateCookies();
                    }
                }
                else if (!response.IsJson())
                {
                    Cache.Instance.Log("GetHttpWebResponseAccessToken: !response.IsJson");
                }
                else Cache.Instance.Log("GetHttpWebResponseAccessToken: response.Body is blank!");
            }
            catch (System.Net.WebException ex)
            {
                string rawResponseBody = string.Empty;
                if (response != null) rawResponseBody = response.Body;

                switch (ex.Status)
                {
                    case WebExceptionStatus.Timeout:
                        response = null;
                        Cache.Instance.Log("GetHttpWebResponseAccessToken: WebExceptionStatus.Timeout");
                        Cache.Instance.Log("Exception [" + ex + "]");
                        Cache.Instance.Log("response: [" + rawResponseBody + "]");
                        return LoginResult.Timeout;

                    case WebExceptionStatus.ProtocolError:
                        response = null;
                        Cache.Instance.Log("GetHttpWebResponseAccessToken: WebExceptionStatus.ProtocolError");
                        Cache.Instance.Log("Exception [" + ex + "]");
                        Cache.Instance.Log("response: [" + rawResponseBody + "]");
                        return LoginResult.Error;
                    default:
                        Cache.Instance.Log("!! Exception !! [" + ex + "]");
                        Cache.Instance.Log("response: [" + rawResponseBody + "]");
                        Cache.Instance.Log("Disabling Scheduler for [" + myEveAccount.MaskedAccountName + "]");
                        myEveAccount.UseScheduler = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
            }

            string responseBody = string.Empty;
            if (response != null) responseBody = response.Body;
            Cache.Instance.Log("LoginResult.Success: response: [" + responseBody + "]");
            return LoginResult.Success;
        }

        public static LoginResult GetHttpWebResponseRefreshToken(HttpWebRequest webRequest, Action updateCookies, EveAccount myEveAccount, IsbelEveAccount myIsbelEveAccount, out Response response)
        {
            response = null;
            //bool useSeleniumn = false;

            try
            {
                //
                // "Normal" EVE SSO login method
                //
                response = new Response(webRequest);
                if (!string.IsNullOrEmpty(response.Body) && response.IsJson())
                {
                    var tempToken = new IsbelEveAccount.Token(JsonConvert.DeserializeObject<IsbelEveAccount.AuthObj>(response.Body));

                    myEveAccount.EveAccessTokenString = tempToken._authObj.access_token;
                    myEveAccount.EveAccessTokenValidUntil = DateTime.Now.AddSeconds(tempToken._authObj.Expires_in);
                    myEveAccount.RefreshTokenString = tempToken._authObj.Refresh_token;
                    myEveAccount.RefreshTokenValidUntil = DateTime.Now.AddDays(90);
                    Cache.Instance.Log("GetHttpWebResponseRefreshToken: SSO Token Retrieved Successfully");
                    Cache.Instance.Log("EveAccessTokenString [" + myEveAccount.EveAccessTokenString + "]");
                    Cache.Instance.Log("EveAccessTokenValidUntil [" + myEveAccount.EveAccessTokenValidUntil + "]");
                    Cache.Instance.Log("RefreshTokenString [" + myEveAccount.RefreshTokenString + "]");
                    Cache.Instance.Log("RefreshTokenValidUntil [" + myEveAccount.RefreshTokenValidUntil + "]");

                    if (updateCookies != null)
                    {
                        Cache.Instance.Log("GetHttpWebResponseRefreshToken: updateCookies: Storing Cookies for later use");
                        updateCookies();
                    }
                }
                else if (!response.IsJson())
                {
                    Cache.Instance.Log("GetHttpWebResponseRefreshToken: !response.IsJson");
                }
                else Cache.Instance.Log("GetHttpWebResponseRefreshToken: response.Body is blank!");
            }
            catch (System.Net.WebException ex)
            {
                string rawResponseBody = string.Empty;
                if (response != null) rawResponseBody = response.Body;

                switch (ex.Status)
                {
                    case WebExceptionStatus.Timeout:
                        response = null;
                        Cache.Instance.Log("GetHttpWebResponseRefreshToken: WebExceptionStatus.Timeout");
                        Cache.Instance.Log("Exception [" + ex + "]");
                        Cache.Instance.Log("response: [" + rawResponseBody + "]");
                        return LoginResult.Timeout;

                    case WebExceptionStatus.ProtocolError:
                        response = null;
                        Cache.Instance.Log("GetHttpWebResponseRefreshToken: WebExceptionStatus.ProtocolError");
                        Cache.Instance.Log("Exception [" + ex + "]");
                        Cache.Instance.Log("response: [" + rawResponseBody + "]");
                        return LoginResult.Error;
                    default:
                        Cache.Instance.Log("!! Exception !! [" + ex + "]");
                        Cache.Instance.Log("response: [" + rawResponseBody + "]");
                        Cache.Instance.Log("Disabling Scheduler for [" + myEveAccount.MaskedAccountName + "]");
                        myEveAccount.UseScheduler = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception [" + ex + "]");
            }

            string responseBody = string.Empty;
            if (response != null) responseBody = response.Body;
            Cache.Instance.Log("LoginResult.Success: response: [" + responseBody + "]");
            return LoginResult.Success;
        }


        public static string GetRequestVerificationTokenResponse(Response response)
        {
            try
            {
                // <input name="__RequestVerificationToken" type="hidden" value="rGFOR5OvmlpJ_6_Kabcx3JSrJ3v6EL0W6tuOuD-e8QvUuK2l1MX5jP7pztjxnm5k0qgHIv-mati2ctst9M8kD9jBg3E1" />
                const string needle = "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"";
                int hashStart = response.Body.IndexOf(needle, StringComparison.Ordinal);
                if (hashStart == -1)
                    return null;

                hashStart += needle.Length;

                // get hash end
                int hashEnd = response.Body.IndexOf('"', hashStart);
                if (hashEnd == -1)
                    return null;

                return response.Body.Substring(hashStart, hashEnd - hashStart);
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("!!!! Exception !!!! [" + ex + "]");
                Cache.Instance.Log("response.Body [" + response.Body + "]");
                return null;
            }
        }

        public static string GetEulaHashFromBody(string body)
        {
            const string needle = "name=\"eulaHash\" type=\"hidden\" value=\"";
            int hashStart = body.IndexOf(needle, StringComparison.Ordinal);
            if (hashStart == -1)
                return null;
            return body.Substring(hashStart + needle.Length, 32);
        }

        public static string GetEulaReturnUrlFromBody(string body)
        {
            const string needle = "input id=\"returnUrl\" name=\"returnUrl\" type=\"hidden\" value=\"";
            int fieldStart = body.IndexOf(needle, StringComparison.Ordinal);
            if (fieldStart == -1)
                return null;

            fieldStart += needle.Length;
            int fieldEnd = body.IndexOf('"', fieldStart);

            return body.Substring(fieldStart, fieldEnd - fieldStart);
        }
    }
}