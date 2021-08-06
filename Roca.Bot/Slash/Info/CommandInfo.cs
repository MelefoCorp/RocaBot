using Roca.Bot.Slash.Builder;
using System.Collections.Generic;
using System.Linq;

namespace Roca.Bot.Slash.Info
{
    public class CommandInfo
    {
        public ModuleInfo Module { get; }
        public string Name { get; }
        public string Description { get; }

        public IReadOnlyCollection<ParameterInfo> Parameters { get; }

        internal CommandInfo(CommandBuilder builder, ModuleInfo module)
        {
            Module = module;
            Name = builder.Name!;
            Description = builder.Description!;
            Parameters = builder.Parameters.Select(x => x.Build(this)).ToArray();
        }
    }
}
