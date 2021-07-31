using DSharpPlus;
using DSharpPlus.EventArgs;
using System.Threading.Tasks;

namespace Roca.Bot.Slash
{
public class SlashCommandsExtension : BaseExtension
    {
        private DiscordClient _client;

        protected override void Setup(DiscordClient client)
        {
            _client = client;

            _client.InteractionCreated += HandleInteractionAsync;
        }

        private async Task HandleInteractionAsync(DiscordClient sender, InteractionCreateEventArgs e)
        {
            if (e.Handled)
                return;

            switch (e.Interaction.Type)
            {
                case InteractionType.ApplicationCommand:
                    await HandleCommandAsync().ConfigureAwait(false);
                    return;
                case InteractionType.Component:
                    await HandleComponentAsync().ConfigureAwait(false);
                    return;
            }

        }

        private async Task HandleCommandAsync()
        {
        }

        private async Task HandleComponentAsync()
        {

        }
    }
}
