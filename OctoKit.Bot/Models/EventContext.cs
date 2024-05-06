using Microsoft.AspNetCore.Http;
using Octokit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Octokit.Bot
{
    public class EventContext
    {
        internal EventContext(HttpContext httpContext, WebHookEvent webHookEvent,
            InstallationContext installationContext, GitHubClient appClient)
        {
            HttpContext = httpContext;
            WebHookEvent = webHookEvent;
            InstallationContext = installationContext;
            AppClient = appClient;
        }

        public HttpContext HttpContext { get; internal set; }

        public WebHookEvent WebHookEvent { get; internal set; }

        public InstallationContext InstallationContext { get; internal set; }

        public GitHubClient AppClient { get; internal set; }
    }
}