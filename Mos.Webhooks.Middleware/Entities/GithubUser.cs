using Newtonsoft.Json.Linq;

namespace DarksideCookie.Owin.GithubWebhooks.Middleware.Entities
{
    public class GithubUser
    {
        public GithubUser(JToken data)
        {
            Name = data["name"].Value<string>();
            Email = data["email"].Value<string>();
            if (data["username"] != null)
                Username = data["username"].Value<string>();
        }

        public string Name { get; private set; }
        public string Email { get; private set; }
        public string Username { get; private set; }
    }
}
