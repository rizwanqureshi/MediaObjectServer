using System.Linq;
using DarksideCookie.Owin.GithubWebhooks.Middleware.Entities;
using Newtonsoft.Json.Linq;

namespace DarksideCookie.Owin.GithubWebhooks.Middleware
{
    public class WebhookEvent
    {
        protected WebhookEvent(string type, string deliveryId, string body)
        {
            Type = type;
            DeliveryId = deliveryId;

            var json = JObject.Parse(body);
            Ref = json["ref"].Value<string>();
            Before = json["before"].Value<string>();
            After = json["after"].Value<string>();
            HeadCommit = new GithubCommit(json["head_commit"]);
            Commits = json["commits"].Values<JToken>().Select(x => new GithubCommit(x)).ToArray();
            Pusher = new GithubUser(json["pusher"]);
            Sender = new GithubIdentity(json["sender"]);
        }

        public static WebhookEvent Create(string type, string deliveryId, string body)
        {
            return new WebhookEvent(type, deliveryId, body);
        }

        public string Type { get; private set; }
        public string DeliveryId { get; private set; }
        public string Ref { get; private set; }
        public string Before { get; private set; }
        public string After { get; private set; }
        public GithubCommit HeadCommit { get; set; }
        public GithubCommit[] Commits { get; set; }
        public GithubUser Pusher { get; private set; }
        public GithubIdentity Sender { get; private set; }
    }
}