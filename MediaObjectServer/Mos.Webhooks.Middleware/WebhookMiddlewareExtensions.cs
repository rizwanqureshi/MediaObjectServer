using Owin;

namespace DarksideCookie.Owin.GithubWebhooks.Middleware
{
    public static class WebhookMiddlewareExtensions
    {
        public static void UseWebhooks(this IAppBuilder app, string path, WebhookMiddlewareOptions options = null)
        {
            if (!path.StartsWith("/"))
            {
                path = "/" + path;
            }

            app.Map(path, (app2) =>
            {
                app2.Use<WebhookMiddleware>(options ?? new WebhookMiddlewareOptions());
            });
        }
    }
}
