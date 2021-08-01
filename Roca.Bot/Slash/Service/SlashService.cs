using DSharpPlus;
using DSharpPlus.EventArgs;
using Roca.Bot.Slash.Service;
using Roca.Core;
using Roca.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Roca.Bot.Slash.Builder
{
    public class SlashService : IService
    {
        private DiscordShardedClient _client;

        public SlashService(DiscordShardedClient client)
        {
            _client = client;

            _client.InteractionCreated += HandleInteractionAsync;
        }

        public async Task RegisterCommandsAsync(Assembly assembly, IServiceProvider? services = null)
        {
            services ??= EmptyServiceProvider.Instance;

            var types = SlashBuilder.FindModules(assembly);
            var modules = SlashBuilder.BuildModules(types, this, services);
        }

        public async Task UnregisterCommandsAsync()
        {

        }

        private async Task HandleInteractionAsync(DiscordClient sender, InteractionCreateEventArgs e)
        {
            if (e.Handled)
                return;

            switch (e.Interaction.Type)
            {
                case InteractionType.ApplicationCommand:
                    await HandleCommandAsync().ConfigureAwait(false);
                    return;
                case InteractionType.Component:
                    await HandleComponentAsync().ConfigureAwait(false);
                    return;
            }

        }

        private async Task HandleCommandAsync()
        {
        }

        private async Task HandleComponentAsync()
        {

        }

        public void Dispose() => GC.SuppressFinalize(this);
    }
}
