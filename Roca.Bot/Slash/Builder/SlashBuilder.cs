using Roca.Bot.Slash.Attributes;
using Roca.Bot.Slash.Builder;
using Roca.Bot.Slash.Info;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Roca.Bot.Slash.Service
{
    internal static class SlashBuilder
    {
        private static readonly TypeInfo _moduleType = typeof(RocaBase).GetTypeInfo();
        private static readonly Type _task = typeof(Task);
        private static readonly Type _void = typeof(void);

        public static bool IsModuleCandidate(this TypeInfo type)
        {
            if (type.GetCustomAttribute<CompilerGeneratedAttribute>() != null)
                return false;

            if (!type.IsAssignableTo(_moduleType))
                return false;
            if (!type.DeclaredMethods.Any(x => x.GetCustomAttribute<RocaCommandAttribute>() != null) && !type.DeclaredNestedTypes.Any(IsGroupCandidate))
                return false;

            return type.IsClass && !type.ContainsGenericParameters && !type.IsAbstract;
        }

        public static bool IsGroupCandidate(this TypeInfo type)
        {
            if (type.GetCustomAttribute<CompilerGeneratedAttribute>() != null)
                return false;

            if (!type.IsAssignableTo(_moduleType))
                return false;
            if (!type.DeclaredMethods.Any(x => x.GetCustomAttribute<RocaCommandAttribute>() != null))
                return false;

            return type.IsClass && !type.ContainsGenericParameters && !type.IsAbstract;
        }

        public static bool IsCommandCandidate(this MethodInfo method)
        {
            if (method.GetCustomAttribute<CompilerGeneratedAttribute>() != null)
                return false;

            if (method.ReturnType != _task && method.ReturnType != _void)
                return false;
            if (method.GetCustomAttribute<RocaCommandAttribute>() == null)
                return false;

            return !method.IsStatic && !method.IsAbstract && !method.IsGenericMethod;
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
                if (type.DeclaringType == null)
                    list.Add(type);
            }

            return list;
        }

        public static IReadOnlyDictionary<Type, ModuleInfo> BuildModules(IEnumerable<TypeInfo> types, SlashService service, IServiceProvider services)
        {
            var list = new Dictionary<Type, ModuleInfo>();

            foreach (var type in types)
            {
                var builder = new ModuleBuilder(service, null);

                BuildModule(builder, type, service, services);
                BuildGroups(builder, type.DeclaredNestedTypes, service, services);

                list[type.AsType()] = builder.Build();
            }
            return list;
        }

        public static void BuildGroups(ModuleBuilder builder, IEnumerable<TypeInfo> types, SlashService service, IServiceProvider services)
        {
            foreach (var type in types.Where(IsGroupCandidate))
                builder.AddModule(x =>
                {
                    BuildModule(x, type, service, services);
                    BuildGroups(x, type.DeclaredNestedTypes, service, services);
                });
        }

        private static void BuildModule(ModuleBuilder builder, TypeInfo type, SlashService service, IServiceProvider services)
        {
            foreach (var attribute in type.GetCustomAttributes())
                switch (attribute)
                {
                    case RocaModuleAttribute module:
                        if (!string.IsNullOrWhiteSpace(builder.Name) && builder.IsGroup)
                            throw new RocaBuilderException("A class cannot be at the same time a module & a group");
                        if (string.IsNullOrWhiteSpace(module.Name))
                            throw new RocaBuilderException("Module must have a name");
                        builder.Name = module.Name;
                        break;
                    case RocaGroupAttribute group:
                        if (!string.IsNullOrWhiteSpace(builder.Name) && !builder.IsGroup)
                            throw new RocaBuilderException("A class cannot be at the same time a module & a group");
                        if (string.IsNullOrWhiteSpace(group.Name))
                            throw new RocaBuilderException("Group must have a name");
                        builder.Name = group.Name;
                        builder.IsGroup = true;
                        break;
                }

            foreach (var command in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(IsCommandCandidate))
                builder.AddCommand(x => BuildCommand(x, type, command, service, services));
        }

        private static void BuildCommand(CommandBuilder builder, TypeInfo type, MethodInfo method, SlashService service, IServiceProvider services)
        {
            foreach (var attribute in method.GetCustomAttributes())
                switch (attribute)
                {
                    case RocaCommandAttribute command:
                        builder.Name = command.Name;
                        break;
                }

            if (string.IsNullOrWhiteSpace(builder.Name))
                throw new RocaBuilderException("Command must have a name");

            foreach (var parameter in method.GetParameters())
                builder.AddParameter(x => BuildParameter(x, parameter, service, services));

        }

        private static void BuildParameter(ParameterBuilder builder, ParameterInfo parameter, SlashService service, IServiceProvider services)
        {

            builder.Name = parameter.Name;
            builder.IsOptional = parameter.IsOptional;
            builder.DefaultValue = parameter.DefaultValue;
            builder.Type = parameter.ParameterType;

            //TODO add type reader/parser
        }
    }

    internal class RocaBuilderException : Exception
    {
        public RocaBuilderException(string? message = null) : base(message)
        {

        }
    }

}
