using System;
using System.IO;
using System.Threading.Tasks;
using GitHubJwt;
using Octokit.Internal;

namespace Octokit.Bot;

public static class GitHubClientFactory
{
    public static GitHubClient CreateGitHubAppClient(GitHubOption option)
    {
        return GetAppClient(option, option.AppName);
    }

    public static async Task<InstallationContext> CreateGitHubInstallationClient(GitHubClient appClient,
        long installationId, string appName)
    {
        return await GetInstallationContext(appClient, installationId, appName);
    }

    public static async Task<InstallationContext> CreateGitHubInstallationClient(GitHubOption option,
        long installationId)
    {
        return await CreateGitHubInstallationClient(CreateGitHubAppClient(option), installationId, option.AppName);
    }

    private static async Task<InstallationContext> GetInstallationContext(GitHubClient appClient, long installationId,
        string appName)
    {
        var accessToken = await appClient.GitHubApps.CreateInstallationToken(installationId);

        var installationClient = new GitHubClient(new ProductHeaderValue($"{appName}-Installation{installationId}"),
            new InMemoryCredentialStore(new Credentials(accessToken.Token, AuthenticationType.Bearer)));

        return new InstallationContext(installationClient, accessToken);
    }

    private static GitHubClient GetAppClient(GitHubOption option, string appName)
    {
        var generator = new GitHubJwtFactory(
            new ClearTextPrivateKeySource(option.PrivateKey),
            new GitHubJwtFactoryOptions
            {
                AppIntegrationId = option.AppIdentifier, // The GitHub App Id
                ExpirationSeconds = 600 // 10 minutes is the maximum time allowed
            }
        );

        var jwtToken = generator.CreateEncodedJwtToken();
        
        return GetGitHubClient(appName, jwtToken);
    }

    private static GitHubClient GetGitHubClient(string appName, string jwtToken)
    {
        return new GitHubClient(new ProductHeaderValue(appName),
            new InMemoryCredentialStore(new Credentials(jwtToken, AuthenticationType.Bearer)));
    }

    public class ClearTextPrivateKeySource : IPrivateKeySource
    {
        protected readonly string Key;

        public ClearTextPrivateKeySource(string key)
        {
            this.Key = !string.IsNullOrEmpty(key) ? key : throw new ArgumentNullException(nameof (key));
        }

        public TextReader GetPrivateKeyReader()
        {
            return (TextReader) new StringReader(this.Key);
        }
    }
}