using Discord;
using Roca.Core.Interfaces;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;

namespace Roca.Bot.Modules.Audio.Handlers
{
    public class VictoriaHandler : IHandler
    {
        private bool _enabled;
        private readonly LavaNode _lava;

        public VictoriaHandler(LavaNode lava) => _lava = lava;

        public async Task Enable()
        {
            if (_enabled)
                return;

            _lava.OnLog += Log;
            _lava.OnTrackEnded += TrackEnded;
        }

        public async Task Disable()
        {
            if (!_enabled)
                return;

            _lava.OnLog -= Log;
        }

        private async Task Log(LogMessage arg)
        {
            if (arg.Severity == LogSeverity.Error || arg.Severity == LogSeverity.Critical)
                throw arg.Exception;
        }

        private async Task TrackEnded(TrackEndedEventArgs arg)
        {
            if (arg.Reason != TrackEndReason.Finished && arg.Reason != TrackEndReason.LoadFailed)
                return;

            if (!arg.Player.Queue.TryDequeue(out var track))
            {
                await _lava.LeaveAsync(arg.Player.VoiceChannel).ConfigureAwait(false);
                return;
            }

            await arg.Player.PlayAsync(track).ConfigureAwait(false);
        }

        public void Dispose() => GC.SuppressFinalize(this);
    }
}
