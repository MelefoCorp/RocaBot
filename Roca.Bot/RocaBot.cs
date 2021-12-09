using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Roca.Core.Extensions;
using Roca.Core.Interfaces;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Victoria;

namespace Roca.Bot
{
    public class RocaBot
    {
        private readonly DiscordShardedClient _client;
        private readonly InteractionService _interaction;
        private readonly IServiceProvider _services;
        private readonly IConfiguration _configuration;
        private readonly Assembly _assembly = Assembly.GetExecutingAssembly();
        private int _shards;
        
        public event Func<DiscordShardedClient, Task> Ready;

        public RocaBot(IConfiguration configuration)
        {
            _configuration = configuration;

            if (!int.TryParse(_configuration["RocaBot:Shards"], out int shards))
                throw new ArgumentException("You must put a valid number in \"Shards\" variable of \"RocaBot\" section inside your configuration");

            _client = new DiscordShardedClient(new DiscordSocketConfig
            {
                //AlwaysAcknowledgeInteractions = false,
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

            _interaction = new InteractionService(_client);

            //TODO Add a custom Logger
            _client.Log += l =>
            {
                Console.WriteLine($"[{DateTime.UtcNow}] [{l.Source}] [{l.Severity}] {l.Message}");
                return Task.CompletedTask;
            };
            _interaction.Log += l =>
            {
                Console.WriteLine($"[{DateTime.UtcNow}] [{l.Source}] [{l.Severity}] {l.Message}");
                return Task.CompletedTask;
            };

            _services = new ServiceCollection()
                .AddSingleton(this)
                .AddSingleton(_client)
                .AddSingleton(_interaction)
                .AddLavaNode(x =>
                {
                    x.Port = ushort.Parse(configuration["LavaLink:Port"]);
                    x.Hostname = configuration["LavaLink:Address"];
                    x.Authorization = configuration["LavaLink:Password"];
                    x.SelfDeaf = false;
                })
                .AddSingletonInterface<IService>(_assembly)
                .BuildServiceProvider();

            Ready += EnableServices;
            _client.ShardReady += ShardReady;
        }

        private async Task ShardReady(DiscordSocketClient _)
        {
            if (++_shards < _client.Shards.Count)
                return;
            _client.ShardReady -= ShardReady;

            await Ready.Invoke(_client).ConfigureAwait(false);
        }

        private async Task EnableServices(DiscordShardedClient _)
        {
            foreach (var service in _services.GetServices<IService>())
                await service.Enable().ConfigureAwait(false);
#if DEBUG
            await _services.GetRequiredService<InteractionService>().RegisterCommandsToGuildAsync(ulong.Parse(_configuration["Discord:Owner"]), true);
#else
            await _services.GetRequiredService<InteractionService>().RegisterCommandsGloballyAsync(true);
#endif
        }

        public async Task Start()
        {
            await _client.LoginAsync(TokenType.Bot, _configuration["RocaBot:Token"]).ConfigureAwait(false);
            await _client.StartAsync().ConfigureAwait(false);
        }

        public async Task Stop()
        {
            _shards = 0;

            foreach (var service in _services.GetServices<IService>())
                await service.Disable().ConfigureAwait(false);

            await _client.StopAsync().ConfigureAwait(false);
            await _client.LogoutAsync().ConfigureAwait(false);

        }
    }
}
