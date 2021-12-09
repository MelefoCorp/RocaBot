﻿using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Roca.Core.Interfaces;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Roca.Bot.Commands
{
    public class CommandHandler : IHandler
    {
        private readonly DiscordShardedClient _client;
        private readonly InteractionService _commands;
        private readonly IServiceProvider _services;
        private bool _enabled;

        public CommandHandler(DiscordShardedClient client, InteractionService commands, IServiceProvider services)
        {
            _client = client;
            _commands = commands;
            _services = services;
        }

        public async Task Enable()
        {
            if (_enabled)
                return;

            _commands.SlashCommandExecuted += SlashCommandExecuted;
            _commands.ContextCommandExecuted += ContextCommandExecuted;
            _commands.ComponentCommandExecuted += ComponentCommandExecuted;

            _client.InteractionCreated += HandleInteraction;

            await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);
        }

        public async Task Disable()
        {
            if (!_enabled)
                return;

            _client.InteractionCreated -= HandleInteraction;

            _commands.ComponentCommandExecuted -= ComponentCommandExecuted;
            _commands.ContextCommandExecuted -= ContextCommandExecuted;
            _commands.SlashCommandExecuted -= SlashCommandExecuted;

            foreach (var module in _commands.Modules)
                await _commands.RemoveModuleAsync(module);

            _enabled = false;
        }

        private async Task HandleInteraction(SocketInteraction arg)
        {
            try
            {
                var ctx = new RocaContext(_client, arg);
                await _commands.ExecuteCommandAsync(ctx, _services);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                if (arg.Type == InteractionType.ApplicationCommand)
                    await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }

        private Task ComponentCommandExecuted(ComponentCommandInfo arg1, Discord.IInteractionContext arg2, IResult arg3)
        {
            if (!arg3.IsSuccess)
            {
                switch (arg3.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        break;
                    case InteractionCommandError.UnknownCommand:
                        break;
                    case InteractionCommandError.BadArgs:
                        break;
                    case InteractionCommandError.Exception:
                        break;
                    case InteractionCommandError.Unsuccessful:
                        break;
                    default:
                        break;
                }
            }

            return Task.CompletedTask;
        }

        private Task ContextCommandExecuted(ContextCommandInfo arg1, Discord.IInteractionContext arg2, IResult arg3)
        {
            if (!arg3.IsSuccess)
            {
                switch (arg3.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        break;
                    case InteractionCommandError.UnknownCommand:
                        break;
                    case InteractionCommandError.BadArgs:
                        break;
                    case InteractionCommandError.Exception:
                        break;
                    case InteractionCommandError.Unsuccessful:
                        break;
                    default:
                        break;
                }
            }

            return Task.CompletedTask;
        }

        private Task SlashCommandExecuted(SlashCommandInfo arg1, Discord.IInteractionContext arg2, IResult arg3)
        {
            if (!arg3.IsSuccess)
            {
                switch (arg3.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        break;
                    case InteractionCommandError.UnknownCommand:
                        break;
                    case InteractionCommandError.BadArgs:
                        break;
                    case InteractionCommandError.Exception:
                        break;
                    case InteractionCommandError.Unsuccessful:
                        break;
                    default:
                        break;
                }
            }

            return Task.CompletedTask;
        }

        public void Dispose() => GC.SuppressFinalize(this);
    }
}