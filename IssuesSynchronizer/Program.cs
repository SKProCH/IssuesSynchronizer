using Discord.WebSocket;
using IssuesSynchronizer;
using IssuesSynchronizer.Discord;
using IssuesSynchronizer.GitHub;
using IssuesSynchronizer.GitHub.Handlers;
using IssuesSynchronizer.GitHub.Infrastructure;
using IssuesSynchronizer.GitHub.Senders;
using IssuesSynchronizer.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Octokit;
using Octokit.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureFromSection<GitHubOption>(builder.Configuration);
builder.Services.ConfigureFromSection<DiscordSocketConfig>(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers(options => options.EnableEndpointRouting = false);

builder.Services.AddPooledDbContextFactory<IssuesSynchronizerDbContext>(optionsBuilder =>
    optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("Main")));
builder.Services.AddSingleton<DiscordSocketClient>();
builder.Services.AddSingleton<GitHubClient>(provider =>
    GitHubClientFactory.CreateGitHubAppClient(provider.GetService<IOptions<GitHubOption>>()!.Value));
builder.Services.AddSingleton<GitHubClientProvider>();
builder.Services.AddSingleton<GitHubToDiscordSenderService>();

builder.Services.AddHostedService<DiscordClientBackgroundService>();
builder.Services.AddHostedService<DiscordClientReliabilityBackgroundService>();
builder.Services.AddHostedService<GitHubClientBackgroundService>();

builder.Services.AddScopedGitHubWebHookHandlers(registry =>
{
    registry.RegisterHandler<GitHubIssueHandler>();
    registry.RegisterHandler<GitHubIssueCommentHandler>();
});

var app = builder.Build();

app.UseMvc();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<IssuesSynchronizerDbContext>>();
    await using var dbContext = await dbContextFactory.CreateDbContextAsync();
    await dbContext.Database.MigrateAsync();
}

app.Run();