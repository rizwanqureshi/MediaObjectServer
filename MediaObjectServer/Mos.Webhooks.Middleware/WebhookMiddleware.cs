using System.IO;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace DarksideCookie.Owin.GithubWebhooks.Middleware
{
    public class WebhookMiddleware : OwinMiddleware
    {
        private readonly WebhookMiddlewareOptions _options;

        public WebhookMiddleware(OwinMiddleware next, WebhookMiddlewareOptions options)
            : base(next)
        {
            _options = options;
        }

        public override async Task Invoke(IOwinContext context)
        {
            var eventType = context.Request.Headers["X-Github-Event"];
            var signature = context.Request.Headers["X-Hub-Signature"];
            var delivery = context.Request.Headers["X-Github-Delivery"];

            string body;
            using (var sr = new StreamReader(context.Request.Body))
            {
                body = await sr.ReadToEndAsync();
            }

            if (!_options.OnValidateSignature(body, signature))
            {
                context.Response.ReasonPhrase = "Could not verify signature";
                context.Response.StatusCode = 400;
                return;
            }

            _options.OnEvent(WebhookEvent.Create(eventType, delivery, body));

            context.Response.StatusCode = 200;
        }
    }
}
