using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FluentScheduler;
using Roca.Core;
using Roca.Core.Accounts;
using Roca.Core.Extensions;
using Roca.Core.Interfaces;

namespace Roca.Bot.Modules.Moderation.Services
{
    public class TicketService : IService
    {
        private bool _enabled;
        private readonly string _jobName = "ticket_job";
        private readonly DiscordShardedClient _client;
        private readonly InteractionService _interaction;

        public TicketService(DiscordShardedClient client, InteractionService interaction)
        {
            _client = client;
            _interaction = interaction;
        }

        public Task Enable()
        {
            if (_enabled)
                return Task.CompletedTask;

            _enabled = true;
            JobManager.AddJob(async () => await CheckTickets(), x => x.WithName(_jobName).ToRunEvery(1).Minutes());
            return Task.CompletedTask;
        }

        public Task Disable()
        {
            if (!_enabled)
                return Task.CompletedTask;

            JobManager.RemoveJob(_jobName);
            _enabled = false;
            return Task.CompletedTask;
        }

        public async Task CheckTickets()
        {
            foreach (var account in await _client.GetGuildAccounts())
                foreach ((ulong channel, var ticket) in account.Moderation.Helper.Tickets)
                    await CheckTicket(account, channel, ticket);
        }

        private async Task CheckTicket(GuildAccount account, ulong channelId, Core.Accounts.Ticket ticket)
        {
            var guild = _client.GetGuild(account.Id);
            var channel = guild.GetTextChannel(channelId);
            var msgs = await channel.GetMessagesAsync(1).FlattenAsync();
            var last = msgs.SingleOrDefault();
            var time = DateTimeOffset.MinValue;

            if (last != null)
                time = last.EditedTimestamp ?? last.Timestamp;
            time = time.Add(account.Moderation.Helper.Duration ?? TimeSpan.FromDays(7));

            if (time >= DateTime.UtcNow)
                return;
            switch (ticket.Status)
            {
                case TicketStatus.Closed:
                    await DeleteTicket(channel, account, ticket);
                    break;
                case TicketStatus.Open:
                default:
                    await CloseTicket(channel, ticket);
                    break;
            }

            await account.Save();
        }

        private async Task CloseTicket(SocketTextChannel channel, Core.Accounts.Ticket ticket)
        {
            await channel.SyncPermissionsAsync();
            await channel.ModifyAsync(x => x.Name = $"closed_{ticket.Id:x8}");

            ticket.Status = TicketStatus.Closed;

            var embed = new EmbedBuilder
            {
                Description = $"Ticket closed by {_client.CurrentUser.Mention}",
                Color = Ticket.Color
            };
            var components = new ComponentBuilder().WithButton("Re-Open",
                _interaction.GetComponentCommandInfo<Ticket>(nameof(Ticket.ReOpen)).GetCustomId(),
                emote: new Emoji("🔓"));
            await channel.SendMessageAsync(embed: embed.Build(), component: components.Build());
        }

        private async Task DeleteTicket(SocketTextChannel channel, GuildAccount account, Core.Accounts.Ticket ticket)
        {
            await LogMessages(channel, ticket);

            await channel.DeleteAsync();
            account.Moderation.Helper.Tickets.Remove(channel.Id);
        }

        private async Task LogMessages(SocketTextChannel channel, Core.Accounts.Ticket ticket)
        {
            List<IMessage> messages = new();

            var msgs = await channel.GetMessagesAsync().FlattenAsync();
            while (msgs != null)
            {
                var collection = msgs as IMessage[] ?? msgs.ToArray();

                messages.AddRange(collection);
                var last = collection.OrderBy(x => x.Timestamp).FirstOrDefault();
                msgs = await channel.GetMessagesAsync(last, Direction.Before).FlattenAsync();
            }

            messages = messages.OrderBy(x => x.Timestamp).ToList();
            var file = File.CreateText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                $"{channel.Guild.Id}_ticket_{ticket.Id:x8}.txt"));
            foreach (var message in messages)
                await file.WriteLineAsync(
                    $"[{message.Timestamp}] [{message.Author}] [{string.Join(" // ", message.Attachments.Select(x => x.ProxyUrl))}] {message.Content}");
        }

        public void Dispose() => GC.SuppressFinalize(this);
    }
}
