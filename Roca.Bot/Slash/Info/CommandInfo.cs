using Discord.WebSocket;
using Roca.Bot.Slash.Builder;
using Roca.Core;
using Roca.Core.Translation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Roca.Bot.Slash.Info
{
    public class CommandInfo
    {
        private Func<RocaContext, object[], IServiceProvider, Task> Callback { get; }
        private Rocalizer Localizer { get; }

        public ModuleInfo Module { get; }
        public string Name { get; }
        public string Description { get; }

        public IReadOnlyCollection<ParameterInfo> Parameters { get; }

        internal CommandInfo(CommandBuilder builder, ModuleInfo module)
        {
            Module = module;
            Name = builder.Name!;
            Description = builder.Description!;
            Parameters = builder.Parameters.Select(x => x.Build(this)).ToArray();
            Callback = builder.Callback!;

            Localizer = GetType().GetLocalizer();
        }

        public async Task ExecuteAsync(SocketSlashCommand command, IEnumerable<SocketSlashCommandDataOption> opts,DiscordShardedClient client, IServiceProvider provider)
        {
            await command.DeferAsync().ConfigureAwait(false);
            try
            {
                RocaContext ctx = new(client, command);

                if (ctx.Guild == null || ctx.Member == null)
                {
                    await command.FollowupAsync(Localizer[CultureInfo.GetCultureInfo("en-US"), "only_guild"]).ConfigureAwait(false);
                    return;
                }

                var args = await GetArguments(ctx, opts, provider).ConfigureAwait(false);

                await Callback(ctx, args, provider).ConfigureAwait(false);
            }
            catch (Exception)
            {
                
            }
        }

        //TODO Better opts/args parsing
        private async Task<object[]> GetArguments(RocaContext context, IEnumerable<SocketSlashCommandDataOption> opts, IServiceProvider provider)
        {
            object[] args = new object[Parameters.Count];
            var array = opts.ToArray();

            int i = 0;
            foreach (var parameter in Parameters)
            {
                if (i > array.Length)
                    break;
                args[i] = await parameter.ParseAsync(context, array[i++], provider).ConfigureAwait(false);
            }

            if (i != Parameters.Count)
                throw new ArgumentOutOfRangeException(nameof(opts));

            return args;
        }
    }
}
