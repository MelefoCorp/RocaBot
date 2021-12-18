using Discord.Interactions;
using Roca.Core;
using Roca.Core.Translation;

namespace Roca.Bot.Commands
{
    public class RocaBase<T> : RocaBase
    {
        public T Service { get; set; }
    }

    public class RocaBase : InteractionModuleBase<RocaContext>
    {
        public RocaBase() : base() 
            => Localizer = GetType().GetLocalizer();

        public InteractionService Interaction { get; set; }
        public Rocalizer Localizer { get; }
    }
}
