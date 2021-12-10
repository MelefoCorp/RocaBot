using Discord.Interactions;
using Discord.WebSocket;
using Roca.Core;
using Roca.Core.Accounts;

namespace Roca.Bot.Commands
{
    public class RocaContext : ShardedInteractionContext
    {
        public GuildAccount GuildAccount { get; }
        public UserAccount UserAccount { get; }
        public SocketGuildUser? Member { get; }
        public MemberAccount? MemberAccount { get; }

        public RocaContext(DiscordShardedClient client, SocketInteraction interaction) : base(client, interaction)
        {
            GuildAccount = Guild.GetAccount();
            UserAccount = User.GetAccount();
            Member = User as SocketGuildUser;
            MemberAccount = Member?.GetAccount();
        }
    }
}
