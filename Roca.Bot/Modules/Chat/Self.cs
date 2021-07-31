using Roca.Core;
using System.Threading.Tasks;

namespace Roca.Bot.Modules.Chat
{
    /*public class Self : BaseCommandModule
    {
        [Command("ping")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.RespondAsync(GetType().GetLocalizer()["ping"]).ConfigureAwait(false);
            await Task.Delay(1000).ConfigureAwait(true);
            var account = await ctx.User.GetAccount(false).ConfigureAwait(false);
            await ctx.RespondAsync(GetType().GetLocalizer()["pong"]).ConfigureAwait(false);
            await account.Save().ConfigureAwait(false);
        }
    }*/
}
