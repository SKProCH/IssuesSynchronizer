namespace IssuesSynchronizer.Postgres.Entities;

public class IssueThreadEntity
{
    public Guid Id { get; set; }
    public RepositoryChannelLinkEntity Repository { get; set; } = null!;
    public Guid RepositoryId { get; set; }
    public int IssueId { get; set; }
    public int IssueNumber { get; set; }
    public long ForumThreadId { get; set; }
    public long InfoMessageInThreadId { get; set; }
    public long InfoMessageInIssueId { get; set; }
}