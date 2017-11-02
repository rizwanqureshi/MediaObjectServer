using Newtonsoft.Json.Linq;

namespace DarksideCookie.Owin.GithubWebhooks.Middleware.Entities
{
    public class GithubIdentity
    {
        public GithubIdentity(JToken data)
        {
            Id = data["id"].Value<string>();
            Login = data["login"].Value<string>();
        }

        public string Id { get; private set; }
        public string Login { get; private set; }
    }
}
