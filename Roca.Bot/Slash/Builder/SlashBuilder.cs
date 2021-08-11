using Roca.Bot.Slash.Attributes;
using Roca.Bot.Slash.Builder;
using Roca.Bot.Slash.Readers;
using Roca.Core;
using Roca.Core.Interfaces;
using System;
using System.Collections.Concurrent;
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
            if (!type.DeclaredMethods.Any(IsCommandCandidate) && !type.DeclaredNestedTypes.Any(IsGroupCandidate))
                return false;

            return type.IsClass && !type.ContainsGenericParameters && !type.IsAbstract;
        }

        public static bool IsGroupCandidate(this TypeInfo type) =>
            type.IsModuleCandidate() && type.GetCustomAttribute<RocaModuleAttribute>() != null;

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

        public static IReadOnlyDictionary<Type, Info.ModuleInfo> BuildModules(IEnumerable<TypeInfo> types, SlashService service, IServiceProvider services)
        {
            var list = new Dictionary<Type, Info.ModuleInfo>();

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
                        builder.Name = module.Name.ToLowerInvariant();
                        break;
                }

            if (string.IsNullOrWhiteSpace(builder.Name))
                throw new RocaBuilderException("Module must have a name");

            builder.Description = type.GetLocalizer()[$"{type.Name}_desc"];

            foreach (var command in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(IsCommandCandidate))
                builder.AddCommand(x => BuildCommand(x, type, command, service));
        }

        private static void BuildCommand(CommandBuilder builder, TypeInfo type, MethodInfo method, SlashService service)
        {
            foreach (var attribute in method.GetCustomAttributes())
                switch (attribute)
                {
                    case RocaCommandAttribute command:
                        builder.Name = command.Name.ToLowerInvariant();
                        break;
                }

            if (string.IsNullOrWhiteSpace(builder.Name))
                throw new RocaBuilderException("Command must have a name");

            builder.Description = type.GetLocalizer()[$"{builder.Name}_desc"];

            foreach (var parameter in method.GetParameters())
                builder.AddParameter(x => BuildParameter(x, parameter, builder, service));

            builder.Callback = async (RocaContext context, object[] args, IServiceProvider provider) =>
            {
                var instance = CreateInstance(type, provider);
                instance.Context = context;

                if (method.Invoke(instance, args) is Task task)
                    await task.ConfigureAwait(false);

                instance.Dispose();
            };
        }

        private static RocaBase CreateInstance(TypeInfo type, IServiceProvider services)
        {
            var constructor = type.DeclaredConstructors.Single(x => !x.IsStatic);
            var parameters = constructor.GetParameters();
            var args = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
                args[i] = services.GetService(parameters[i].ParameterType)!;

            object instance = constructor.Invoke(args);

            foreach (var property in type.DeclaredProperties)
                property.SetValue(instance, services.GetService(property.PropertyType));

            return (RocaBase)instance;
        }

        private static void BuildParameter(ParameterBuilder builder, ParameterInfo parameter, CommandBuilder command, SlashService service)
        {
            builder.Name = parameter.Name!.ToLowerInvariant();
            builder.IsOptional = parameter.IsOptional;
            builder.DefaultValue = parameter.DefaultValue;
            builder.Type = parameter.ParameterType;

            var type = parameter.Member.ReflectedType!;
            builder.Description = type.GetLocalizer()[$"{command.Name}_{builder.Name}_desc"];

            if (!service.TypeReaders.TryGetValue(builder.Type, out var reader))
                throw new ArgumentException("A command doesn't contains a valid argument type");
            builder.TypeReader = reader;
        }
    }

    internal class RocaBuilderException : Exception
    {
        public RocaBuilderException(string? message = null) : base(message)
        {

        }
    }

}
