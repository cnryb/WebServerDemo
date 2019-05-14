using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;

namespace WebServerDemo
{
    public class HttpListenerServer : IServer
    {
        private readonly HttpListener listener;

        public IFeatureCollection Features { get; } = new FeatureCollection();

        public HttpListenerServer()
        {
            listener = new HttpListener();
            this.Features.Set<IServerAddressesFeature>(new ServerAddressesFeature());
        }

        public void Dispose()
        {
            listener.Stop();
        }

        public void Start<TContext>(IHttpApplication<TContext> application)
        {
            foreach (string address in this.Features.Get<IServerAddressesFeature>().Addresses)
            {
                listener.Prefixes.Add(address.TrimEnd('/') + "/");
            }

            listener.Start();
            while (true)
            {
                HttpListenerContext httpListenerContext = listener.GetContext();

                string listenUrl = this.Features.Get<IServerAddressesFeature>().Addresses.First(address => httpListenerContext.Request.Url.IsBaseOf(new Uri(address)));
                string pathBase = new Uri(listenUrl).LocalPath.TrimEnd('/');
                HttpListenerServerFeature feature = new HttpListenerServerFeature(httpListenerContext, pathBase);

                FeatureCollection features = new FeatureCollection();
                features.Set<IHttpRequestFeature>(feature);
                features.Set<IHttpResponseFeature>(feature);
                TContext context = application.CreateContext(features);

                application.ProcessRequestAsync(context).ContinueWith(task =>
                {
                    httpListenerContext.Response.Close();
                    application.DisposeContext(context, task.Exception);
                });
            }
        }

        public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
        {
            foreach (string address in this.Features.Get<IServerAddressesFeature>().Addresses)
            {
                listener.Prefixes.Add(address.TrimEnd('/') + "/");
            }

            listener.Start();
            while (true)
            {
                HttpListenerContext httpListenerContext =   listener.GetContext();

                string listenUrl = this.Features.Get<IServerAddressesFeature>().Addresses.First(address => httpListenerContext.Request.Url.IsBaseOf(new Uri(address)));
                string pathBase = new Uri(listenUrl).LocalPath.TrimEnd('/');
                HttpListenerServerFeature feature = new HttpListenerServerFeature(httpListenerContext, pathBase);

                FeatureCollection features = new FeatureCollection();
                features.Set<IHttpRequestFeature>(feature);
                features.Set<IHttpResponseFeature>(feature);
                TContext context = application.CreateContext(features);

                application.ProcessRequestAsync(context).ContinueWith(task =>
                {
                    httpListenerContext.Response.Close();
                    application.DisposeContext(context, task.Exception);
                });
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
           return Task.Run(() => listener.Stop());
        }
    }
}