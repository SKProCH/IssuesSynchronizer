using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using IssuesSynchronizer.Discord;
using IssuesSynchronizer.GitHub.Infrastructure;
using IssuesSynchronizer.Postgres;
using IssuesSynchronizer.Postgres.Entities;
using Microsoft.EntityFrameworkCore;
using Octokit;

namespace IssuesSynchronizer.GitHub.Senders;

public class GitHubIssueToDiscordSenderService
{
    private readonly Subject<int?> _bufferSubject = new();
    private static readonly TimeSpan ThrottleDelay = new(0, 0, 30);

    private readonly GitHubClientProvider _gitHubClientProvider;
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly IDbContextFactory<IssuesSynchronizerDbContext> _dbContextFactory;
    private readonly long _repositoryId;
    private readonly int _issueNumber;
    private readonly ILogger<GitHubIssueToDiscordSenderService> _logger;

    public GitHubIssueToDiscordSenderService(GitHubClientProvider gitHubClientProvider,
        DiscordSocketClient discordSocketClient, IDbContextFactory<IssuesSynchronizerDbContext> dbContextFactory,
        long repositoryId, int issueNumber,
        ILogger<GitHubIssueToDiscordSenderService> logger)
    {
        _gitHubClientProvider = gitHubClientProvider;
        _discordSocketClient = discordSocketClient;
        _dbContextFactory = dbContextFactory;
        _repositoryId = repositoryId;
        _issueNumber = issueNumber;
        _logger = logger;

        _bufferSubject
            .Buffer(ThrottleDelay)
            .Where(list => list.Count != 0)
            .Subscribe(OnNext);
    }

