using System;
using System.Runtime.CompilerServices;

namespace Roca.Bot.Slash.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RocaModuleAttribute : Attribute
    {
        public string Name { get; }

        public RocaModuleAttribute([CallerMemberName] string name = "") => Name = name;
    }
}
