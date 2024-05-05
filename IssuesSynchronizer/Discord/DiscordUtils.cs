using System.Runtime.CompilerServices;
using Discord;
using Discord.Rest;

namespace IssuesSynchronizer.Discord;

public static class DiscordUtils
{
    public static RequestOptions DefaultRequestOptions = new() { RetryMode = RetryMode.AlwaysRetry };

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    public static extern RestThreadChannel CreateFakeThreadChannel(BaseDiscordClient discord, IGuild guild, ulong id,
        DateTimeOffset? createdAt);
}