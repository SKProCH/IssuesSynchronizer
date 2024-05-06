using Microsoft.Extensions.Options;
using Octokit;
using Octokit.Bot;

namespace IssuesSynchronizer.GitHub.Infrastructure;

public class GitHubClientProvider(GitHubClient _gitHubClient, IOptions<GitHubOption> _options)
{
    public GitHubClient App => _gitHubClient;

    public async Task<GitHubClient> CreateInstallationClient(long installationId)
    {
        var gitHubInstallationClient = await GitHubClientFactory.CreateGitHubInstallationClient(_options.Value, installationId);
        return gitHubInstallationClient.Client;
    }
}