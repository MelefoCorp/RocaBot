using Roca.Bot.Slash.Builder;
using System.Collections.Generic;
using System.Linq;

namespace Roca.Bot.Slash.Info
{
    public class ModuleInfo
    {
        public SlashService Service { get; }
        public string? Name { get; }
        public string? Description { get; }

        public IReadOnlyCollection<CommandInfo> Commands { get; }
        public IReadOnlyCollection<ModuleInfo> Groups { get; }
        public ModuleInfo? Parent { get; }
        public bool IsGroup => Parent == null;

        internal ModuleInfo(ModuleBuilder builder, SlashService service, ModuleInfo? parent = null)
        {
            Service = service;
            Name = builder.Name;
            Description = builder.Description;
            Commands = builder.Commands.Select(x => x.Build(this)).ToArray();
            Groups = builder.Groups.Select(x => x.Build(this)).ToArray();
        }
    }
}
