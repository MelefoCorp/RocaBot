using Discord;
using Discord.Rest;
using Roca.Core.Translation;

namespace Roca.Bot.Slash
{
    public class RocaBase : RocaBase<RocaContext> 
    {
    }

    public class RocaBase<T> : IDisposable where T : RocaContext
    {
        public RocaLocalizer Localizer { get; internal set; }
        public T Context { get; internal set; }

        public async Task<RestFollowupMessage> ReplyAsync(string? text = null, Embed[]? embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions? allowedMentions = null, RequestOptions? options = null, MessageComponent? component = null, Embed? embed = null)
            => await Context.RespondAsync(text, embeds, isTTS, ephemeral, allowedMentions, options, component, embed).ConfigureAwait(false);

        public void Dispose() => GC.SuppressFinalize(this);
    }
}
