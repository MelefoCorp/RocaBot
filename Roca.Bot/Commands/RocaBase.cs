using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Roca.Core;
using Roca.Core.Accounts;
using Roca.Core.Translation;

namespace Roca.Bot
{
    public class RocaBase<T> : RocaBase
    {
        public T Service { get; set; }
    }

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

    public class RocaBase : InteractionModuleBase<RocaContext>
    {
        public RocaBase() : base() 
            => Localizer = GetType().GetLocalizer();

        public Rocalizer Localizer { get; }
    }
}
