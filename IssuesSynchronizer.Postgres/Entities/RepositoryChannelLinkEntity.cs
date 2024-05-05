namespace IssuesSynchronizer.Postgres.Entities;

public class RepositoryChannelLinkEntity
{
    public Guid Id { get; set; }
    public long RepositoryId { get; set; }
    public ulong GuildId { get; set; }
    public ulong ChannelId { get; set; }

    public HashSet<IssueThreadEntity> IssueThreadEntities { get; set; } = null!;
}