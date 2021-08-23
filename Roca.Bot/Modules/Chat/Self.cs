using Discord;
using Microsoft.Extensions.Localization;
using Roca.Bot.Slash;
using Roca.Bot.Slash.Attributes;
using Roca.Core;
using System.Threading.Tasks;

namespace Roca.Bot.Modules.Chat
{
    [RocaModule("self")]
    public class Self : RocaBase
    {
        [RocaCommand]
        public async Task Ping() => await ReplyAsync(Localizer["pong"]);
    }
}
