using System;
using System.Collections.Generic;
using System.Text;

namespace Octokit.Bot
{
    public class InstallationContext
    {
        public InstallationContext(GitHubClient client, AccessToken accessToken)
        {
            AccessToken = accessToken;
            Client = client;
        }

        public AccessToken AccessToken { get; set; }

        public GitHubClient Client { get; set; }
    }
}