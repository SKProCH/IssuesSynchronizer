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
        var scopedWebHookHandlerRegistry = new ScopedWebHookHandlerRegistry(services);
        register(scopedWebHookHandlerRegistry);
        
        services.AddScoped(serviceProvider =>
        {
            var hookHandlerRegistry = new WebHookHandlerRegistry(serviceProvider);
            scopedWebHookHandlerRegistry.ApplyToWebHookHandlerRegistry(hookHandlerRegistry);
            return hookHandlerRegistry;
        });
        services.AddHttpContextAccessor();
    }
}