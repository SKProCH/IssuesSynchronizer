using Discord.WebSocket;
using IssuesSynchronizer.Discord;
using IssuesSynchronizer.GitHub;
using IssuesSynchronizer.GitHub.Handlers;
using IssuesSynchronizer.GitHub.Infrastructure;
using IssuesSynchronizer.GitHub.Senders;
using IssuesSynchronizer.Postgres;
using Microsoft.EntityFrameworkCore;
using Octokit;
using Octokit.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<GitHubOption>(builder.Configuration.GetSection("GitHubOption"));
builder.Services.Configure<DiscordSocketConfig>(builder.Configuration);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddPooledDbContextFactory<IssuesSynchronizerDbContext>(optionsBuilder =>
    optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("Main")));
builder.Services.AddSingleton<DiscordShardedClient>();
builder.Services.AddSingleton<GitHubClient>(provider =>
    GitHubClientFactory.CreateGitHubAppClient(provider.GetService<GitHubOption>()));
builder.Services.AddSingleton<GitHubToDiscordSenderService>();

builder.Services.AddHostedService<DiscordClientBackgroundService>();
builder.Services.AddHostedService<DiscordClientReliabilityBackgroundService>();
builder.Services.AddHostedService<GitHubClientBackgroundService>();

builder.Services.AddScopedGitHubWebHookHandlers(registry => registry.RegisterHandler<GitHubIssueHandler>());

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<IssuesSynchronizerDbContext>>();
    await using var dbContext = await dbContextFactory.CreateDbContextAsync();
    await dbContext.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();