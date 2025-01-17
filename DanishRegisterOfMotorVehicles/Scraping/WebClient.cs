﻿using System;
using System.Net;

namespace DanishRegisterOfMotorVehicles.Api.Scraping
{
    public class WebClient : System.Net.WebClient
    {
        private WebRequest _request;

        public WebClient(CookieContainer cookies = null, bool autoRedirect = true)
        {
            CookieContainer = cookies ?? new CookieContainer();
            AutoRedirect = autoRedirect;
        }

        /// <summary>
        ///     Gets or sets whether to automatically follow a redirect
        /// </summary>
        public bool AutoRedirect { get; set; }

        /// <summary>
        ///     Gets or sets the cookie container, contains all the cookies for all the requests
        /// </summary>
        public CookieContainer CookieContainer { get; set; }

        /// <summary>
        ///     Gets last cookie header
        /// </summary>
        public string Cookies => GetHeaderValue("Set-Cookie");

        /// <summary>
        ///     Get last location header
        /// </summary>
        public string Location => GetHeaderValue("Location");

        /// <summary>
        ///     Get last request url
        /// </summary>
        public string Url => _request.RequestUri.ToString();

        /// <summary>
        ///     Get last status code
        /// </summary>
        public HttpStatusCode StatusCode
        {
            get
            {
                var result = HttpStatusCode.BadRequest;

                if (_request != null)
                {
                    var response = base.GetWebResponse(_request) as HttpWebResponse;
                    if (response != null)
                        result = response.StatusCode;
                }

                return result;
            }
        }

        /// <summary>
        ///     Get header value by headername
        /// </summary>
        public string GetHeaderValue(string headerName)
        {
            string result = null;
            if (_request != null)
            {
                var response = base.GetWebResponse(_request) as HttpWebResponse;
                if (response != null)
                    result = response.Headers[headerName];
            }

            return result;
        }

        /// <summary>
        ///     Override GetWebRequest on base (WebCLient)
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        protected override WebRequest GetWebRequest(Uri address)
        {
            _request = base.GetWebRequest(address);

            var httpRequest = _request as HttpWebRequest;
            if (httpRequest != null)
            {
                httpRequest.AllowAutoRedirect = AutoRedirect;
                httpRequest.CookieContainer = CookieContainer;
                httpRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }

            return _request;
        }
    }
}