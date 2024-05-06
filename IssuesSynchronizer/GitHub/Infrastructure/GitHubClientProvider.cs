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
    
    public async Task<GitHubClient?> CreateInstallationClientForRepo(long repositoryId)
    {
        var installation = await App.GitHubApps.GetRepositoryInstallationForCurrent(repositoryId);
        if (installation is null)
        {
            return null;
        }
        var gitHubInstallationClient = await GitHubClientFactory.CreateGitHubInstallationClient(_options.Value, installation.Id);
        return gitHubInstallationClient.Client;
    }
}