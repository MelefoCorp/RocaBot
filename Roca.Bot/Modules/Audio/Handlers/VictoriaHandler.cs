using System;
using System.Threading.Tasks;
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

        public Task Enable()
        {
            if (_enabled)
                return Task.CompletedTask;
            
            _lava.OnLog += Log;
            _lava.OnTrackEnded += TrackEnded;
            _enabled = true;

            return Task.CompletedTask;
        }

        public Task Disable()
        {
            if (!_enabled)
                return Task.CompletedTask;

            _lava.OnLog -= Log;
            _enabled = false;

            return Task.CompletedTask;
        }

        private Task Log(LogMessage arg)
        {
            if (arg.Severity is LogSeverity.Error or LogSeverity.Critical)
            {

            }
            
            return Task.CompletedTask;
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
