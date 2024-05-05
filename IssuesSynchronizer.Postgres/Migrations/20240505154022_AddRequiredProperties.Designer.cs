﻿// <auto-generated />
using System;
using IssuesSynchronizer.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IssuesSynchronizer.Postgres.Migrations
{
    [DbContext(typeof(IssuesSynchronizerDbContext))]
    [Migration("20240505154022_AddRequiredProperties")]
    partial class AddRequiredProperties
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("IssuesSynchronizer.Postgres.Entities.IssueCommentEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<decimal>("DiscordMessageId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<long?>("DiscordUserId")
                        .HasColumnType("bigint");

                    b.Property<int>("GitHubCommentId")
                        .HasColumnType("integer");

                    b.Property<int?>("GitHubUserId")
                        .HasColumnType("integer");

                    b.Property<Guid>("IssueId")
                        .HasColumnType("uuid");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("IssueId");

                    b.ToTable("IssueCommentEntities");
                });

            modelBuilder.Entity("IssuesSynchronizer.Postgres.Entities.IssueThreadEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<decimal>("BodyMessageInThreadId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("ForumThreadId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<long>("InfoMessageInIssueId")
                        .HasColumnType("bigint");

                    b.Property<decimal>("InfoMessageInThreadId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int>("IssueNumber")
                        .HasColumnType("integer");

                    b.Property<string>("IssueUrl")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("RepositoryId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("RepositoryId");

                    b.ToTable("IssueThreadEntities");
                });

            modelBuilder.Entity("IssuesSynchronizer.Postgres.Entities.RepositoryChannelLinkEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<long>("RepositoryId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("RepositoryChannelLinkEntities");
                });

            modelBuilder.Entity("IssuesSynchronizer.Postgres.Entities.IssueCommentEntity", b =>
                {
                    b.HasOne("IssuesSynchronizer.Postgres.Entities.IssueThreadEntity", "Issue")
                        .WithMany()
                        .HasForeignKey("IssueId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Issue");
                });

            modelBuilder.Entity("IssuesSynchronizer.Postgres.Entities.IssueThreadEntity", b =>
                {
                    b.HasOne("IssuesSynchronizer.Postgres.Entities.RepositoryChannelLinkEntity", "Repository")
                        .WithMany("IssueThreadEntities")
                        .HasForeignKey("RepositoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Repository");
                });

            modelBuilder.Entity("IssuesSynchronizer.Postgres.Entities.RepositoryChannelLinkEntity", b =>
                {
                    b.Navigation("IssueThreadEntities");
                });
#pragma warning restore 612, 618
        }
    }
}