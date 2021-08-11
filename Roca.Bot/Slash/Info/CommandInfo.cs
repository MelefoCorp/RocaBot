using Discord.WebSocket;
using Roca.Bot.Slash.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Roca.Bot.Slash.Info
{
    public class CommandInfo
    {
        private Func<RocaContext, object[], IServiceProvider, Task> _callback { get; }
        
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
            _callback = builder.Callback!;
        }

        public async Task ExecuteAsync(SocketSlashCommand command, IEnumerable<SocketSlashCommandDataOption> opts,DiscordShardedClient client, IServiceProvider provider)
        {
            await command.DeferAsync().ConfigureAwait(false);
            try
            {
                RocaContext ctx = new(client, command);
                var args = await GetArguments(ctx, opts, provider).ConfigureAwait(false);

                await _callback(ctx, args, provider).ConfigureAwait(false);
            }
            catch (Exception)
            {
                
            }
        }

        //TODO Better opts/args parsing
        private async Task<object[]> GetArguments(RocaContext context, IEnumerable<SocketSlashCommandDataOption> opts, IServiceProvider provider)
        {
            if (opts == null)
                return Array.Empty<object>();

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
