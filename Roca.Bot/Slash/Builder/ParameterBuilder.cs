using Roca.Bot.Slash.Info;
using Roca.Bot.Slash.Readers;
using System;

namespace Roca.Bot.Slash.Builder
{
    internal class ParameterBuilder
    {
        private CommandBuilder _command;

        public string? Name { get; set; }
        public string? Description { get; set; }

        public bool IsOptional { get; set; }
        public object? DefaultValue { get; set; }
        public Type? Type { get; set; }
        public TypeReader TypeReader { get; set; }

        public ParameterBuilder(CommandBuilder command) => _command = command;

        public ParameterInfo Build(CommandInfo command) => new(this, command);
    }
}
