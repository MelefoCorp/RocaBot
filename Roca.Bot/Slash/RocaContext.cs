using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Roca.Core;
using Roca.Core.Accounts;
using System.Threading.Tasks;

namespace Roca.Bot.Slash
{
    public class RocaContext : ICommandContext
    {
        public SocketSlashCommand Command { get; }
        public IDiscordClient Client { get; }
        public IUser User => Command.User;
        public UserAccount UserAccount => User.GetAccount();
        public IGuildUser? Member => User as IGuildUser;
        public MemberAccount? MemberAccount => Member?.GetAccount();

        public ISocketMessageChannel Channel => Command.Channel;
        public IGuild? Guild => (Channel as IGuildChannel)?.Guild;
        public GuildAccount? GuildAccount => Guild?.GetAccount();
        public IUserMessage? Message => null;

        internal RocaContext(IDiscordClient client, SocketSlashCommand command)
        {
            Client = client;
            Command = command;
        }

        public async Task<RestFollowupMessage> RespondAsync(string? text = null, Embed[]? embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions? allowedMentions = null, RequestOptions? options = null, MessageComponent? component = null, Embed? embed = null) => 
            await Command.FollowupAsync(text, embeds, isTTS, ephemeral, allowedMentions, options, component, embed).ConfigureAwait(false);

        IMessageChannel ICommandContext.Channel => Channel;
    }
}
