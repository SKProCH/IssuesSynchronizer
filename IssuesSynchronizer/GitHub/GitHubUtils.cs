using System.Text.RegularExpressions;

namespace IssuesSynchronizer.GitHub;

public static partial class GitHubUtils
{
    [GeneratedRegex(@"https:\/\/github\.com\/([\w\.@\:\-~]+)\/([\w\.@\:\-~]+)")]
    public static partial Regex GitHubRepoRegex();

    public static bool TryParseRepoUrl(string url, out string owner, out string repo)
    {
        owner = string.Empty;
        repo = string.Empty;
        var match = GitHubRepoRegex().Match(url);
        if (match.Groups.Count != 3)
        {
            return false;
        }
        
        owner = match.Groups[1].Value;
        repo = match.Groups[2].Value;
        return true;
    }
}