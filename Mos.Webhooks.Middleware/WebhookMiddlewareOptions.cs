using System;
using System.Linq;
using System.Security.Cryptography;

namespace DarksideCookie.Owin.GithubWebhooks.Middleware
{
    public class WebhookMiddlewareOptions
    {
        public WebhookMiddlewareOptions()
        {
            OnEvent = (obj) => { };
            OnValidateSignature = ValidateSignature;
        }

        private bool ValidateSignature(string body, string signature)
        {
            if (string.IsNullOrWhiteSpace(Secret))
                return false;

            var vals = signature.Split('=');
            if (vals[0] != "sha1")
            {
                return false;
            }

            var encoding = new System.Text.ASCIIEncoding();
            var keyByte = encoding.GetBytes(Secret);

            var hmacsha1 = new HMACSHA1(keyByte);

            var messageBytes = encoding.GetBytes(body);
            var hashmessage = hmacsha1.ComputeHash(messageBytes);
            var hash = hashmessage.Aggregate("", (current, t) => current + t.ToString("X2"));

            return hash.Equals(vals[1], StringComparison.OrdinalIgnoreCase);
        }

        public string Secret { get; set; }
        public Action<WebhookEvent> OnEvent { get; set; }
        public Func<string, string, bool> OnValidateSignature { get; set; }
    }
}