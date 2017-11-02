using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace DarksideCookie.Owin.GithubWebhooks.Middleware.Entities
{
    public class GithubCommit
    {
        public GithubCommit(JToken data)
        {
            Id = data["id"].Value<string>();
            Message = data["message"].Value<string>();
            TimeStamp = data["timestamp"].Value<DateTime>();
            Added = ((JArray)data["added"]).Select(x => x.Value<string>()).ToArray();
            Removed = ((JArray)data["removed"]).Select(x => x.Value<string>()).ToArray();
            Modified = ((JArray)data["modified"]).Select(x => x.Value<string>()).ToArray();
            Author = new GithubUser(data["author"]);
            Committer = new GithubUser(data["committer"]);
        }

        public string Id { get; private set; }
        public string Message { get; private set; }
        public DateTime TimeStamp { get; private set; }
        public string[] Added { get; private set; }
        public string[] Removed { get; private set; }
        public string[] Modified { get; private set; }
        public GithubUser Author { get; private set; }
        public GithubUser Committer { get; private set; }
    }
}
