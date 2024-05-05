using IssuesSynchronizer.Postgres.Entities;
using Microsoft.EntityFrameworkCore;

namespace IssuesSynchronizer.Postgres;

public class IssuesSynchronizerDbContext(DbContextOptions<IssuesSynchronizerDbContext> options) : DbContext(options)
{
    public DbSet<RepositoryChannelLinkEntity> RepositoryChannelLinkEntities { get; init; }
    public DbSet<IssueThreadEntity> IssueThreadEntities { get; init; }
    public DbSet<IssueCommentEntity> IssueCommentEntities { get; init; }
}