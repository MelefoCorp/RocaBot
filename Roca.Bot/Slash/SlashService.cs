using Discord;
using Discord.Rest;
using Discord.WebSocket;
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

namespace Roca.Bot.Slash
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
            //TODO add choices (a.k.a enum ?)
            //TODO add permissions
            foreach (var assembly in assemblies)
            {
                var types = SlashBuilder.FindModules(assembly);
                var modules = SlashBuilder.BuildModules(types, this, _services);

                foreach (var module in modules)
                    _modules.TryAdd(module.Key, module.Value);
            }

            List<SlashCommandCreationProperties> commands = new();

            foreach (var module in _modules.Values)
            {
                if (module.Name == null)
                {
                    foreach (var command in module.Commands)
                        commands.Add(new SlashCommandBuilder
                        {
                            Name = command.Name,
                            Description = command.Description,
                            Options = AddParameters(command.Parameters)
                        }.Build());
                }
                else
                    commands.Add(new SlashCommandBuilder
                    {
                        Name = module.Name,
                        Description = module.Description,
                        Options = AddModule(module)
                    }.Build());
            }

           await _client.Rest.BulkOverwriteGlobalCommands(commands.ToArray()).ConfigureAwait(false);
        }

        private List<SlashCommandOptionBuilder> AddModule(ModuleInfo module)
        {
            List<SlashCommandOptionBuilder> subs = new();

            foreach (var command in module.Commands)
                subs.Add(new()
                { 
                    Name = command.Name,
                    Description = command.Description,
                    Type = ApplicationCommandOptionType.SubCommand,
                    Options = AddParameters(command.Parameters)
                });

            foreach (var group in module.Groups)
                subs.Add(new()
                {
                    Name = group.Name!,
                    Description = group.Description,
                    Type = ApplicationCommandOptionType.SubCommandGroup,
                    Options = AddCommands(group.Commands)
                });

            return subs;
        }

        private List<SlashCommandOptionBuilder> AddCommands(IReadOnlyCollection<CommandInfo> commands)
        {
            List<SlashCommandOptionBuilder> options = new();

            foreach (var command in commands)
                options.Add(new()
                {
                    Name = command.Name,
                    Description = command.Description,
                    Type = ApplicationCommandOptionType.SubCommand,
                    Options = AddParameters(command.Parameters)
                });

            return options;
        }

        private List<SlashCommandOptionBuilder> AddParameters(IReadOnlyCollection<Info.ParameterInfo> parameters)
        {
            List<SlashCommandOptionBuilder> options = new();

            foreach (var parameter in parameters)
                options.Add(new()
                {
                    Name = parameter.Name,
                    Description = parameter.Description,
                    Type = parameter.OptionType,
                    Required = !parameter.IsOptional
                });

            return options;
        }

        public async Task UnregisterCommandsAsync()
        {
            throw new NotImplementedException();
        }

        private async Task HandleInteractionAsync(SocketInteraction interaction)
        {
            switch (interaction)
            {
                case SocketSlashCommand command:
                    await HandleCommandAsync().ConfigureAwait(false);
                    return;
                case SocketMessageComponent component:
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
