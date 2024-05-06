using IssuesSynchronizer.GitHub.Handlers;
using Octokit.Bot;

namespace IssuesSynchronizer.GitHub.Infrastructure;

public class ScopedWebHookHandlerRegistry
{
    private readonly IServiceCollection _serviceCollection;
    private readonly List<Action<WebHookHandlerRegistry>> _deferredActions = new();

    public ScopedWebHookHandlerRegistry(IServiceCollection serviceCollection)
    {
        _serviceCollection = serviceCollection;
    }
    public ScopedWebHookHandlerRegistry RegisterHandler(string eventName, WebHookHandlerRegistry.HandleWebHook webHookHandler)
    {
        _deferredActions.Add(registry => registry.RegisterHandler(eventName, webHookHandler));
        return this;
    }

    public ScopedWebHookHandlerRegistry RegisterHandler<THookHandler>(string eventName) where THookHandler : class, IHookHandler
    {
        _serviceCollection.AddScoped<THookHandler>();
        _deferredActions.Add(registry => registry.RegisterHandler<THookHandler>(eventName));
        return this;
    }
    
    public ScopedWebHookHandlerRegistry RegisterHandler<THookHandler>() where THookHandler : class, IHookHandler, IGitHubEventName
    {
        _serviceCollection.AddScoped<THookHandler>();
        _deferredActions.Add(registry => registry.RegisterHandler<THookHandler>(THookHandler.EventName));
        return this;
    }

    public void ApplyToWebHookHandlerRegistry(WebHookHandlerRegistry registry)
    {
        _deferredActions.ForEach(action => action(registry));
    }
}