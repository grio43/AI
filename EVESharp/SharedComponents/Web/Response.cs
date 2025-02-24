using System;
using System.Net;
using Newtonsoft.Json;
using SharedComponents.EVE;
using SharedComponents;
using SharedComponents.Extensions;
using SharedComponents.ISBELExtensions;
using System.IO;

namespace SharedComponents.Web
{
    public class Response
    {
        private Uri _requestUri = null;
        private string _origin;
        private string _referer;
        private HttpStatusCode _response = HttpStatusCode.Unused;
        private string _responseLocation;
        private string _responseBody;
        private Uri _responseUri = null;

        public Response(HttpWebRequest request)
        {
            if (request.Proxy == null)
            {
                Cache.Instance.Log("CreateHttpWebRequest: Proxy == null!");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
                request = (HttpWebRequest)HttpWebRequest.Create(new Uri("http://www.google.com"));
            }

            //request.Proxy = new WebProxy(proxyIp, int.Parse(proxyHttpPort));
            Cache.Instance.Log("Response(HttpWebRequest request): using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) starting");

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                Cache.Instance.Log("Response(HttpWebRequest request): using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()): running");
                //System.IO.Stream stream = response.GetResponseStream();
                //StreamReader reader = new StreamReader(stream);
                //string html = reader.ReadToEnd();
                //Cache.Instance.Log(html);

                _requestUri = request.RequestUri;
                _origin = request.Headers["Origin"];
                _referer = request.Referer;
                _response = response.StatusCode;
                _responseLocation = response.Headers["Location"];
                _responseBody = response.GetResponseBody();
                _responseUri = response.ResponseUri;
            }
        }


        //
        // used only is debugging and testing, you can set responseBody to the page content you expect to get back so you can test without sending requests to CCP...
        //
        public Response(HttpWebRequest request, string responseBody)
        {
            if (request.Proxy == null)
            {
                Cache.Instance.Log("CreateHttpWebRequest: Proxy == null!");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
                request = (HttpWebRequest)HttpWebRequest.Create(new Uri("http://www.google.com"));
            }

            _requestUri = request.RequestUri;
            _origin = request.Headers["Origin"];
            _referer = request.Referer;
            _response = HttpStatusCode.OK;
            _responseLocation = null;
            _responseBody = responseBody;
            _responseUri = _requestUri;
        }

        /**
        public Response(HttpWebRequest request, WebRequestType requestType, EveAccount myEveAccount)
        {
            if (request.Proxy == null)
            {
                Cache.Instance.Log("CreateHttpWebRequest: Proxy == null!");
                request = (HttpWebRequest)HttpWebRequest.Create(new Uri("http://www.google.com"));
            }

            _requestUri = request.RequestUri;
            _origin = request.Headers["Origin"];
            _referer = request.Referer;
            _response = HttpStatusCode.OK;
            _responseLocation = null;

            switch (requestType)
            {
                case WebRequestType.RequestVerificationToken:
                    _responseBody = myEveAccount.myLB.strHTML_RequestVerificationToken;
                    _responseUri = new Uri(myEveAccount.myLB.strURL_RequestVerificationToken, UriKind.Absolute);
                    break;
                case WebRequestType.VerficationCode:
                    _responseBody = myEveAccount.myLB.strHTML_VerficationCode;
                    _responseUri = new Uri(myEveAccount.myLB.strURL_VerficationCode, UriKind.Absolute);
                    break;
                case WebRequestType.Result:
                    _responseBody = myEveAccount.myLB.strHTML_Result;
                    _responseUri = new Uri(myEveAccount.myLB.strURL_Result, UriKind.Absolute);
                    break;
            }

        }
        **/

        public string Body
        {
            get { return _responseBody; }
            set { value = _responseBody; }
        }

        public Uri ResponseUri
        {
            get { return _responseUri; }
        }

        public bool IsHtml()
        {
            /**
            try
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(_responseBody);
            }
            catch (Exception)
            {
                return false;
            }
            **/
            return true;
        }

        public bool IsJson()
        {
            try
            {
                dynamic json = JsonConvert.DeserializeObject(_responseBody);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            return
                "RequestUri: " + _requestUri.ToString() + Environment.NewLine
                + "Origin: " + _origin + Environment.NewLine
                + "Referer: " + _referer + Environment.NewLine + Environment.NewLine
                + "ResponseCode: " + _response.ToString() + Environment.NewLine
                + "ResponseUri: " + _responseUri + Environment.NewLine
                + "Location: " + _responseLocation + Environment.NewLine + Environment.NewLine
                + "Body: " + Environment.NewLine + _responseBody + Environment.NewLine;
        }
    }
}