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
using System.Reflection;
using System.Threading.Tasks;

namespace Roca.Bot
{
    public class RocaBot
    {
        private static readonly MethodInfo _internalMethod = typeof(DiscordShardedClient)!.GetMethod("InitializeShardsAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;
        private static readonly Func<DiscordShardedClient, Task<int>> _initializeShardsAsync = (Func<DiscordShardedClient, Task<int>>)Delegate.CreateDelegate(typeof(Func<DiscordShardedClient, Task<int>>), _internalMethod);

        private readonly DiscordShardedClient _client;
        private readonly IServiceProvider _services;

        internal Assembly Assembly;

        public RocaBot(IConfiguration configuration)
        {
            Assembly = GetType().Assembly;

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
                .AddSingleton(this)
                .AddSingleton(_client)
                .AddSingletonInterface<IService>(Assembly)
                .BuildServiceProvider();
        }

        public async Task Start()
        {
            await _initializeShardsAsync(_client).ConfigureAwait(false);

            await _client.StartAsync().ConfigureAwait(false);

            foreach (var service in _services.GetServices<IService>())
                await service.Enable().ConfigureAwait(false);
        }

        public async Task Stop()
        {
            await _services.GetRequiredService<SlashService>().UnregisterCommandsAsync().ConfigureAwait(false);
            await _client.StopAsync().ConfigureAwait(false);
        }
    }
}
