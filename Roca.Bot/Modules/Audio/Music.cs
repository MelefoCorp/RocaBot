using System;
using System.Linq;
using Roca.Bot.Modules.Audio.Services;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Victoria;
using Victoria.Responses.Search;
using Discord.Interactions;
using Victoria.Enums;
using Roca.Bot.Commands;

namespace Roca.Bot.Modules.Audio
{
    [Group("music", "A list of music commands")]
    public class Music : RocaBase<MusicService>
    {
        private static readonly Regex Ytb = new(@"^(?:https?:)?(?:\/\/)?(?:youtu\.be\/|(?:www\.|m\.)?youtube\.com\/(?:watch|v|embed)(?:\.php)?(?:\?.*v=|\/))([a-zA-Z0-9_-]{7,15})(?:[\?&][a-zA-Z0-9_-]+=[a-zA-Z0-9_-]+)*(?:[&\/\#].*)?$");
        private static readonly Regex List = new(@"^(?:https?:)?(?:\/\/)?(?:youtu\.be\/|(?:www\.|m\.)?youtube\.com\/(?:playlist|list|embed)(?:\.php)?(?:\?.*list=|\/))([a-zA-Z0-9\-_]+)$");

        [SlashCommand("play", "bonjour")]
        public async Task Play(string search)
        {
            if (!Service.Lava.IsConnected)
            {
                await RespondAsync(Localizer[Context.GuildAccount!.Language, "unavailable"], ephemeral: true).ConfigureAwait(false);
                return;
            }

            var result = await Service.Lava.SearchAsync((Ytb.IsMatch(search) || List.IsMatch(search)) ? SearchType.Direct : SearchType.YouTube, search);
            if (result.Status is SearchStatus.NoMatches or SearchStatus.LoadFailed)
            {
                await RespondAsync(Localizer[Context.GuildAccount!.Language, "not_found"], ephemeral: true).ConfigureAwait(false);
                return;
            }

            var joinable = await Service.TryJoinAsync(Context).ConfigureAwait(false);
            if (!joinable.IsSuccess)
            {
                await RespondAsync(Localizer[Context.GuildAccount!.Language, "not_joinable"], ephemeral: true).ConfigureAwait(false);
                return;
            }

            if (result.Status == SearchStatus.PlaylistLoaded)
                joinable.Player!.Queue.Enqueue(result.Tracks);
            else
                joinable.Player!.Queue.Enqueue(result.Tracks.First());

            if (joinable.Player.PlayerState != PlayerState.Playing && joinable.Player.Queue.TryDequeue(out var play))
                await joinable.Player.PlayAsync(play).ConfigureAwait(false);

            await RespondAsync(Localizer[Context.GuildAccount!.Language, "added_queue"]).ConfigureAwait(false);
        }

        [SlashCommand("pause", "bonjour")]
        public async Task Pause()
        {
            if (!Service.Lava.IsConnected)
            {
                await RespondAsync(Localizer[Context.GuildAccount!.Language, "unavailable"], ephemeral: true).ConfigureAwait(false);
                return;
            }

            if (!Service.TryGetPlayer(Context, out var player))
            {
                await RespondAsync(Localizer[Context.GuildAccount!.Language, "no_player"], ephemeral: true).ConfigureAwait(false);
                return;
            }

            switch (player.PlayerState)
            {
                case PlayerState.Playing:
                    await player.PauseAsync().ConfigureAwait(false);
                    await RespondAsync(Localizer[Context.GuildAccount!.Language, "paused"]).ConfigureAwait(false);
                    break;
                case PlayerState.Paused:
                    await player.ResumeAsync().ConfigureAwait(false);
                    await RespondAsync(Localizer[Context.GuildAccount!.Language, "resumed"]).ConfigureAwait(false);
                    break;
            }
        }

        [SlashCommand("skip", "bonjour")]
        public async Task Skip()
        {
            if (!Service.Lava.IsConnected)
            {
                await RespondAsync(Localizer[Context.GuildAccount!.Language, "unavailable"], ephemeral: true).ConfigureAwait(false);
                return;
            }

            if (!Service.TryGetPlayer(Context, out var player) || player.PlayerState != PlayerState.Playing)
            {
                await RespondAsync(Localizer[Context.GuildAccount!.Language, "no_player"], ephemeral: true).ConfigureAwait(false);
                return;
            }

            if (player.Queue.Count > 0)
                await player.SkipAsync().ConfigureAwait(false);
            else
            {
                await player.StopAsync().ConfigureAwait(false);
                await Service.Lava.LeaveAsync(Context.Member!.VoiceChannel).ConfigureAwait(false);
            }
            await RespondAsync(Localizer[Context.GuildAccount!.Language, "skipped"]).ConfigureAwait(false);
        }

