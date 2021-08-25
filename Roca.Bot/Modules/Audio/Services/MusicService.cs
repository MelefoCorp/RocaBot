using Discord;
using Roca.Bot.Slash;
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
        }
        public async Task Disable()
        {
            if (!_enabled)
                return;
            await Lava.DisconnectAsync().ConfigureAwait(false);
        }

        public async Task<(bool IsSuccess, LavaPlayer? Player)> TryJoinAsync(RocaContext context, bool move = false)
        {
            if (context.Member == null || context.Member.VoiceChannel == null)
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

            return (true, player);
        }

        public bool TryGetPlayer(RocaContext context, out LavaPlayer player)
        {
            if (!Lava.TryGetPlayer(context.Guild, out player))
                return false;

            if (player.VoiceChannel.Id != context.Member!.VoiceChannel.Id)
                return false;

            return true;
        }

        public void Dispose() => GC.SuppressFinalize(this);
    }
}
