using System;
using System.Threading.Tasks;
using Discord;
using Roca.Core.Interfaces;
using Victoria;

namespace Roca.Bot.Modules.Audio.Services
{
    public class MusicService : IService
    {
        private bool _enabled;

        public readonly LavaNode Lava;

        public MusicService(LavaNode lava) => Lava = lava;

        public async Task Enable()
        {
            if (_enabled)
                return;
            
            await Lava.ConnectAsync().ConfigureAwait(false);
            _enabled = true;
        }
        public async Task Disable()
        {
            if (!_enabled)
                return;
            
            await Lava.DisconnectAsync().ConfigureAwait(false);
            _enabled = false;
        }

        public async Task<(bool IsSuccess, LavaPlayer? Player)> TryJoinAsync(RocaContext context, bool move = false)
        {
            if (context.Member?.VoiceChannel == null)
                return (false, null);

            if (Lava.TryGetPlayer(context.Guild, out var player))
            {
                if (move)
                    await Lava.MoveChannelAsync(context.Member.VoiceChannel).ConfigureAwait(false);
                else if (context.Member!.VoiceChannel.Id != player.VoiceChannel.Id)
                    return (false, null);
            }
            else
                player = await Lava.JoinAsync(context.Member.VoiceChannel, context.Channel as ITextChannel).ConfigureAwait(false);

            if (player.VoiceChannel is IStageChannel stage)
                await stage.BecomeSpeakerAsync().ConfigureAwait(false);
            return (true, player);
        }

        public bool TryGetPlayer(RocaContext context, out LavaPlayer player)
        {
            if (!Lava.TryGetPlayer(context.Guild, out player))
                return false;
            
            return player.VoiceChannel.Id == context.Member!.VoiceChannel.Id;
        }

        public void Dispose() => GC.SuppressFinalize(this);
    }
}
