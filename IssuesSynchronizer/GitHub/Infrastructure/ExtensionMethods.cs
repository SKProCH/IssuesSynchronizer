using Octokit.Bot;

namespace IssuesSynchronizer.GitHub.Infrastructure;

public static class ExtensionMethods
{
    public static void AddScopedGitHubWebHookHandlers(
        this IServiceCollection services,
        Action<ScopedWebHookHandlerRegistry> register)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(register);
        
        services.AddScoped(serviceProvider =>
        {
            var hookHandlerRegistry = new WebHookHandlerRegistry(serviceProvider);
            var scopedWebHookHandlerRegistry = new ScopedWebHookHandlerRegistry(services, hookHandlerRegistry);
            register(scopedWebHookHandlerRegistry);
            return hookHandlerRegistry;
        });
        services.AddHttpContextAccessor();
    }
}