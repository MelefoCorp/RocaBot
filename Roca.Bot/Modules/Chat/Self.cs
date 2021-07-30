using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Roca.Core;
using System.Threading.Tasks;

namespace Roca.Bot.Modules.Chat
{
    public class Self : BaseCommandModule
    {
        [Command("ping")]
        public async Task Ping(CommandContext ctx)
        {
            var account = await ctx.User.GetAccount(false).ConfigureAwait(false);
            await ctx.RespondAsync(GetType().GetLocalizer()["ping"]).ConfigureAwait(false);
            await account.Save().ConfigureAwait(false);
        }
    }
}
