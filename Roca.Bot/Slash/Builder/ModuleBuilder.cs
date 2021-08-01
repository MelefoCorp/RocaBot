using Roca.Bot.Slash.Info;
using Roca.Bot.Slash.Service;

namespace Roca.Bot.Slash.Builder
{
    internal class ModuleBuilder
    {
        private SlashService _service;
        private ModuleBuilder? _parent;

        public ModuleBuilder(SlashService service, ModuleBuilder? parent)
        {
            _service = service;
            _parent = parent;
        }

        public ModuleInfo Build()
        {
            return new();
        }
    }
}
