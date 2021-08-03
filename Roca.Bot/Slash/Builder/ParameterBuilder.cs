using System;

namespace Roca.Bot.Slash.Builder
{
    internal class ParameterBuilder
    {
        private CommandBuilder _command;

        public string? Name { get; set; }
        public bool IsOptional { get; set; }
        public object? DefaultValue { get; set; }
        public Type? Type { get; set; }

        public ParameterBuilder(CommandBuilder command) => _command = command;
    }
}
