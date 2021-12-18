using Discord;
using Discord.Interactions;
using Roca.Bot.Commands;
using Roca.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Roca.Bot.Attributes
{
    public enum Role
    {
        Helper,
        Mod
    }

    public class RequireRoleAttribute : PreconditionAttribute
    {
        private readonly Role _role;

        public RequireRoleAttribute(Role role)
            => _role = role;

        private static PreconditionResult CheckHelper(RocaContext ctx)
        {
            if (!ctx.GuildAccount.Moderation.Helper.Role.HasValue)
                return PreconditionResult.FromError("");
           var role = ctx.Guild.GetRole(ctx.GuildAccount.Moderation.Helper.Role.Value);
            if (role == null)
                return PreconditionResult.FromError("");
            
            return ctx.Member?.Hierarchy >= role.Position
                ? PreconditionResult.FromSuccess()
                : PreconditionResult.FromError("");
        }

        private static PreconditionResult CheckMod(RocaContext ctx)
        {
            if (!ctx.GuildAccount.Moderation.Role.HasValue)
                return PreconditionResult.FromError("");
            var role = ctx.Guild.GetRole(ctx.GuildAccount.Moderation.Role.Value);
            if (role == null)
                return PreconditionResult.FromError("");

            return ctx.Member?.Hierarchy >= role.Position
                ? PreconditionResult.FromSuccess()
                : PreconditionResult.FromError("");
        }

        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo _, IServiceProvider __)
        {
            if (context is not RocaContext ctx)
                return Task.FromResult(PreconditionResult.FromError(""));

            return _role switch
            {
                Role.Helper => Task.FromResult(CheckHelper(ctx)),
                Role.Mod => Task.FromResult(CheckMod(ctx)),
                _ => Task.FromResult(PreconditionResult.FromError(""))
            };
        }
    }
}
