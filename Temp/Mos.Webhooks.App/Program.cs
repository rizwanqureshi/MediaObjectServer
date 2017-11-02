using System;
using System.Linq;
using DarksideCookie.Owin.GithubWebhooks.Middleware;
using Microsoft.Owin.Hosting;
using Owin;

namespace DarksideCookie.Owin.GithubWebhooks.App
{
    class Program
    {
        static void Main(string[] args)
        {
            using (WebApp.Start<Startup>("http://127.0.0.1:4567"))
            {
                Console.ReadLine();
            }
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseWebhooks("/webhook", new WebhookMiddlewareOptions
            {
                Secret = "12345",
                OnEvent = (obj) =>
                {
                    Console.WriteLine("Incoming hook call: {0}\r\nCommits:\r\n{1}", obj.Type, string.Join("\r\n", obj.Commits.Select(x => x.Id)));
                }
            });

            app.UseWelcomePage("/");
        }
    }
}
