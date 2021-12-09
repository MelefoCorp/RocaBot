using Discord.Interactions;
using System.Threading.Tasks;

namespace Roca.Bot.Modules.Chat
{
    [Group("self", "A list of commands used to control RocaBot")]
    public class Self : RocaBase
    {
        [SlashCommand("ping", "ping")]
        public async Task Ping() => await RespondAsync(Localizer[Context.GuildAccount!.Language, "pong"]);
    }
}
