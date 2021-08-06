using Roca.Bot.Slash.Info;
using System;
using System.Collections.Generic;

namespace Roca.Bot.Slash.Builder
{
    internal class CommandBuilder
    {
        private ModuleBuilder _module;

        public List<ParameterBuilder> Parameters = new();
        public string? Name { get; set; }
        public string? Description { get; set; }

        public CommandBuilder(ModuleBuilder module) => _module = module;

        public void AddParameter(Action<ParameterBuilder> action)
        {
            var builder = new ParameterBuilder(this);
            action(builder);
            Parameters.Add(builder);
        }

        public CommandInfo Build(ModuleInfo module) => new(this, module);
    }
}
