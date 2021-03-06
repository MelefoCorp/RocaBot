using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Roca.Bot.Slash.Info;
using Roca.Bot.Slash.Readers;
using Roca.Core;
using Roca.Core.Interfaces;
using Roca.Core.Translation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Roca.Bot.Slash.Builder;

namespace Roca.Bot.Slash
{
    public class SlashService : IService
    {
        private readonly DiscordShardedClient _client;
        private readonly IServiceProvider _services;
        private readonly ConcurrentDictionary<Type, ModuleInfo> _modules = new();
        private bool _enabled;
        private readonly Rocalizer _localizer;

        internal readonly ConcurrentDictionary<Type, TypeReader> TypeReaders = new();

        public SlashService(DiscordShardedClient client, IServiceProvider? services)
        {
            _client = client;
            _services = services ?? EmptyServiceProvider.Instance;
            _localizer = GetType().GetLocalizer();

            //STRING
            TypeReaders[typeof(string)] = new StringTypeReader();

            //INTEGER
            //_typeReaders[typeof(sbyte)] = sbyte.TryParse;
            //_typeReaders[typeof(byte)] = byte.TryParse;
            //_typeReaders[typeof(short)] = short.TryParse;
            //_typeReaders[typeof(ushort)] = ushort.TryParse;
            //_typeReaders[typeof(int)] = int.TryParse;
            //_typeReaders[typeof(uint)] = uint.TryParse;
            TypeReaders[typeof(long)] = new IntegerTypeReader();
            //_typeReaders[typeof(ulong)] = ulong.TryParse;

            //BOOLEAN
            TypeReaders[typeof(bool)] = new BooleanTypeReader();

            //USER
            TypeReaders[typeof(IUser)] = new UserTypeReader();


            //CHANNEL
            TypeReaders[typeof(IChannel)] = new ChannelTypeReader();

            //ROLE
            TypeReaders[typeof(IRole)] = new RoleTypeReader();

            //MENTIONABLE
            TypeReaders[typeof(IMentionable)] = new MentionableTypeReader();

            //NUMBER
            //_typeReaders[typeof(float)] = float.TryParse;
            TypeReaders[typeof(double)] = new NumberTypeReader();
            //_typeReaders[typeof(decimal)] = decimal.TryParse;
        }

        public async Task Enable()
        {
            if (_enabled)
                return;

            await RegisterCommandsAsync(_services.GetRequiredService<RocaBot>().Assembly).ConfigureAwait(false);

            _client.InteractionCreated += HandleInteractionAsync;
            _enabled = true;
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

                foreach (var (key, module) in modules)
                    _modules.TryAdd(key, module);
            }

            List<SlashCommandCreationProperties> commands = new();

            foreach (var module in _modules.Values)
            {
                if (module.Name == null)
                {
                    commands.AddRange(module.Commands.Select(command => new SlashCommandBuilder
                    {
                        Name = command.Name,
                        Description = command.Description,
                        Options = AddParameters(command.Parameters)
                    }.Build()));
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

        private static List<SlashCommandOptionBuilder> AddModule(ModuleInfo module)
        {
            List<SlashCommandOptionBuilder> subs = module.Commands.Select(command => new SlashCommandOptionBuilder()
            {
                Name = command.Name,
                Description = command.Description,
                Type = ApplicationCommandOptionType.SubCommand,
                Options = AddParameters(command.Parameters)
            }).ToList();
            
            subs.AddRange(module.Groups.Select(group => new SlashCommandOptionBuilder()
            {
                Name = group.Name!, 
                Description = group.Description, 
                Type = ApplicationCommandOptionType.SubCommandGroup,
                Options = AddCommands(group.Commands)
            }));

            return subs;
        }

        private static List<SlashCommandOptionBuilder> AddCommands(IReadOnlyCollection<CommandInfo> commands)
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

        private static List<SlashCommandOptionBuilder> AddParameters(IReadOnlyCollection<Info.ParameterInfo> parameters)
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
                    await HandleCommandAsync(command).ConfigureAwait(false);
                    return;
            }

        }

        private async Task HandleCommandAsync(SocketSlashCommand command)
        {
            //TODO Improve how to find command 
            try
            {
                CommandInfo result;
                IEnumerable<SocketSlashCommandDataOption> opts;

                switch (command.Data.Options.Count)
                {
                    case 1 when command.Data.Options.First().Type == ApplicationCommandOptionType.SubCommandGroup:
                        (result, opts) = FindCommand(FindModule(command).Groups, command.Data.Options.First());
                        break;
                    case 1 when command.Data.Options.First().Type == ApplicationCommandOptionType.SubCommand:
                        (result, opts) = FindCommand(FindModule(command).Commands, command.Data.Options.First());
                        break;
                    default:
                        result = _modules.Values.Where(x => x.Name == null).SelectMany(x => x.Commands).Single(x => x.Name == command.Data.Name);
                        opts = command.Data.Options;
                        break;
                }
                
                await result.ExecuteAsync(command, opts, _client, _services).ConfigureAwait(false);
            }
            catch (InvalidOperationException)
            {
                await command.RespondAsync(_localizer[CultureInfo.GetCultureInfo("en-US"), "cmd_not_found"], ephemeral: true).ConfigureAwait(false);
            }
        }

        private ModuleInfo FindModule(SocketSlashCommand command) =>
            _modules.Values.Single(x => x.Name == command.Data.Name);

        private static (CommandInfo, IEnumerable<SocketSlashCommandDataOption>) FindCommand(IEnumerable<ModuleInfo> groups, SocketSlashCommandDataOption command) =>
            FindCommand(groups.Single(x => x.Name == command.Name).Commands, command.Options.First());

        private static (CommandInfo, IEnumerable<SocketSlashCommandDataOption>) FindCommand(IEnumerable<CommandInfo> commands, SocketSlashCommandDataOption command) => 
            (commands.Single(x => x.Name == command.Name), command.Options);

        public void Dispose() => GC.SuppressFinalize(this);
    }
}
