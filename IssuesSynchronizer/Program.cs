using Discord.WebSocket;
using IssuesSynchronizer.Discord;
using IssuesSynchronizer.Postgres;
using Microsoft.EntityFrameworkCore;
using Octokit.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<GitHubOption>(builder.Configuration.GetSection("GitHubApp"));
builder.Services.Configure<DiscordSocketConfig>(builder.Configuration);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddPooledDbContextFactory<IssuesSynchronizerDbContext>(optionsBuilder => 
    optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("Main")));
builder.Services.AddSingleton<DiscordShardedClient>();
builder.Services.AddHostedService<DiscordClientBackgroundService>();
builder.Services.AddHostedService<DiscordClientReliabilityBackgroundService>();

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
