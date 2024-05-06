using IssuesSynchronizer.GitHub.Senders;
using Octokit.Bot;

namespace IssuesSynchronizer.GitHub.Handlers;

public class GitHubIssueCommentHandler(GitHubToDiscordSenderService _gitHubToDiscordSenderService)
    : IGitHubEventName, IHookHandler
{
    public static string EventName => "issue_comment";

    public async Task Handle(EventContext eventContext)
    {
        if (!eventContext.WebHookEvent.IsMessageAuthenticated)
        {
            return;
        }

        var payload = eventContext.WebHookEvent.JsonPayload;

        var repositoryId = payload["repository"]!["id"]!.GetValue<long>();
        var issueId = payload["issue"]!["number"]!.GetValue<int>();
        var commentId = payload["comment"]!["id"]!.GetValue<int>();
        var senderType = payload["sender"]!["type"]!.GetValue<string>();
        var (hasLinkedForum, hasLinkedThread) = await _gitHubToDiscordSenderService.HasDiscordLinkedChannelAndThread(repositoryId, issueId);
        if (!hasLinkedForum || !hasLinkedThread || senderType != "User")
        {
            return;
        }

        _gitHubToDiscordSenderService.EnqueueUpdate(repositoryId, issueId, commentId);
    }
}