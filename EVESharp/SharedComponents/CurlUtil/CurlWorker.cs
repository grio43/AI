/*
 * ---------------------------------------
 * User: duketwo
 * Date: 03.02.2014
 * Time: 11:32
 *
 * ---------------------------------------
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using SeasideResearch.LibCurlNet;
using SharedComponents.EVE;
using SharedComponents.Utility;

namespace SharedComponents.CurlUtil
{
    /// <summary>
    ///     Description of CurlWorker.
    /// </summary>
    public class CurlWorker : IDisposable
    {

        public static bool DisableSSLVerifcation;

        private const string UserAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.3";

        private static Object Lock = new object();
        private bool _persistentCookie;

        static CurlWorker()
        {
            Curl.GlobalInit((int)CURLinitFlag.CURL_GLOBAL_ALL);
        }

        public CurlWorker()
        {

            lock (Lock)
            {
                CreateDirectories();
                var path = Path.Combine(Utility.Util.AssemblyPath, "EveSharpSettings", "Cookies");
                CookieFile = Path.Combine(path, Guid.NewGuid().ToString("n") + DateTime.UtcNow.Ticks + Cnt.ToString() + ".cookie");
                Cnt++;
                while (File.Exists(CookieFile))
                {
                    Cnt++;
                    CookieFile = Path.Combine(path, Guid.NewGuid().ToString("n") + DateTime.UtcNow.Ticks + Cnt.ToString() + ".cookie");
                }
                try
                {
                    Util.Touch(CookieFile);
                }
                catch (Exception e)
                {
                    Cache.Instance.Log("Exception: " + e);
                }
            }
        }

        public CurlWorker(string cookieName)
        {
            lock (Lock)
            {
                CreateDirectories();
                var path = Path.Combine(Util.AssemblyPath, "EveSharpSettings", "Cookies");
                CookieFile = Path.Combine(path, cookieName + ".cookie");
                _persistentCookie = true;
                if (!File.Exists(CookieFile))
                {
                    try
                    {
                        Util.Touch(CookieFile);
                    }
                    catch (Exception e)
                    {
                        Cache.Instance.Log("Exception: " + e);
                    }
                }
            }
        }

        private void CreateDirectories()
        {
            var path = Util.AssemblyPath;
            if (!Directory.Exists(Path.Combine(path, "EveSharpSettings")))
            {
                Directory.CreateDirectory(Path.Combine(path, "EveSharpSettings"));
            }
            if (!Directory.Exists(Path.Combine(path, "EveSharpSettings", "Cookies")))
            {
                Directory.CreateDirectory(Path.Combine(path, "EveSharpSettings", "Cookies"));
            }
        }

        private string CookieFile { get; set; }
        private static int Cnt { get; set; }

        #region IDisposable

        public void Dispose()
        {
            DeleteCurrentSessionCookie();
        }

        #endregion

        ~CurlWorker()
        {
            DeleteCurrentSessionCookie();
        }

        public bool DeleteCurrentSessionCookie(bool force = false)
        {
            if (File.Exists(CookieFile) && (!_persistentCookie || force))
                try
                {
                    File.Delete(CookieFile);
                    var msg = string.Format("Deleted session cookie '{0}' file.", CookieFile);
                    Cache.Instance.Log(msg);
                    Debug.WriteLine(msg);
                    return true;
                }
                catch (Exception)
                {
                    Cache.Instance.Log(string.Format("Error: Couldn't delete session cookie '{0}' file.", CookieFile));
                }

            return false;
        }

        public string GetPostPage(string url, string postData, string proxyPort, string userPassword, bool followLocation = true, bool includeHeader = false, bool debug = false)
        {
            if (debug)
            {
                Cache.Instance.Log("URL: [ " + url + " ]");
                Cache.Instance.Log("postData: [ " + postData + " ]");
                Cache.Instance.Log("proxyPort: [ " + proxyPort + " ]");
                Cache.Instance.Log("userPassword: [" + userPassword + " ]");
                Cache.Instance.Log("folowlocation: [ " + followLocation + " ]");
                Cache.Instance.Log("includeHeader: [ " + includeHeader + " ]");
            }

            Easy easy = null;
            var writer = new CurlWriter();
            try
            {
                easy = new Easy();
                Easy.WriteFunction wf = writer.WriteData;
                easy.SetOpt(CURLoption.CURLOPT_URL, url);
                easy.SetOpt(CURLoption.CURLOPT_WRITEFUNCTION, wf);
                easy.SetOpt(CURLoption.CURLOPT_SSL_VERIFYHOST, 2);
                var verifyPeer = DisableSSLVerifcation ? 0 : 1;
                    easy.SetOpt(CURLoption.CURLOPT_SSL_VERIFYPEER, verifyPeer);
                easy.SetOpt(CURLoption.CURLOPT_CAINFO, "curl-ca-bundle.crt");
                if (!string.IsNullOrEmpty(proxyPort)) easy.SetOpt(CURLoption.CURLOPT_PROXY, proxyPort);
                if (!string.IsNullOrEmpty(userPassword)) easy.SetOpt(CURLoption.CURLOPT_PROXYUSERPWD, userPassword);
                if (!string.IsNullOrEmpty(proxyPort)) easy.SetOpt(CURLoption.CURLOPT_PROXYTYPE, CURLproxyType.CURLPROXY_SOCKS5);
                easy.SetOpt(CURLoption.CURLOPT_USERAGENT, UserAgent);
                easy.SetOpt(CURLoption.CURLOPT_COOKIEFILE, CookieFile);
                easy.SetOpt(CURLoption.CURLOPT_COOKIEJAR, CookieFile);
                if (followLocation) easy.SetOpt(CURLoption.CURLOPT_FOLLOWLOCATION, 1);
                easy.SetOpt(CURLoption.CURLOPT_AUTOREFERER, 1);
                easy.SetOpt(CURLoption.CURLOPT_CONNECTTIMEOUT, 60L);
                easy.SetOpt(CURLoption.CURLOPT_TIMEOUT, 60L);
                if (includeHeader) easy.SetOpt(CURLoption.CURLOPT_HEADER, true);
                if (!string.IsNullOrEmpty(postData)) easy.SetOpt(CURLoption.CURLOPT_POSTFIELDS, postData);
                easy.Perform();
                return writer.CurrentPage;
            }
            catch (Exception exp)
            {
                if (exp is ThreadAbortException)
                    writer.CurrentPage = string.Empty;
                Cache.Instance.Log("Exception: " + exp.StackTrace);
            }
            finally
            {
                try
                {
                    if (easy != null)
                        easy.Dispose();
                }
                catch (Exception exp)
                {
                    Cache.Instance.Log("Exception " + exp.StackTrace);
                }
            }
            return writer.CurrentPage;
        }

        public static bool CheckInternetConnectivity(Proxy myProxy, CURLproxyType myProxyType)
        {
            Easy easy = null;
            var writer = new CurlWriter();
            try
            {
                easy = new Easy();
                Easy.WriteFunction wf = writer.WriteData;
                easy.SetOpt(CURLoption.CURLOPT_URL, "https://www.google.com");
                easy.SetOpt(CURLoption.CURLOPT_WRITEFUNCTION, wf);
                easy.SetOpt(CURLoption.CURLOPT_SSL_VERIFYHOST, 2);
                easy.SetOpt(CURLoption.CURLOPT_SSL_VERIFYPEER, 1);
                easy.SetOpt(CURLoption.CURLOPT_CAINFO, "curl-ca-bundle.crt");
                easy.SetOpt(CURLoption.CURLOPT_FOLLOWLOCATION, 1);
                easy.SetOpt(CURLoption.CURLOPT_HEADER, 1);
                easy.SetOpt(CURLoption.CURLOPT_NOBODY, 1);
                if (!string.IsNullOrEmpty(myProxy.Password)) easy.SetOpt(CURLoption.CURLOPT_PROXYUSERPWD, myProxy.Password);
                easy.SetOpt(CURLoption.CURLOPT_PROXYTYPE, myProxyType);
                easy.SetOpt(CURLoption.CURLOPT_PROXY, myProxy.Ip);

                if (myProxyType == CURLproxyType.CURLPROXY_SOCKS5)
                {
                    Cache.Instance.Log("HookManagerImpl: CurlWorker: ProxyType [" + myProxyType + "] Socks5Port [" + myProxy.Socks5Port + "]");
                    easy.SetOpt(CURLoption.CURLOPT_PROXYPORT, myProxy.Socks5Port);
                }
                else if (myProxyType == CURLproxyType.CURLPROXY_HTTP)
                {
                    Cache.Instance.Log("HookManagerImpl: CurlWorker: ProxyType [" + myProxyType + "] HttpProxyPort [" + myProxy.HttpProxyPort + "]");
                    easy.SetOpt(CURLoption.CURLOPT_PROXYPORT, myProxy.HttpProxyPort);
                }

                easy.SetOpt(CURLoption.CURLOPT_USERAGENT, UserAgent);
                easy.SetOpt(CURLoption.CURLOPT_AUTOREFERER, 1);
                easy.SetOpt(CURLoption.CURLOPT_CONNECTTIMEOUT, 5L);
                easy.SetOpt(CURLoption.CURLOPT_TIMEOUT, 10L);
                easy.Perform();
                if (writer.CurrentPage != null)
                {
                    Cache.Instance.Log("CheckInternetConnectivity: [" + writer.CurrentPage + "]");
                    bool tempIsAlive = writer.CurrentPage != null && (writer.CurrentPage.ToUpper().Contains("200 OK") || writer.CurrentPage.ToUpper().Contains("302 FOUND"));
                    myProxy.IsAlive = tempIsAlive;
                    return tempIsAlive;
                }

                Cache.Instance.Log("CheckInternetConnectivity: [" + false + " !!]");
                return false;
            }
            catch (Exception ex)
            {
                if (ex is ThreadAbortException)
                    writer.CurrentPage = string.Empty;
                Cache.Instance.Log("Exception [" + ex + "]");
                return false;
            }
            finally
            {
                try
                {
                    if (easy != null)
                        easy.Dispose();
                }
                catch (Exception exp)
                {
                    Cache.Instance.Log("Exception: " + exp.StackTrace);
                }
            }
        }

        public static bool CheckTorExit(string proxyPort, string userPassword)
        {
            Easy easy = null;
            var writer = new CurlWriter();
            try
            {
                easy = new Easy();
                Easy.WriteFunction wf = writer.WriteData;
                easy.SetOpt(CURLoption.CURLOPT_URL, "https://check.torproject.org");
                easy.SetOpt(CURLoption.CURLOPT_WRITEFUNCTION, wf);
                easy.SetOpt(CURLoption.CURLOPT_SSL_VERIFYHOST, 2);
                easy.SetOpt(CURLoption.CURLOPT_SSL_VERIFYPEER, 1);
                easy.SetOpt(CURLoption.CURLOPT_CAINFO, "curl-ca-bundle.crt");
                easy.SetOpt(CURLoption.CURLOPT_FOLLOWLOCATION, 0);
                easy.SetOpt(CURLoption.CURLOPT_HEADER, 1);
                if (!string.IsNullOrEmpty(proxyPort)) easy.SetOpt(CURLoption.CURLOPT_PROXY, proxyPort);
                if (!string.IsNullOrEmpty(userPassword)) easy.SetOpt(CURLoption.CURLOPT_PROXYUSERPWD, userPassword);
                if (!string.IsNullOrEmpty(proxyPort)) easy.SetOpt(CURLoption.CURLOPT_PROXYTYPE, CURLproxyType.CURLPROXY_SOCKS5);
                easy.SetOpt(CURLoption.CURLOPT_USERAGENT, UserAgent);
                easy.SetOpt(CURLoption.CURLOPT_AUTOREFERER, 1);
                easy.SetOpt(CURLoption.CURLOPT_CONNECTTIMEOUT, 5L);
                easy.SetOpt(CURLoption.CURLOPT_TIMEOUT, 10L);
                easy.Perform();
                return writer.CurrentPage != null && !writer.CurrentPage.Contains("tor-off.png");
            }
            catch (Exception exp)
            {
                if (exp is ThreadAbortException)
                    writer.CurrentPage = string.Empty;
                Cache.Instance.Log("Exception: " + exp.StackTrace);
                return false;
            }
            finally
            {
                try
                {
                    if (easy != null)
                        easy.Dispose();
                }
                catch (Exception exp)
                {
                    Cache.Instance.Log("Exception: " + exp.StackTrace);
                }
            }
        }

        public Byte[] RetrieveImage(string url, string proxyPort, string userPassword)
        {
            Easy easy = null;
            var writer = new CurlWriter();
            try
            {
                easy = new Easy();
                Easy.WriteFunction wf = writer.WriteData;
                easy.SetOpt(CURLoption.CURLOPT_URL, url);
                easy.SetOpt(CURLoption.CURLOPT_WRITEFUNCTION, wf);
                easy.SetOpt(CURLoption.CURLOPT_SSL_VERIFYHOST, 2);
                easy.SetOpt(CURLoption.CURLOPT_SSL_VERIFYPEER, 1);
                easy.SetOpt(CURLoption.CURLOPT_CAINFO, "curl-ca-bundle.crt");
                easy.SetOpt(CURLoption.CURLOPT_FOLLOWLOCATION, 1);
                easy.SetOpt(CURLoption.CURLOPT_HEADER, 0);
                if (!string.IsNullOrEmpty(proxyPort)) easy.SetOpt(CURLoption.CURLOPT_PROXY, proxyPort);
                if (!string.IsNullOrEmpty(userPassword)) easy.SetOpt(CURLoption.CURLOPT_PROXYUSERPWD, userPassword);
                if (!string.IsNullOrEmpty(proxyPort)) easy.SetOpt(CURLoption.CURLOPT_PROXYTYPE, CURLproxyType.CURLPROXY_SOCKS5);
                easy.SetOpt(CURLoption.CURLOPT_USERAGENT, UserAgent);
                easy.SetOpt(CURLoption.CURLOPT_AUTOREFERER, 1);
                easy.SetOpt(CURLoption.CURLOPT_CONNECTTIMEOUT, 5L);
                easy.SetOpt(CURLoption.CURLOPT_TIMEOUT, 10L);
                var r = easy.Perform();

                if (r == CURLcode.CURLE_OK)
                {
                    Debug.WriteLine("Code: CURLcode.CURLE_OK, Image downloaded successfully.");
                    return writer.ByteArr;
                }

                Debug.WriteLine("Error downloading the image.");
                return new List<Byte>().ToArray();
            }
            catch (Exception ex)
            {
                if (ex is ThreadAbortException)
                    writer.CurrentPage = string.Empty;
                Cache.Instance.Log("Exception [" + ex + "]");
                return new List<Byte>().ToArray();
            }
            finally
            {
                try
                {
                    if (easy != null)
                        easy.Dispose();
                }
                catch (Exception ex)
                {
                    Cache.Instance.Log("Exception [" + ex + "]");
                }
            }
        }
    }
}