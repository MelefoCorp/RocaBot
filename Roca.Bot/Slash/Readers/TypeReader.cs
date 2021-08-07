using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Roca.Bot.Slash.Readers
{
    public abstract class TypeReader
    {
        public abstract ApplicationCommandOptionType OptionType { get; }

        public abstract Task<object> ReadAsync(RocaContext context, SocketSlashCommandDataOption input, IServiceProvider services);
    }
}
