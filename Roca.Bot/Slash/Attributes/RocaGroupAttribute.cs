using System;
using System.Runtime.CompilerServices;

namespace Roca.Bot.Slash.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RocaGroupAttribute : Attribute
    {
        public string Name { get; }
        public RocaGroupAttribute([CallerMemberName] string name = "") : base() => Name = name;
    }
}
