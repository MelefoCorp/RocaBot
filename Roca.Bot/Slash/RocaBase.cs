using System;

namespace Roca.Bot.Slash
{
    public class RocaBase : IDisposable
    {
        public RocaContext Context { get; internal set; }

        public void Dispose() => GC.SuppressFinalize(this);
    }
}
