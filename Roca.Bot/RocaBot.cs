using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Roca.Bot
{
    public class RocaBot
    {
        private DiscordShardedClient _client;
        private CommandsNextConfiguration _cmdConfig;
        private IReadOnlyDictionary<int, CommandsNextExtension>? _cmds;

        public RocaBot(IConfiguration configuration)
        {
            _client = new DiscordShardedClient(new DiscordConfiguration
            {
                Token = configuration["RocaBot:Token"],
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

            _cmdConfig = new CommandsNextConfiguration
            {
                CaseSensitive = false,
                EnableDefaultHelp = false,
                EnableDms = false,
                EnableMentionPrefix = false,
                IgnoreExtraArguments = true,
                StringPrefixes = new string[] { configuration["RocaBot:DefaultPrefix"] },
            };

            _cmds = null;
        }

        public async Task Start()
        {
            _cmds = await _client.UseCommandsNextAsync(_cmdConfig).ConfigureAwait(false);

            foreach (var cmd in _cmds)
                cmd.Value.RegisterCommands(GetType().Assembly);

            await _client.StartAsync().ConfigureAwait(false);
        }

        public async Task Stop()
        {
            await _client.StopAsync().ConfigureAwait(false);
            _cmds = null;
        }
    }
}
