using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IssuesSynchronizer.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RepositoryChannelLinkEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RepositoryId = table.Column<long>(type: "bigint", nullable: false),
                    GuildId = table.Column<long>(type: "bigint", nullable: false),
                    ChannelId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepositoryChannelLinkEntities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IssueThreadEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RepositoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    IssueId = table.Column<int>(type: "integer", nullable: false),
                    IssueNumber = table.Column<int>(type: "integer", nullable: false),
                    ForumThreadId = table.Column<long>(type: "bigint", nullable: false),
                    InfoMessageInThreadId = table.Column<long>(type: "bigint", nullable: false),
                    InfoMessageInIssueId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueThreadEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssueThreadEntities_RepositoryChannelLinkEntities_Repositor~",
                        column: x => x.RepositoryId,
                        principalTable: "RepositoryChannelLinkEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IssueCommentEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IssueId = table.Column<Guid>(type: "uuid", nullable: false),
                    GitHubUserId = table.Column<int>(type: "integer", nullable: true),
                    DiscordUserId = table.Column<long>(type: "bigint", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueCommentEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssueCommentEntities_IssueThreadEntities_IssueId",
                        column: x => x.IssueId,
                        principalTable: "IssueThreadEntities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IssueCommentEntities_IssueId",
                table: "IssueCommentEntities",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueThreadEntities_RepositoryId",
                table: "IssueThreadEntities",
                column: "RepositoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IssueCommentEntities");

            migrationBuilder.DropTable(
                name: "IssueThreadEntities");

            migrationBuilder.DropTable(
                name: "RepositoryChannelLinkEntities");
        }
    }
}
