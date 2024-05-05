namespace IssuesSynchronizer.Postgres.Entities;

public class IssueThreadEntity
{
    public Guid Id { get; set; }
    public RepositoryChannelLinkEntity Repository { get; set; } = null!;
    public Guid RepositoryId { get; set; }
    public int IssueNumber { get; set; }
    public ulong ForumThreadId { get; set; }
    public ulong InfoMessageInThreadId { get; set; }
    public ulong BodyMessageInThreadId { get; set; }
    public long InfoMessageInIssueId { get; set; }
    public string IssueUrl { get; set; } = null!;
}