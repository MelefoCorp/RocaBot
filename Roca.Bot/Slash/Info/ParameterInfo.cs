using DSharpPlus;
using Roca.Bot.Slash.Builder;
using Roca.Bot.Slash.Readers;
using System;

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
        }
    }
}