        [SlashCommand("volume", "bonjour")]
        public async Task Volume(long volume)
        {
            if (!Service.Lava.IsConnected)
            {
                await RespondAsync(Localizer[Context.GuildAccount!.Language, "unavailable"], ephemeral: true).ConfigureAwait(false);
                return;
            }

            if (volume is < 0 or > 100)
            {
                await RespondAsync(Localizer[Context.GuildAccount!.Language, "between_0_100"], ephemeral: true).ConfigureAwait(false);
                return;
            }

            if (!Service.TryGetPlayer(Context, out var player))
            {
                await RespondAsync(Localizer[Context.GuildAccount!.Language, "no_player"], ephemeral: true).ConfigureAwait(false);
                return;
            }

            await player.UpdateVolumeAsync((ushort)volume).ConfigureAwait(false);
            await RespondAsync(Localizer[Context.GuildAccount!.Language, "volume_updated"]).ConfigureAwait(false);
        }

        [SlashCommand("lyrics", "bonjour")]
        public async Task Lyrics()
        {
            if (!Service.Lava.IsConnected)
            {
                await RespondAsync(Localizer[Context.GuildAccount!.Language, "unavailable"], ephemeral: true).ConfigureAwait(false);
                return;
            }

            if (!Service.TryGetPlayer(Context, out var player))
            {
                await RespondAsync(Localizer[Context.GuildAccount!.Language, "no_player"], ephemeral: true).ConfigureAwait(false);
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                await RespondAsync(Localizer[Context.GuildAccount!.Language, "no_track"], ephemeral: true).ConfigureAwait(false);
                return;
            }

            string lyrics = await player.Track.FetchLyricsFromGeniusAsync().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(lyrics))
                lyrics = await player.Track.FetchLyricsFromOvhAsync().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(lyrics))
            {
                await RespondAsync(Localizer[Context.GuildAccount!.Language, "no_lyrics"], ephemeral: true).ConfigureAwait(false);
                return;
            }

            await RespondAsync(lyrics).ConfigureAwait(false);
        }

        [SlashCommand("queue", "bonjour")]
        public async Task Queue()
        {
            if (!Service.Lava.IsConnected)
            {
                await RespondAsync(Localizer[Context.GuildAccount!.Language, "unavailable"], ephemeral: true).ConfigureAwait(false);
                return;
            }

            if (!Service.TryGetPlayer(Context, out var player))
            {
                await RespondAsync(Localizer[Context.GuildAccount!.Language, "no_player"], ephemeral: true).ConfigureAwait(false);
                return;
            }

            var fields = player.Queue.Select((track, i) => $"#{i + 1} {track.Title}\n{(track.IsStream ? "Livestream" : track.Duration.ToString(@"hh\:mm\:ss"))} {track.Author}\n[Link]({track.Url})\n");
            string final = string.Join("\n", fields);

            if (string.IsNullOrWhiteSpace(final))
            {
                await RespondAsync(Localizer[Context.GuildAccount!.Language, "empty_queue"], ephemeral: true).ConfigureAwait(false);
                return;
            }

            while (final.Length > 2000)
                final = final[..final.LastIndexOf('\n')];
            await RespondAsync(final).ConfigureAwait(false);
        }

        [SlashCommand("track", "bonjour")]
        public async Task Track()
        {
            if (!Service.Lava.IsConnected)
            {
                await RespondAsync(Localizer[Context.GuildAccount!.Language, "unavailable"], ephemeral: true).ConfigureAwait(false);
                return;
            }

            if (!Service.TryGetPlayer(Context, out var player))
            {
                await RespondAsync(Localizer[Context.GuildAccount!.Language, "no_player"], ephemeral: true).ConfigureAwait(false);
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                await RespondAsync(Localizer[Context.GuildAccount!.Language, "no_track"], ephemeral: true).ConfigureAwait(false);
                return;
            }

            var percent = (double)player.Track.Position.Ticks / player.Track.Duration.Ticks;
            char[] position =
                {'-', '-', '-', '-', '-', '-', '-', '-', '-', '-', '-', '-', '-', '-', '-', '-', '-', '-', '-', '-'};
            position[Convert.ToInt32(percent * position.Length)] = '⏺';

            string final = $"{player.Track.Title}\n{player.Track.Author}\n[Link]({player.Track.Url})\n{player.Track.Position:hh\\:mm\\:ss}/{player.Track.Duration:hh\\:mm\\:ss} {Convert.ToInt32(percent * 100)}%\n⏯️ 🔁 ⏮️ {new string(position)} ⏭️ ⏹️ 🔀";

            await RespondAsync(final).ConfigureAwait(false);
        }
    }
}
