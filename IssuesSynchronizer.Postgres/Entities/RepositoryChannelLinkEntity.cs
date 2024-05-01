namespace IssuesSynchronizer.Postgres.Entities;

public class RepositoryChannelLinkEntity
{
    public Guid Id { get; set; }
    public long RepositoryId { get; set; }
    public long GuildId { get; set; }
    public long ChannelId { get; set; }

    public HashSet<IssueThreadEntity> IssueThreadEntities { get; set; } = null!;
}