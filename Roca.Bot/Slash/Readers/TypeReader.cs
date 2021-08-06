using Discord;
using System;
using System.Threading.Tasks;

namespace Roca.Bot.Slash.Readers
{
    public abstract class TypeReader
    {
        public abstract ApplicationCommandOptionType OptionType { get; }

        public abstract Task ReadAsync(RocaContext context, string input, IServiceProvider services);
    }
}
