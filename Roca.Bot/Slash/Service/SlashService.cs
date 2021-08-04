using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Roca.Bot.Slash.Info;
using Roca.Bot.Slash.Readers;
using Roca.Bot.Slash.Service;
using Roca.Core;
using Roca.Core.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Roca.Bot.Slash.Builder
{
    public class SlashService : IService
    {
        private DiscordShardedClient _client;
        private IServiceProvider _services;
        private ConcurrentDictionary<Type, TypeReader> _typeReaders = new();
        private ConcurrentDictionary<Type, ModuleInfo> _modules = new();
        private bool _enabled;

        public SlashService(DiscordShardedClient client, IServiceProvider? services)
        {
            _client = client;
            _services = services ?? EmptyServiceProvider.Instance;

            //STRING
            //_typeReaders[typeof(string)] =

            //INTEGER
            //_typeReaders[typeof(sbyte)] = sbyte.TryParse;
            //_typeReaders[typeof(byte)] = byte.TryParse;
            //_typeReaders[typeof(short)] = short.TryParse;
            //_typeReaders[typeof(ushort)] = ushort.TryParse;
            //_typeReaders[typeof(int)] = int.TryParse;
            //_typeReaders[typeof(uint)] = uint.TryParse;
            //_typeReaders[typeof(long)] = long.TryParse;
            //_typeReaders[typeof(ulong)] = ulong.TryParse;

            //BOOLEAN
            //_typeReaders[typeof(bool)] = bool.TryParse;

            //USER
            //_typeReaders[typeof(DiscordUser)] =


            //CHANNEL
            //_typeReaders[typeof(DiscordChannel)] =

            //ROLE
            //_typeReaders[typeof(DiscordRole)] = 

            //MENTIONABLE


            //NUMBER
            //_typeReaders[typeof(float)] = float.TryParse;
            //_typeReaders[typeof(double)] = double.TryParse;
            //_typeReaders[typeof(decimal)] = decimal.TryParse;
        }

        public async Task Enable()
        {
            if (_enabled)
                return;

            await RegisterCommandsAsync(_services.GetRequiredService<RocaBot>().Assembly).ConfigureAwait(false);

            _client.InteractionCreated += HandleInteractionAsync;
            _enabled = true;

            return;
        }

        public Task Disable()
        {
            if (!_enabled)
                return Task.CompletedTask;

            _client.InteractionCreated -= HandleInteractionAsync;
            _enabled = false;

            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync(params Assembly[] assemblies)
        {
            //TODO add command description
            //TODO add choices (a.k.a enum ?)
            //TODO add permissions
            foreach (var assembly in assemblies)
            {
                var types = SlashBuilder.FindModules(assembly);
                var modules = SlashBuilder.BuildModules(types, this, _services);

                foreach (var module in modules)
                    _modules.TryAdd(module.Key, module.Value);
            }

            List<DiscordApplicationCommand> commands = new();
            foreach (var module in _modules.Values)
            {
                if (module.Name == null)
                {
                    foreach(var command in module.Commands)
                        commands.Add(new(command.Name.ToLowerInvariant(), "command TODO DESC", AddParameters(command.Parameters)));
                }
                else
                    commands.Add(new(module.Name.ToLowerInvariant(), "module TODO DESC", AddModule(module)));
            }

            var client = _services.GetRequiredService<DiscordShardedClient>();
           await client.ShardClients[0].BulkOverwriteGlobalApplicationCommandsAsync(commands).ConfigureAwait(false);
        }

        private List<DiscordApplicationCommandOption> AddModule(ModuleInfo module)
        {
            List<DiscordApplicationCommandOption> subs = new();

            foreach (var command in module.Commands)
                subs.Add(new(command.Name.ToLowerInvariant(), "command TODO DESC", ApplicationCommandOptionType.SubCommand, null, null, AddParameters(command.Parameters)));

            foreach (var group in module.Groups)
                subs.Add(new(group.Name!.ToLowerInvariant(), "group TODO DESC", ApplicationCommandOptionType.SubCommandGroup, null, null, AddCommands(group.Commands)));

            return subs;
        }

        private List<DiscordApplicationCommandOption> AddCommands(IReadOnlyCollection<Info.CommandInfo> commands)
        {
            List<DiscordApplicationCommandOption> options = new();

            foreach (var command in commands)
                options.Add(new(command.Name.ToLowerInvariant(), "command TODO DESC", ApplicationCommandOptionType.SubCommand, null, null, AddParameters(command.Parameters)));

            return options;
        }

        private List<DiscordApplicationCommandOption> AddParameters(IReadOnlyCollection<Info.ParameterInfo> parameters)
        {
            List<DiscordApplicationCommandOption> options = new();

            foreach (var parameter in parameters)
                options.Add(new DiscordApplicationCommandOption(parameter.Name.ToLowerInvariant(), "parameter TODO DESC", parameter.OptionType, parameter.IsOptional));

            return options;
        }

        public async Task UnregisterCommandsAsync()
        {
            throw new NotImplementedException();
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
