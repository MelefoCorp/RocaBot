using System;
using System.Collections.Generic;

namespace Roca.Bot.Slash.Builder
{
    internal class CommandBuilder
    {
        private ModuleBuilder _module;
        private List<ParameterBuilder> _parameters = new();

        public string? Name { get; set; }

        public CommandBuilder(ModuleBuilder module) => _module = module;

        public void AddParameter(Action<ParameterBuilder> action)
        {
            var builder = new ParameterBuilder(this);
            action(builder);
            _parameters.Add(builder);
        }
    }
}
