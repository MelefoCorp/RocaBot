using Discord.WebSocket;
using Discord;
using System;
using System.Threading.Tasks;

namespace Roca.Bot.Slash.Readers
{
    public class IntegerTypeReader : TypeReader
    {
        public override ApplicationCommandOptionType OptionType => ApplicationCommandOptionType.Integer;

        public override Task<object> ReadAsync(RocaContext context, SocketSlashCommandDataOption input, IServiceProvider services)
        {
            if (input.Type != OptionType)
                throw new ArgumentException("Wrong argument type received");
            return Task.FromResult(input.Value);
        }
    }
}
