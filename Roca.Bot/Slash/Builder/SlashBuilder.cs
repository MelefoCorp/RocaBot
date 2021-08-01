using System.Reflection;
using System.Runtime.CompilerServices;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Roca.Bot.Slash.Info;
using System.Linq;
using Roca.Bot.Slash.Builder;
using Roca.Bot.Slash.Attributes;

namespace Roca.Bot.Slash.Service
{
    internal static class SlashBuilder
    {
        private static readonly TypeInfo _moduleType = typeof(RocaBase).GetTypeInfo();

        public static bool IsModuleCandidate(this TypeInfo type)
        {
            if (type.GetCustomAttribute<CompilerGeneratedAttribute>() != null)
                return false;

            if (!type.IsAssignableTo(_moduleType))
                return false;
            if (!type.DeclaredMethods.Any(x => x.GetCustomAttribute<RocaCommandAttribute>() != null))
                return false;

            return type.IsClass && !type.ContainsGenericParameters && !type.IsAbstract;
        }

        public static IEnumerable<TypeInfo> FindModules(Assembly assembly)
        {
            var list = new List<TypeInfo>();

            foreach (var type in assembly.DefinedTypes)
            {
                if (!type.IsPublic && !type.IsNestedPublic)
                    continue;
                if (!type.IsModuleCandidate())
                    continue;
                list.Add(type);
            }

            return list;
        }

        public static async Task<IReadOnlyDictionary<Type, ModuleInfo>> BuildModules(IEnumerable<TypeInfo> types, SlashService service, IServiceProvider services)
        {
            var list = new Dictionary<Type, ModuleInfo>();

            foreach (var type in types)
            {
                var builder = new ModuleBuilder(service, null);

                list[type.AsType()] = builder.Build();
            }

            return list;
        }
    }
}
