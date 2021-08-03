using Roca.Bot.Slash.Info;
using Roca.Bot.Slash.Service;
using System;
using System.Collections.Generic;

namespace Roca.Bot.Slash.Builder
{
    internal class ModuleBuilder
    {
        private SlashService _service;
        private ModuleBuilder? _parent;
        private readonly List<CommandBuilder> _commands = new();
        private readonly List<ModuleBuilder> _groups = new();

        public string? Name { get; set; }
        public bool IsGroup { get; set; }

        public ModuleBuilder(SlashService service, ModuleBuilder? parent)
        {
            _service = service;
            _parent = parent;
        }

        public ModuleInfo Build()
        {
            return new();
        }

        public void AddCommand(Action<CommandBuilder> action)
        {
            var builder = new CommandBuilder(this);
            action(builder);
            _commands.Add(builder);
        }

        public void AddModule(Action<ModuleBuilder> action)
        {
            var builder = new ModuleBuilder(_service, this);
            action(builder);
            _groups.Add(builder);
        }
    }
}
