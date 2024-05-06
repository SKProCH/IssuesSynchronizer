using Microsoft.Extensions.DependencyInjection;
using System;

namespace Octokit.Bot
{
    public static class Extensions
    {
        public static void AddGitHubWebHookHandler(this IServiceCollection services,
            Action<WebHookHandlerRegistry> register)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (register == null)
            {
                throw new ArgumentNullException(nameof(register));
            }

            services.AddScoped(serviceProvider =>
            {
                var registry = new WebHookHandlerRegistry(serviceProvider);

                register(registry);

                return registry;
            });

            services.AddHttpContextAccessor();
        }
    }
}