using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Roca.Bot.Attributes;
using Roca.Bot.Commands;

namespace Roca.Bot.Modules.Moderation
{
    [Group("mod", "Moderation commands")]

    public class Info : RocaBase
    {
        private readonly Color _color = new(245, 150, 49);

        [RequireRole(Role.Mod)]
        [UserCommand("Whois")]
        public async Task Whois(SocketGuildUser user)
        {
            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = user.Username,
                    IconUrl = user.GetGuildAvatarUrl() ?? user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl()
                },
                Color = _color,
                Description = $"**User**: {user.Mention}/{user}/{user.Id}\n" +
                              $"**Nickname**: {user.Nickname}\n" +
                              $"**Roles**: {string.Join(' ', user.Roles.Where(x => !x.IsEveryone).OrderByDescending(x => x.Position).Select(x => x.Mention))}\n" +
                              $"**Created**: {TimestampTag.FromDateTime(user.CreatedAt.UtcDateTime)}\n" +
                              $"**Joined**: {(user.JoinedAt.HasValue ? TimestampTag.FromDateTime(user.JoinedAt.Value.UtcDateTime) : "")}\n" +
                              $"**Status**: {user.Status}\n" +
                              $"**Playing**: {string.Join('/', user.Activities.Select(x => $"`{x.Name}`{x.Details}"))}\n" +
                              $"**Mutes**: {0}\n" +
                              $"**Warns**: {0}\n" +
                              $"**Tempbans**: {0}\n"
            };
            await RespondAsync(embed: embed.Build(), ephemeral: true);
        }
    }
}
