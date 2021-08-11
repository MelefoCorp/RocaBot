using Discord;
using Discord.WebSocket;
using Roca.Bot.Slash.Builder;
using Roca.Bot.Slash.Readers;
using System;
using System.Threading.Tasks;

namespace Roca.Bot.Slash.Info
{
    public class ParameterInfo
    {
        private TypeReader _typeReader { get; }

        public CommandInfo Command { get; }
        public string Name { get; }
        public string Description { get; }
        public bool IsOptional { get; }
        public object? DefaultValue { get; }
        public Type Type { get; }
        public ApplicationCommandOptionType OptionType => _typeReader.OptionType;


        internal ParameterInfo(ParameterBuilder builder, CommandInfo command)
        {
            Command = command;
            Name = builder.Name!;
            Description = builder.Description!;
            IsOptional = builder.IsOptional;
            DefaultValue = builder.DefaultValue;
            Type = builder.Type!;
            _typeReader = builder.TypeReader!;
        }

        public async Task<object> ParseAsync(RocaContext context, SocketSlashCommandDataOption option, IServiceProvider services) => 
            await _typeReader.ReadAsync(context, option, services).ConfigureAwait(false);
    }
}
