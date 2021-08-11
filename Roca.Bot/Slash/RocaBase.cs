using Discord.Commands;
using System;

namespace Roca.Bot.Slash
{
    public class RocaBase : RocaBase<RocaContext> 
    {
    }

    public class RocaBase<T> : IDisposable where T : class, ICommandContext
    {
        public T Context { get; internal set; }

        public void Dispose() => GC.SuppressFinalize(this);
    }
}
