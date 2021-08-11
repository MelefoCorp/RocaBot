using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Roca.Bot.Slash
{
    public class RocaContext : ICommandContext
    {
        private SocketSlashCommand _command { get; }

        public IDiscordClient Client { get; }
        public IUser User => _command.User;
        public ISocketMessageChannel Channel => _command.Channel;
        public IGuild? Guild => (Channel as IGuildChannel)?.Guild;
        public IUserMessage? Message => null;

        internal RocaContext(IDiscordClient client, SocketSlashCommand command)
        {
            Client = client;
            _command = command;
        }

        public async Task<RestFollowupMessage> RespondAsync(string? text = null, Embed[]? embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions? allowedMentions = null, RequestOptions? options = null, MessageComponent? component = null, Embed? embed = null) => 
            await _command.FollowupAsync(text, embeds, isTTS, ephemeral, allowedMentions, options, component, embed).ConfigureAwait(false);

        IMessageChannel ICommandContext.Channel => Channel;
    }
}
