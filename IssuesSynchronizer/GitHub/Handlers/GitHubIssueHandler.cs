using IssuesSynchronizer.GitHub.Senders;
using Octokit.Bot;

namespace IssuesSynchronizer.GitHub.Handlers;

public class GitHubIssueHandler(GitHubToDiscordSenderService _gitHubToDiscordSenderService)
    : IGitHubEventName, IHookHandler
{
    private static readonly string[] IgnoredEvents = ["labeled", "unlabeled", "transferred", "deleted"];
    public static string EventName => "issues";

    public async Task Handle(EventContext eventContext)
    {
        if (!eventContext.WebHookEvent.IsMessageAuthenticated)
        {
            return;
        }

        var payload = eventContext.WebHookEvent.JsonPayload;
        var action = payload["action"]!.GetValue<string>();
        if (IgnoredEvents.Contains(action))
        {
            return;
        }

        var repositoryId = payload["repository"]!["id"]!.GetValue<long>();
        var issueId = payload["issue"]!["number"]!.GetValue<int>();
        var (hasLinkedForum, hasLinkedThread) = await _gitHubToDiscordSenderService.HasDiscordLinkedChannelAndThread(repositoryId, issueId);
        if (!hasLinkedForum || (action != "create" && !hasLinkedThread))
        {
            return;
        }

        _gitHubToDiscordSenderService.EnqueueUpdate(repositoryId, issueId, null);
    }
}