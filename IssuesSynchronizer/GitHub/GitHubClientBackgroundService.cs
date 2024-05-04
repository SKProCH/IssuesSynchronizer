using Microsoft.Extensions.Options;
using Octokit.Bot;

namespace IssuesSynchronizer.GitHub;

public class GitHubClientBackgroundService(IOptions<GitHubOption> gitHubOptions, ILogger<GitHubClientBackgroundService> logger)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var gitHubAppClient = GitHubClientFactory.CreateGitHubAppClient(gitHubOptions.Value);
        var gitHubApp = await gitHubAppClient.GitHubApps.GetCurrent();
        logger.LogInformation("Logged to GitHub app: {Name} ({Id})", gitHubApp.Name, gitHubApp.Id);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}