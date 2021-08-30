using Roca.Bot.Slash;
using Roca.Bot.Slash.Attributes;
using System.Threading.Tasks;

namespace Roca.Bot.Modules.Chat
{
    [RocaModule]
    public class Self : RocaBase
    {
        [RocaCommand]
        public async Task Ping() => await ReplyAsync(Localizer[Context.GuildAccount!.Language, "pong"]);
    }
}
