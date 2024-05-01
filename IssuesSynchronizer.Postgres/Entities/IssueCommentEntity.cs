namespace IssuesSynchronizer.Postgres.Entities;

public class IssueCommentEntity
{
    public Guid Id { get; set; }
    
    public IssueThreadEntity Issue { get; set; }
    public Guid IssueId { get; set; }
    
    public int? GitHubUserId { get; set; }
    public long? DiscordUserId { get; set; }
    public string Message { get; set; }
}