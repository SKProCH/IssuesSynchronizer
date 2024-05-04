using Octokit.Bot;

namespace IssuesSynchronizer.GitHub.Infrastructure;

public class ScopedWebHookHandlerRegistry
{
    private readonly IServiceCollection _serviceCollection;
    private readonly WebHookHandlerRegistry _hookHandlerRegistry;

    public ScopedWebHookHandlerRegistry(IServiceCollection serviceCollection, WebHookHandlerRegistry hookHandlerRegistry)
    {
        _serviceCollection = serviceCollection;
        _hookHandlerRegistry = hookHandlerRegistry;
    }
    public ScopedWebHookHandlerRegistry RegisterHandler(string eventName, WebHookHandlerRegistry.HandleWebHook webHookHandler)
    {
        _hookHandlerRegistry.RegisterHandler(eventName, webHookHandler);
        return this;
    }

    public ScopedWebHookHandlerRegistry RegisterHandler<THookHandler>(string eventName) where THookHandler : class, IHookHandler
    {
        _serviceCollection.AddScoped<THookHandler>();
        _hookHandlerRegistry.RegisterHandler<THookHandler>(eventName);
        return this;
    }
}