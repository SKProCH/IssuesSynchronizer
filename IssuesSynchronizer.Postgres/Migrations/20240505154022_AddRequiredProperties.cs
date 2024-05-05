using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IssuesSynchronizer.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddRequiredProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IssueId",
                table: "IssueThreadEntities");

            migrationBuilder.AlterColumn<decimal>(
                name: "GuildId",
                table: "RepositoryChannelLinkEntities",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<decimal>(
                name: "ChannelId",
                table: "RepositoryChannelLinkEntities",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<decimal>(
                name: "InfoMessageInThreadId",
                table: "IssueThreadEntities",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<decimal>(
                name: "ForumThreadId",
                table: "IssueThreadEntities",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<decimal>(
                name: "BodyMessageInThreadId",
                table: "IssueThreadEntities",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "IssueUrl",
                table: "IssueThreadEntities",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "DiscordMessageId",
                table: "IssueCommentEntities",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "GitHubCommentId",
                table: "IssueCommentEntities",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BodyMessageInThreadId",
                table: "IssueThreadEntities");

            migrationBuilder.DropColumn(
                name: "IssueUrl",
                table: "IssueThreadEntities");

            migrationBuilder.DropColumn(
                name: "DiscordMessageId",
                table: "IssueCommentEntities");

            migrationBuilder.DropColumn(
                name: "GitHubCommentId",
                table: "IssueCommentEntities");

            migrationBuilder.AlterColumn<long>(
                name: "GuildId",
                table: "RepositoryChannelLinkEntities",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "ChannelId",
                table: "RepositoryChannelLinkEntities",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "InfoMessageInThreadId",
                table: "IssueThreadEntities",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "ForumThreadId",
                table: "IssueThreadEntities",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.AddColumn<int>(
                name: "IssueId",
                table: "IssueThreadEntities",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
