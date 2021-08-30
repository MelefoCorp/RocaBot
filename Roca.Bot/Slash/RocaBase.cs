using System;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Roca.Core.Interfaces;
using Roca.Core.Translation;

namespace Roca.Bot.Slash
{
    public class RocaBase<T> : RocaBase where T : IService
    {
        public T Service { get; internal set; }
    }

    public class RocaBase : IDisposable
    {
        public Rocalizer Localizer { get; internal set; }
        public RocaContext Context { get; internal set; }

        public async Task<RestFollowupMessage> ReplyAsync(string? text = null, Embed[]? embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions? allowedMentions = null, RequestOptions? options = null, MessageComponent? component = null, Embed? embed = null)
            => await Context.RespondAsync(text, embeds, isTTS, ephemeral, allowedMentions, options, component, embed).ConfigureAwait(false);

        public void Dispose() => GC.SuppressFinalize(this);
    }
}
