using System;
using System.Runtime.CompilerServices;

namespace Roca.Bot.Slash.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RocaCommandAttribute : Attribute
    {
        public string Name { get; }
        public RocaCommandAttribute([CallerMemberName] string name = "") => Name = name;
    }
}
