using Discord;
using Discord.WebSocket;

namespace IssuesSynchronizer.Discord;

public class DiscordClientBackgroundService : BackgroundService
{
    private readonly DiscordShardedClient _discordShardedClient;
    private readonly IConfiguration _configuration;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private ILogger _logger;

    public DiscordClientBackgroundService(DiscordShardedClient discordShardedClient, IConfiguration configuration, ILoggerFactory loggerFactory, IHostApplicationLifetime hostApplicationLifetime)
    {
        _discordShardedClient = discordShardedClient;
        _configuration = configuration;
        _hostApplicationLifetime = hostApplicationLifetime;
        _logger = loggerFactory.CreateLogger("Discord.Net");
        _discordShardedClient.Log += DiscordShardedClientOnLog;
    }

    private Task DiscordShardedClientOnLog(LogMessage arg)
    {
        var logLevel = arg.Severity switch {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Debug,
            LogSeverity.Debug => LogLevel.Trace,
            _ => throw new ArgumentOutOfRangeException()
        };

        _logger.Log(logLevel, arg.Exception, "[{Source}] {Message}", arg.Source, arg.Message);
        
        return Task.CompletedTask;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var token = _configuration.GetValue<string>("DiscordBotToken");
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogCritical("Discord bot token is empty. Shutdown");
            _hostApplicationLifetime.StopApplication();
        }
        
        await _discordShardedClient.LoginAsync(TokenType.Bot, token);
        await _discordShardedClient.StartAsync();
        
        await base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}
