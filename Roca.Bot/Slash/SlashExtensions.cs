using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Roca.Bot.Slash
{
    public static class SlashExtensions
    {
        private static readonly MethodInfo _internalMethod = typeof(DiscordShardedClient)!.GetMethod("InitializeShardsAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;
        private static readonly Func<DiscordShardedClient, Task<int>> _initializeShardsAsync = (Func<DiscordShardedClient, Task<int>>)Delegate.CreateDelegate(typeof(Func<DiscordShardedClient, Task<int>>), _internalMethod);

        public static SlashCommandsExtension UseSlashCommands(this DiscordClient client, SlashCommandsConfiguration cfg)
        {
            if (client.GetExtension<SlashCommandsExtension>() != null)
                throw new InvalidOperationException("SlashCommands is alreay enabled for that client.");

            var slash = new SlashCommandsExtension();
            client.AddExtension(slash);
            return slash;
        }

        public static async Task<IReadOnlyDictionary<int, SlashCommandsExtension>> UseSlashCommandsAsync(this DiscordShardedClient client, SlashCommandsConfiguration cfg)
        {
            var exts = new Dictionary<int, SlashCommandsExtension>();
            await _initializeShardsAsync(client).ConfigureAwait(false);

            foreach (var shard in client.ShardClients)
            {
                var slash = shard.Value.GetExtension<SlashCommandsExtension>();
                if (slash == null)
                    slash = shard.Value.UseSlashCommands(cfg);

                exts[shard.Key] = slash;
            }

            return exts;
        }
    }
}
