using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Octokit.Bot
{
    public class WebHookHandlerRegistry
    {
        public delegate Task HandleWebHook(EventContext eventContext);

        private readonly IServiceProvider _serviceProvider;
        private readonly GitHubOption _gitHubOption;
        private readonly HttpContext _httpContext;

        private readonly Dictionary<string, List<HandleWebHook>> _registry =
            new Dictionary<string, List<HandleWebHook>>();

        public WebHookHandlerRegistry(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _gitHubOption = serviceProvider.GetService<IOptions<GitHubOption>>().Value;
            _httpContext = serviceProvider.GetService<IHttpContextAccessor>().HttpContext;
        }

        public WebHookHandlerRegistry RegisterHandler(string eventName, HandleWebHook webHookHandler)
        {
            if (!_registry.ContainsKey(eventName))
            {
                _registry[eventName] = new List<HandleWebHook>();
            }

            _registry[eventName].Add(webHookHandler);

            return this;
        }

        public WebHookHandlerRegistry RegisterHandler<THookHandler>(string eventName) where THookHandler : IHookHandler
        {
            var handler = _serviceProvider.GetService(typeof(THookHandler)) as IHookHandler;

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (!_registry.ContainsKey(eventName))
            {
                _registry[eventName] = new List<HandleWebHook>();
            }

            _registry[eventName].Add(handler.Handle);

            return this;
        }

        public async Task Handle(WebHookEvent webhookEvent)
        {
            var installationId = webhookEvent.GetInstallationId();

            var appClient = GitHubClientFactory.CreateGitHubAppClient(_gitHubOption);

            var installationContext = !installationId.HasValue
                ? null
                : await GitHubClientFactory.CreateGitHubInstallationClient(appClient, installationId.Value,
                    _gitHubOption.AppName);

            if (_registry.ContainsKey(webhookEvent.GitHubEvent))
            {
                var context = new EventContext(_httpContext, webhookEvent, installationContext, appClient);

                var handlers = _registry[webhookEvent.GitHubEvent];

                foreach (var handler in handlers)
                {
                    await handler(context);
                }
            }
        }
    }
}