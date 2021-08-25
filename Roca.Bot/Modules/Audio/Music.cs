using Discord;
using Roca.Bot.Modules.Audio.Services;
using Roca.Bot.Slash;
using Roca.Bot.Slash.Attributes;
using System.Text.RegularExpressions;
using Victoria;
using Victoria.Enums;
using Victoria.Responses.Search;

namespace Roca.Bot.Modules.Audio
{
    [RocaModule("music")]
    public class Music : RocaBase<MusicService>
    {
        private static Regex _ytb = new(@"^(?:https?:)?(?:\/\/)?(?:youtu\.be\/|(?:www\.|m\.)?youtube\.com\/(?:watch|v|embed)(?:\.php)?(?:\?.*v=|\/))([a-zA-Z0-9_-]{7,15})(?:[\?&][a-zA-Z0-9_-]+=[a-zA-Z0-9_-]+)*(?:[&\/\#].*)?$");

        [RocaCommand]
        public async Task Play(string search)
        {
            if (!Service.Lava.IsConnected)
                return;

            SearchResponse result = await Service.Lava.SearchAsync(_ytb.IsMatch(search) ? SearchType.Direct : SearchType.YouTube, search);
            if (result.Status == SearchStatus.NoMatches || result.Status == SearchStatus.LoadFailed)
                return;

            //direct == link ytb or platform live
            //autrement chercher sur les platformes

            var joinable = await Service.TryJoinAsync(Context).ConfigureAwait(false);
            if (!joinable.IsSuccess)
                return;

            joinable.Player!.Queue.Enqueue(result.Tracks.First());

            if (joinable.Player.PlayerState != PlayerState.Playing && joinable.Player.Queue.TryDequeue(out var play))
                await joinable.Player.PlayAsync(play).ConfigureAwait(false);
        }

        [RocaCommand]
        public async Task Pause()
        {
            if (!Service.Lava.IsConnected)
                return;

            if (!Service.TryGetPlayer(Context, out var player))
                return;

            if (player.PlayerState == PlayerState.Playing)
                await player.PauseAsync().ConfigureAwait(false);
            else if (player.PlayerState == PlayerState.Paused)
                await player.ResumeAsync().ConfigureAwait(false);
        }

        [RocaCommand]
        public async Task Skip()
        {
            if (!Service.Lava.IsConnected)
                return;

            if (!Service.TryGetPlayer(Context, out var player))
                return;
            await player.SkipAsync().ConfigureAwait(false);
        }

        [RocaCommand]
        public async Task Volume(long volume)
        {
            if (!Service.Lava.IsConnected)
                return;

            if (volume < 0 || volume > 100)
                return;

            if (!Service.TryGetPlayer(Context, out var player))
                return;
            await player.UpdateVolumeAsync((ushort)volume).ConfigureAwait(false);
        }

        [RocaCommand]
        public async Task Lyrics()
        {
            if (!Service.Lava.IsConnected)
                return;

            if (!Service.TryGetPlayer(Context, out var player))
                return;
            if (player.PlayerState != PlayerState.Playing)
                return;

            string lyrics = await player.Track.FetchLyricsFromGeniusAsync().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(lyrics))
                lyrics = await player.Track.FetchLyricsFromOvhAsync().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(lyrics))
                return;
            await ReplyAsync(lyrics).ConfigureAwait(false);
        }

        [RocaCommand]
        public async Task Queue()
        {
            if (!Service.Lava.IsConnected)
                return;

            if (!Service.TryGetPlayer(Context, out var player))
                return;

            var fields = player.Queue.ToArray().Select((track, i) => $"#{i + 1} {track.Title}\n{(track.IsStream ? "Livestream" : track.Duration.ToString(@"hh\:mm\:ss"))} {track.Author}\n[Link]({player.Track.Url})\n");
            string final = string.Join("\n", fields);
            await ReplyAsync().ConfigureAwait(false);
        }

        [RocaCommand]
        public async Task Track()
        {
            if (!Service.Lava.IsConnected)
                return;

            if (!Service.TryGetPlayer(Context, out var player))
                return;

            var percent = (double)player.Track.Position.Ticks / player.Track.Duration.Ticks;
            char[] position =
                {'-', '-', '-', '-', '-', '-', '-', '-', '-', '-', '-', '-', '-', '-', '-', '-', '-', '-', '-', '-'};
            position[Convert.ToInt32(percent * 20)] = '⏺';

            string final = $"{player.Track.Title}\n{player.Track.Author}\n[Link]({player.Track.Url})\n{player.Track.Position:hh\\:mm\\:ss}/{player.Track.Duration:hh\\:mm\\:ss} {Convert.ToInt32(percent * 100)}\n⏯️ 🔁 ⏮️ {new string(position)} ⏭️ ⏹️ 🔀";

            await ReplyAsync(final).ConfigureAwait(false);
        }
    }
}
