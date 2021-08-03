using DSharpPlus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Roca.Bot.Slash;
using Roca.Bot.Slash.Builder;
using Roca.Bot.Slash.Service;
using Roca.Core.Extensions;
using Roca.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Roca.Bot
{
    public class RocaBot
    {
        private readonly DiscordShardedClient _client;
        private readonly IServiceProvider _services;
        private Assembly _assembly;

        public RocaBot(IConfiguration configuration)
        {
            _assembly = GetType().Assembly;

            _client = new DiscordShardedClient(new DiscordConfiguration
            {
                Token = configuration["RocaBot:Token"],
                TokenType = TokenType.Bot,
                AlwaysCacheMembers = true,
                UseRelativeRatelimit = true,
                MessageCacheSize = 4096,
                AutoReconnect = true,
                Intents = DiscordIntents.All,
#if DEBUG
                MinimumLogLevel = LogLevel.Debug
#else
                MinimumLogLevel = LogLevel.Warning
#endif
            });

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingletonInterface<IService>(_assembly)
                .BuildServiceProvider();
        }

        public async Task Start()
        {
            await _services.GetRequiredService<SlashService>().RegisterCommandsAsync(_assembly).ConfigureAwait(false);
            await _client.StartAsync().ConfigureAwait(false);
        }

        public async Task Stop()
        {
            await _services.GetRequiredService<SlashService>().UnregisterCommandsAsync().ConfigureAwait(false);
            await _client.StopAsync().ConfigureAwait(false);
        }
    }
}
