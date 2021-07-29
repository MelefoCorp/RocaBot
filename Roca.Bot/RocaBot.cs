using DSharpPlus;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Roca.Bot
{
    public class RocaBot
    {
        private DiscordShardedClient _client;

        public RocaBot(string token)
        {
            _client = new DiscordShardedClient(new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot,
                AlwaysCacheMembers = true,
                UseRelativeRatelimit = true,
                MessageCacheSize = 4096,
                AutoReconnect = true,
#if DEBUG
                MinimumLogLevel = LogLevel.Debug
#else
                MinimumLogLevel = LogLevel.Warning
#endif
            });
        }

        public async Task Start() => await _client.StartAsync().ConfigureAwait(false);

        public async Task Stop() => await _client.StopAsync().ConfigureAwait(false);
    }
}
