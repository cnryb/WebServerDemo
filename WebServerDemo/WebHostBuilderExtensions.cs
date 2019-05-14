using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace WebServerDemo
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder UseHttpListener(this IWebHostBuilder builder)
        {
            builder.ConfigureServices(services => services.AddSingleton<IServer, HttpListenerServer>());
            return builder;
        }
    }
}