    private void OnNext(IList<int?> commentsList)
    {
        commentsList = commentsList
            .Distinct()
            .ToImmutableArray();

        _ = Task.Run(async () =>
        {
            _logger.LogInformation(
                "Starting processing issue {IssueId} on repository {RepoId} with {CommentsCount} updates",
                _issueNumber, _repositoryId, commentsList.Count);
            try
            {
                await ProcessChanges(commentsList);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Starting processing issue {IssueId} on repository {RepoId}",
                    _issueNumber, _repositoryId);
            }
        });
    }

    private async Task ProcessChanges(IList<int?> commentsList)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var repositoryChannelLinkEntity = await dbContext.RepositoryChannelLinkEntities
            .Include(entity =>
                entity.IssueThreadEntities.Where(threadEntity => threadEntity.IssueNumber == _issueNumber))
            .Where(entity => entity.RepositoryId == _repositoryId)
            .FirstAsync();
        
        var gitHubClient = await _gitHubClientProvider.CreateInstallationClientForRepo(_repositoryId);
        if (gitHubClient is null)
        {
            dbContext.RepositoryChannelLinkEntities.Remove(repositoryChannelLinkEntity);
            await dbContext.SaveChangesAsync();
            return;
        }

        var guild = await _discordSocketClient.Rest.GetGuildAsync(repositoryChannelLinkEntity.GuildId);
        var forumChannel = await guild.GetForumChannelAsync(repositoryChannelLinkEntity.ChannelId);

        if (commentsList.Contains(null))
        {
            await ProcessIssueUpdate(gitHubClient, repositoryChannelLinkEntity, guild, forumChannel);
        }

        var commentsToUpdate = commentsList.Where(i => i is not null)
            .Cast<int>()
            .ToImmutableArray();
        if (commentsToUpdate.Length > 0)
        {
            await ProcessCommentsUpdate(gitHubClient, commentsToUpdate, dbContext, repositoryChannelLinkEntity, guild);
        }

        await dbContext.SaveChangesAsync();
    }

    private async Task ProcessIssueUpdate(GitHubClient gitHubClient,
        RepositoryChannelLinkEntity repositoryChannelLinkEntity, IGuild guild, RestForumChannel forumChannel)
    {
        var repository = await gitHubClient.Repository.Get(_repositoryId);
        var issue = await gitHubClient.Issue.Get(_repositoryId, _issueNumber);

        // Forum thread does not exist, creating
        if (repositoryChannelLinkEntity.IssueThreadEntities.Count == 0)
        {
            var threadChannel = await forumChannel.CreatePostAsync(issue.Title, ThreadArchiveDuration.OneWeek,
                text: issue.Body, allowedMentions: AllowedMentions.None, options: DiscordUtils.DefaultRequestOptions);

            await threadChannel.SendMessageAsync(
                $"Linked to [{repository.FullName}#{issue.Number}]({issue.Url})");

            var firstMessages = await threadChannel.GetMessagesAsync(2, options: DiscordUtils.DefaultRequestOptions)
                .Pipe(enumerable => enumerable.FirstAsync())
                .PipeAsync(messages => messages.OfType<IUserMessage>().Take(2))
                .PipeAsync(messages => messages.ToImmutableArray());

            foreach (var message in firstMessages)
            {
                await message.PinAsync(options: DiscordUtils.DefaultRequestOptions);
            }

            var threadLink = $"https://discord.com/channels/{forumChannel.Id}/{threadChannel.Id}";
            var githubInfoMessage = await gitHubClient.Issue.Comment.Create(_repositoryId, _issueNumber,
                $"Linked to Discord #{forumChannel.Name}: [navigate]({threadLink})");

            repositoryChannelLinkEntity.IssueThreadEntities.Add(new IssueThreadEntity
            {
                IssueNumber = issue.Number, ForumThreadId = threadChannel.Id,
                BodyMessageInThreadId = firstMessages.First().Id,
                InfoMessageInThreadId = firstMessages.Last().Id,
                InfoMessageInIssueId = githubInfoMessage.Id,
                IssueUrl = issue.Url
            });
        }
        else
        {
            var issueThreadEntity = repositoryChannelLinkEntity.IssueThreadEntities.First();
            var threadChannel = DiscordUtils.CreateFakeThreadChannel(_discordSocketClient.Rest, guild,
                issueThreadEntity.ForumThreadId, null);
            await threadChannel.ModifyAsync(properties =>
            {
                properties.Name = issue.Title;
                properties.Locked = issue.Locked;
            });
        }
    }

    private async Task ProcessCommentsUpdate(GitHubClient gitHubClient, IList<int> commentsList,
        IssuesSynchronizerDbContext dbContext, RepositoryChannelLinkEntity repositoryChannelLinkEntity,
        IGuild guild)
    {
        var issueThreadEntity = repositoryChannelLinkEntity.IssueThreadEntities.First();
        var threadChannel = DiscordUtils.CreateFakeThreadChannel(_discordSocketClient.Rest, guild,
            issueThreadEntity.ForumThreadId, null);

        var issueComments = await gitHubClient.Issue.Comment.GetAllForIssue(_repositoryId, _issueNumber)
            .PipeAsync(list => list.ToDictionary(comment => comment.Id));

        var existingComments = await dbContext.IssueCommentEntities
            .Where(entity => entity.Issue.IssueNumber == _issueNumber &&
                             entity.Issue.Repository.RepositoryId == _repositoryId)
            .ToDictionaryAsync(entity => entity.GitHubCommentId);

        // Creating
        var createdComments = commentsList
            .Where(updateId => issueComments.ContainsKey(updateId))
            .Where(updateId => !existingComments.ContainsKey(updateId))
            .Select(updateId => issueComments[updateId])
            .OrderBy(comment => comment.CreatedAt);

        foreach (var createdComment in createdComments)
        {
            var discordMessage = await threadChannel.SendMessageAsync(FormHeader(createdComment) + createdComment.Body,
                options: DiscordUtils.DefaultRequestOptions);
            await dbContext.IssueCommentEntities.AddAsync(new IssueCommentEntity
            {
                Issue = issueThreadEntity,
                DiscordMessageId = discordMessage.Id,
                GitHubCommentId = createdComment.Id,
                Message = createdComment.Body,
                GitHubUserId = createdComment.User.Id
            });
        }

        // Updating

        var updatedComments = commentsList
            .Where(updateId => issueComments.ContainsKey(updateId))
            .Where(updateId => existingComments.ContainsKey(updateId))
            .Select(updateId => (issueComments[updateId], existingComments[updateId]));

        foreach (var (issueComment, commentEntity) in updatedComments)
        {
            await threadChannel.ModifyMessageAsync(commentEntity.DiscordMessageId,
                properties => properties.Content = FormHeader(issueComment) + issueComment.Body,
                options: DiscordUtils.DefaultRequestOptions);
            commentEntity.Message = issueComment.Body;
        }

        // Deleting
        var deletedComments = commentsList
            .Where(updateId => !issueComments.ContainsKey(updateId))
            .ToImmutableArray();
        var discordMessagesToDelete = existingComments
            .Values
            .Where(entity => deletedComments.Contains(entity.GitHubCommentId))
            .ToImmutableArray();
        var discordMessageIdsToDelete = discordMessagesToDelete
            .Select(entity => entity.DiscordMessageId);

        await threadChannel.DeleteMessagesAsync(discordMessageIdsToDelete);

        dbContext.IssueCommentEntities.RemoveRange(discordMessagesToDelete);

        string FormHeader(IssueComment createdComment)
        {
            return $"@{createdComment.User.Login} commented " +
                   $"<t:{createdComment.CreatedAt.ToUnixTimeSeconds()}:R> " +
                   $"on [#{issueThreadEntity.IssueNumber}]({issueThreadEntity.IssueUrl})\n";
        }
    }

    public void EnqueueUpdate(int? commentId)
    {
        _logger.LogInformation("Enqueueing github -> discord update with comment id {CommentId}", commentId);
        _bufferSubject.OnNext(commentId);
    }
}