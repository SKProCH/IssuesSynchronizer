namespace IssuesSynchronizer.GitHub.Handlers;

public interface IGitHubEventName
{
    static abstract string EventName { get; }
}