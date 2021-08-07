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
        [RocaModule("test")]
        public class Test : RocaBase
        {
            [RocaCommand]
            public async Task Ping()
            {
                //await Context.RespondAsync(GetType().GetLocalizer()["ping"]).ConfigureAwait(false);
                //await Task.Delay(1000).ConfigureAwait(true);
                //var account = await Context.User.GetAccount(false).ConfigureAwait(false);
                //await Context.RespondAsync(GetType().GetLocalizer()["pong"]).ConfigureAwait(false);
                //await account.Save().ConfigureAwait(false);
            }
        }

        [RocaCommand]
        public async Task Command(string @string, long interger, bool boolean, IUser user, IChannel channel, IRole role, IMentionable mentionable, double number)
        {
        }
    }
}
