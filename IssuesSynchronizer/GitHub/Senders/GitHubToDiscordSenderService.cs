using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Discord.WebSocket;
using IssuesSynchronizer.Postgres;
using Microsoft.EntityFrameworkCore;
using Octokit;
using Octokit.Bot;

namespace IssuesSynchronizer.GitHub.Senders;

public class GitHubToDiscordSenderService(
    GitHubClient _gitHubClient,
    ILoggerFactory _loggerFactory,
    DiscordSocketClient _DiscordSocketClient,
    IDbContextFactory<IssuesSynchronizerDbContext> _dbContextFactory)
{
    private ConcurrentDictionary<(long, int), GitHubIssueToDiscordSenderService> _servicesCache = new();

    public void EnqueueUpdate(long repositoryId, int issueNumber, int? commentId)
    {
        var gitHubIssueToDiscordSenderService = _servicesCache.GetOrAdd((repositoryId, issueNumber), _ =>
        {
            var logger = _loggerFactory.CreateLogger<GitHubIssueToDiscordSenderService>();
            return new GitHubIssueToDiscordSenderService(_gitHubClient, _DiscordSocketClient, _dbContextFactory,
                repositoryId, issueNumber, logger);
        });

        gitHubIssueToDiscordSenderService.EnqueueUpdate(commentId);
    }

    public async Task<(bool, bool)> HasDiscordLinkedChannelAndThread(long repositoryId, int issueId)
    {
        if (_servicesCache.ContainsKey((repositoryId, issueId)))
        {
            return (true, true);
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var repositoryChannelLinkEntity = await dbContext.RepositoryChannelLinkEntities
            .Include(entity => entity.IssueThreadEntities.Where(threadEntity => threadEntity.IssueNumber == issueId))
            .Where(entity => entity.RepositoryId == repositoryId)
            .FirstOrDefaultAsync();

        if (repositoryChannelLinkEntity is null)
        {
            return (false, false);
        }

        return (true, repositoryChannelLinkEntity.IssueThreadEntities.Count > 0);
    }
}