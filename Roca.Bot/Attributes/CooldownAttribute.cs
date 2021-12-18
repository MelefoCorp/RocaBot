using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Roca.Bot.Attributes
{
    public enum Measure
    {
        Day,
        Hour,
        Minute,
        Second
    }

    public class CooldownAttribute : PreconditionAttribute
    {
        private readonly TimeSpan _period;
        private readonly Dictionary<(ulong User, ulong Guild), DateTime> _invokes = new();

        public CooldownAttribute(double period, Measure measure)
            => _period = measure switch
            {
                Measure.Day => TimeSpan.FromDays(period),
                Measure.Hour => TimeSpan.FromHours(period),
                Measure.Minute => TimeSpan.FromMinutes(period),
                Measure.Second => TimeSpan.FromSeconds(period),
                _ => throw new ArgumentException(nameof(measure))
            };


        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context,
            ICommandInfo commandInfo, IServiceProvider services)
        {
            if (!_invokes.ContainsKey((context.User.Id, context.Guild.Id)))
            {
                _invokes.Add((context.User.Id, context.Guild.Id), DateTime.UtcNow.Add(_period));
                return Task.FromResult(PreconditionResult.FromSuccess());
            }

            if (_invokes[(context.User.Id, context.Guild.Id)] > DateTime.UtcNow)
                return Task.FromResult(PreconditionResult.FromError(""));

            _invokes.Add((context.User.Id, context.Guild.Id), DateTime.UtcNow);
            return Task.FromResult(PreconditionResult.FromSuccess());

        }
    }
}