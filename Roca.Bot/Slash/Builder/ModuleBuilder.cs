using Roca.Bot.Slash.Info;
using System;
using System.Collections.Generic;

namespace Roca.Bot.Slash.Builder
{
    internal class ModuleBuilder
    {
        private readonly SlashService _service;
        private readonly ModuleBuilder? _parent;

        public readonly List<CommandBuilder> Commands = new();
        public readonly List<ModuleBuilder> Groups = new();
        public string? Name { get; set; }
        public string? Description { get; set; }

        public ModuleBuilder(SlashService service, ModuleBuilder? parent)
        {
            _service = service;
            _parent = parent;
        }

        public ModuleInfo Build(ModuleInfo? parent = null) => new(this, _service, parent);

        public void AddCommand(Action<CommandBuilder> action)
        {
            var builder = new CommandBuilder(this);
            action(builder);
            Commands.Add(builder);
        }

        public void AddModule(Action<ModuleBuilder> action)
        {
            var builder = new ModuleBuilder(_service, this);
            action(builder);
            Groups.Add(builder);
        }
    }
}
