using Discord;
using Discord.Commands;
using Discord.Interactions;
using IssuesSynchronizer.GitHub;
using IssuesSynchronizer.GitHub.Infrastructure;
using IssuesSynchronizer.Postgres;
using IssuesSynchronizer.Postgres.Entities;
using Microsoft.EntityFrameworkCore;
using Octokit;

namespace IssuesSynchronizer.Discord.CommandModules;

public class LinkRepositoryCommandModule(GitHubClientProvider _gitHubClientProvider, IDbContextFactory<IssuesSynchronizerDbContext> _dbContextFactory) : InteractionModuleBase
{
    [SlashCommand("link-repository", "Links a GitHub repository with forum channel")]
    public async Task LinkRepositoryCommand(IForumChannel forumChannel, string repoUrl)
    {
        if (!GitHubUtils.TryParseRepoUrl(repoUrl, out var owner, out var repo))
        {
            await RespondAsync("Can't parse repo url");
            return;
        }
        var repoInstallation = await _gitHubClientProvider.App.GitHubApps.GetRepositoryInstallationForCurrent(owner, repo);
        if (repoInstallation is null)
        {
            await RespondAsync("Install an app for your repo using https://github.com/apps/issues-synchronizer/installations/new");
            return;
        }

        await DeferAsync();

        var installationClient = await _gitHubClientProvider.CreateInstallationClient(repoInstallation.Id);
        var repository = await installationClient.Repository.Get(owner, repo);

        await FollowupAsync("Successfully linked");

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        dbContext.RepositoryChannelLinkEntities.Add(new RepositoryChannelLinkEntity()
            { ChannelId = forumChannel.Id, GuildId = forumChannel.GuildId, RepositoryId = repository.Id });

        await dbContext.SaveChangesAsync();
    }
}