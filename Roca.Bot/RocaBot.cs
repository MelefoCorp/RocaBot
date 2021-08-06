using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Roca.Bot.Slash;
using Roca.Bot.Slash.Builder;
using Roca.Bot.Slash.Service;
using Roca.Core.Extensions;
using Roca.Core.Interfaces;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Roca.Bot
{
    public class RocaBot
    {
        private readonly DiscordShardedClient _client;
        private readonly IServiceProvider _services;
        private readonly IConfiguration _configuration;

        internal Assembly Assembly;

        public RocaBot(IConfiguration configuration)
        {
            Assembly = GetType().Assembly;

            _configuration = configuration;

            if (!int.TryParse(_configuration["RocaBot:Shards"], out int shards))
                throw new ArgumentException("You must put a valid number in \"Shards\" variable of \"RocaBot\" section inside your configuration");

            _client = new DiscordShardedClient(new DiscordSocketConfig
            {
                AlwaysAcknowledgeInteractions = false,
                GatewayIntents = GatewayIntents.All,
                LargeThreshold = 250,
                AlwaysDownloadUsers = true,
                TotalShards = shards,
                MessageCacheSize = 4096,
                DefaultRetryMode = RetryMode.AlwaysRetry,
                MaxWaitBetweenGuildAvailablesBeforeReady = 10,
#if DEBUG
                LogLevel = LogSeverity.Debug
#else
                LogLevel = LogSeverity.Warning
#endif
            });

            //TODO Add a custom Logger
            _client.Log += l =>
            {
                Console.WriteLine($"[{DateTime.UtcNow}] [{l.Source}] [{l.Severity}] {l.Message}");
                return Task.CompletedTask;
            };

            _services = new ServiceCollection()
                .AddSingleton(this)
                .AddSingleton(_client)
                .AddSingletonInterface<IService>(Assembly)
                .BuildServiceProvider();
        }

        public async Task Start()
        {
            await _client.LoginAsync(TokenType.Bot, _configuration["RocaBot:Token"]).ConfigureAwait(false);
            await _client.StartAsync().ConfigureAwait(false);

            _client.ShardReady += _client_ShardReady;

        }

        private async Task _client_ShardReady(DiscordSocketClient arg)
        {
            _client.ShardReady -= _client_ShardReady;

            foreach (var service in _services.GetServices<IService>())
                await service.Enable().ConfigureAwait(false);
        }

        public async Task Stop()
        {
            foreach (var service in _services.GetServices<IService>())
                await service.Disable().ConfigureAwait(false);

            await _client.StopAsync().ConfigureAwait(false);
            await _client.LogoutAsync().ConfigureAwait(false);
        }
    }
}
