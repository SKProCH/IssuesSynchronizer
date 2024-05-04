using Discord;
using Discord.WebSocket;

namespace IssuesSynchronizer.Discord;

public class DiscordClientReliabilityBackgroundService : BackgroundService {
    // --- Begin Configuration Section ---
    // How long should we wait on the client to reconnect before resetting?
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

    // Should we attempt to reset the client? Set this to false if your client is still locking up.
    private static readonly bool _attemptReset = true;
    // --- End Configuration Section ---

    private readonly Dictionary<IDiscordClient, CancellationTokenSource> _disconnectedClients = new();
    private readonly ILogger<DiscordClientReliabilityBackgroundService> _logger;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    public DiscordClientReliabilityBackgroundService(DiscordSocketClient mainDiscord, ILogger<DiscordClientReliabilityBackgroundService> logger, IHostApplicationLifetime hostApplicationLifetime) {
        _logger = logger;
        _hostApplicationLifetime = hostApplicationLifetime;

        mainDiscord.Connected += () => ConnectedAsync(mainDiscord);
        mainDiscord.Disconnected += exception => DisconnectedAsync(exception, mainDiscord);
    }

    public DiscordClientReliabilityBackgroundService(DiscordShardedClient mainDiscord, ILogger<DiscordClientReliabilityBackgroundService> logger, IHostApplicationLifetime hostApplicationLifetime) {
        _logger = logger;
        _hostApplicationLifetime = hostApplicationLifetime;

        mainDiscord.ShardConnected += ConnectedAsync;
        mainDiscord.ShardDisconnected += DisconnectedAsync;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    private Task DisconnectedAsync(Exception arg1, DiscordSocketClient arg2) {
        // Check the state after <timeout> to see if we reconnected
        _logger.LogInformation("Client disconnected, starting timeout task...");
        _disconnectedClients[arg2] = new CancellationTokenSource();
        _ = Task.Delay(Timeout, _disconnectedClients[arg2].Token).ContinueWith(async _ => {
            _logger.LogDebug("Timeout expired, continuing to check client state...");
            if (await CheckIsStateCorrectAsync(arg2))
                _logger.LogDebug("State came back okay");
            else
                FailFast();
        });

        return Task.CompletedTask;
    }

    private Task ConnectedAsync(DiscordSocketClient arg) {
        // Cancel all previous state checks and reset the CancelToken - client is back online
        _logger.LogDebug("Client reconnected, resetting cancel tokens...");
        if (_disconnectedClients.TryGetValue(arg, out var cts)) cts!.Cancel();

        _logger.LogDebug("Client reconnected, cancel tokens reset.");

        return Task.CompletedTask;
    }

    private async Task<bool> CheckIsStateCorrectAsync(IDiscordClient client) {
        // Client reconnected, no need to reset
        if (client.ConnectionState == ConnectionState.Connected) return true;
        if (_attemptReset) {
            _logger.LogInformation("Attempting to reset the client");

            var timeout = Task.Delay(Timeout);
            var connect = client.StartAsync();
            var task = await Task.WhenAny(timeout, connect);

            if (task == timeout) {
                _logger.LogCritical(null, "Client reset timed out (task deadlocked?), killing process");
                return false;
            }
            if (connect.IsFaulted) {
                _logger.LogCritical(connect.Exception, "Client reset faulted, killing process");
                return false;
            }
            if (connect.IsCompletedSuccessfully)
                _logger.LogInformation("Client reset successfully!");

            return true;
        }

        _logger.LogCritical(null, "Client did not reconnect in time, killing process");
        return false;
    }

    protected virtual void FailFast() {
        _hostApplicationLifetime.StopApplication();
    }
}