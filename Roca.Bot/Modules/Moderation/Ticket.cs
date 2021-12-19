using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Roca.Bot.Attributes;
using Roca.Bot.Commands;
using Roca.Core;
using Roca.Core.Accounts;
using Roca.Core.Extensions;
using System.Threading.Tasks;

namespace Roca.Bot.Modules.Moderation
{
    [Group("mod", "Moderation commands")]
    public class Ticket : RocaBase
    {
        internal static readonly Color Color = new(209, 17, 71);

        [RequireRole(Role.Mod)]
        [SlashCommand("ticket", "Start a new ticket context")]
        public async Task Support()
        {
            var embed = new EmbedBuilder
            {
                Title = "Support",
                Description = "Create a ticket with the button below",
                Color = Color
            };
            var btn = new ComponentBuilder().WithButton("Create a ticket",
                Interaction.GetComponentCommandInfo<Ticket>(nameof(Create)).GetCustomId(), emote: new Emoji("🎟️"));

            await RespondAsync(embed: embed.Build(), component: btn.Build());
        }

        [ComponentInteraction("create_ticket")]
        [RequireRole(Role.Helper)]
        [Cooldown(1, Measure.Minute)]
        public async Task Create()
        {
            await DeferAsync();

            var embed = new EmbedBuilder
            {
                Description = Context.GuildAccount.Moderation.Helper.Info ??
                              "The support comes to your rescue.\nIn the meantime, you can send as much information as you can to help them.",
                Footer = new EmbedFooterBuilder
                {
                    Text = "To close this ticket use the 🔒 Close button."
                },
                Color = Color
            };
            var components = new ComponentBuilder().WithButton("Close",
                Interaction.GetComponentCommandInfo<Ticket>(nameof(Close)).GetCustomId(), emote: new Emoji("🔒"));

            var channel = await Context.Guild.CreateTextChannelAsync(
                $"ticket_{Context.GuildAccount.Moderation.Helper.TicketsCount:x8}", x =>
                {
                    if (Context.GuildAccount.Moderation.Helper.Category.HasValue)
                        x.CategoryId = Context.GuildAccount.Moderation.Helper.Category!.Value;
                });

            Context.GuildAccount.Moderation.Helper.Tickets.Add(channel.Id, new(Context.GuildAccount.Moderation.Helper.TicketsCount++, Context.User.Id));
            await Context.GuildAccount.Save();

            await channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole,
                OverwritePermissions.DenyAll(channel));
            OverwritePermissions perms = new(sendMessages: PermValue.Allow, viewChannel: PermValue.Allow, readMessageHistory: PermValue.Allow);
            if (Context.GuildAccount.Moderation.Role.HasValue)
            {
                var mods = Context.Guild.GetRole(Context.GuildAccount.Moderation.Role.Value);
                if (mods != null)
                    await channel.AddPermissionOverwriteAsync(mods, perms);
                await channel.AddPermissionOverwriteAsync(Context.User, perms);
            }

            var msg = await channel.SendMessageAsync($"Hello {Context.User.Mention}", embed: embed.Build(),
                component: components.Build());

            await msg.PinAsync();
            await channel.DeleteMessagesAsync(await channel.GetMessagesAsync(1).FlattenAsync());
        }

        [ComponentInteraction("close_ticket")]
        [RequireRole(Role.Helper)]
        public async Task Close()
        {
            await DeferAsync();

            var channel = Context.Channel as SocketTextChannel;
            var ticket = Context.GuildAccount.Moderation.Helper.Tickets[channel!.Id];

            if (ticket.Status == TicketStatus.Closed)
                return;

            await channel.SyncPermissionsAsync();
            await channel.ModifyAsync(x => x.Name = $"closed_{ticket.Id:x8}");

            ticket.Status = TicketStatus.Closed;
            await Context.GuildAccount.Save();

            var embed = new EmbedBuilder
            {
                Description = $"Ticket closed by {Context.User.Mention}",
                Color = Color
            };
            var components = new ComponentBuilder().WithButton("Re-Open", Interaction.GetComponentCommandInfo<Ticket>(nameof(ReOpen)).GetCustomId(), emote: new Emoji("🔓"));
            await channel.SendMessageAsync(embed: embed.Build(), component: components.Build());
        }

        [ComponentInteraction(("repoen_ticket"))]
        [RequireRole(Role.Helper)]
        public async Task ReOpen()
        {
            var channel = Context.Channel as SocketTextChannel;
            var ticket = Context.GuildAccount.Moderation.Helper.Tickets[channel!.Id];

            await ((SocketMessageComponent)Context.Interaction).UpdateAsync(x => x.Components = null);

            ticket.Status = TicketStatus.Open;
            await Context.GuildAccount.Save();

            var embed = new EmbedBuilder
            {
                Description = $"Ticket re-opened by {Context.User.Mention}",
                Color = Color
            };
            await channel.SendMessageAsync(embed: embed.Build());

            await channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole,
                OverwritePermissions.DenyAll(channel));
            OverwritePermissions perms = new(sendMessages: PermValue.Allow, viewChannel: PermValue.Allow, readMessageHistory: PermValue.Allow);
            if (Context.GuildAccount.Moderation.Role.HasValue)
            {
                var mods = Context.Guild.GetRole(Context.GuildAccount.Moderation.Role.Value);
                if (mods != null)
                    await channel.AddPermissionOverwriteAsync(mods, perms);
                await channel.AddPermissionOverwriteAsync(Context.Guild.GetUser(ticket.User), perms);
            }
        }
    }
}