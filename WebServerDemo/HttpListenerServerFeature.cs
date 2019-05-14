using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebServerDemo
{
    public class HttpListenerServerFeature : IHttpRequestFeature, IHttpResponseFeature
    {
        private readonly HttpListenerContext httpListenerContext;
        private string queryString;
        private IHeaderDictionary requestHeaders;
        private IHeaderDictionary responseHeaders;
        private string protocol;
        private readonly string pathBase;

        public HttpListenerServerFeature(HttpListenerContext httpListenerContext, string pathBase)
        {
            this.httpListenerContext = httpListenerContext;
            this.pathBase = pathBase;
        }

        #region IHttpRequestFeature

        Stream IHttpRequestFeature.Body
        {
            get { return httpListenerContext.Request.InputStream; }
            set { throw new NotImplementedException(); }
        }

        IHeaderDictionary IHttpRequestFeature.Headers
        {
            get
            {
                return requestHeaders
           ?? (requestHeaders = GetHttpHeaders(httpListenerContext.Request.Headers));
            }
            set { throw new NotImplementedException(); }
        }

        string IHttpRequestFeature.Method
        {
            get { return httpListenerContext.Request.HttpMethod; }
            set { throw new NotImplementedException(); }
        }

        string IHttpRequestFeature.Path
        {
            get { return httpListenerContext.Request.RawUrl.Substring(pathBase.Length); }
            set { throw new NotImplementedException(); }
        }

        string IHttpRequestFeature.PathBase
        {
            get { return pathBase; }
            set { throw new NotImplementedException(); }
        }

        string IHttpRequestFeature.Protocol
        {
            get { return protocol ?? (protocol = this.GetProtocol()); }
            set { throw new NotImplementedException(); }
        }

        string IHttpRequestFeature.QueryString
        {
            get { return queryString ?? (queryString = this.ResolveQueryString()); }
            set { throw new NotImplementedException(); }
        }

        string IHttpRequestFeature.Scheme
        {
            get { return httpListenerContext.Request.IsWebSocketRequest ? "https" : "http"; }
            set { throw new NotImplementedException(); }
        }
        #endregion

        #region IHttpResponseFeature
        Stream IHttpResponseFeature.Body
        {
            get { return httpListenerContext.Response.OutputStream; }
            set { throw new NotImplementedException(); }
        }

        string IHttpResponseFeature.ReasonPhrase
        {
            get { return httpListenerContext.Response.StatusDescription; }
            set { httpListenerContext.Response.StatusDescription = value; }
        }

        bool IHttpResponseFeature.HasStarted
        {
            get { return httpListenerContext.Response.SendChunked; }
        }

        IHeaderDictionary IHttpResponseFeature.Headers
        {
            get
            {
                return responseHeaders
              ?? (responseHeaders = GetHttpHeaders(httpListenerContext.Response.Headers));
            }
            set { throw new NotImplementedException(); }
        }
        int IHttpResponseFeature.StatusCode
        {
            get { return httpListenerContext.Response.StatusCode; }
            set { httpListenerContext.Response.StatusCode = value; }
        }

        public string RawTarget { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        void IHttpResponseFeature.OnCompleted(Func<object, Task> callback, object state)
        {
            throw new NotImplementedException();
        }

        void IHttpResponseFeature.OnStarting(Func<object, Task> callback, object state)
        {
            throw new NotImplementedException();
        }
        #endregion

        private string ResolveQueryString()
        {
            string queryString = "";
            var collection = httpListenerContext.Request.QueryString;
            for (int i = 0; i < collection.Count; i++)
            {
                queryString += $"{collection.GetKey(i)}={collection.Get(i)}&";
            }
            return queryString.TrimEnd('&');
        }

        private IHeaderDictionary GetHttpHeaders(NameValueCollection headers)
        {
            HeaderDictionary dictionary = new HeaderDictionary();
            foreach (string name in headers.Keys)
            {
                dictionary[name] = new StringValues(headers.GetValues(name));
            }
            return dictionary;
        }

        private string GetProtocol()
        {
            HttpListenerRequest request = httpListenerContext.Request;
            Version version = request.ProtocolVersion;
            return string.Format("{0}/{1}.{2}", request.IsWebSocketRequest ? "HTTPS" : "HTTP", version.Major, version.Minor);
        }
    }
}
