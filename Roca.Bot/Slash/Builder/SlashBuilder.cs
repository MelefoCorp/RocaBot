using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Roca.Bot.Slash.Attributes;
using Roca.Core;

namespace Roca.Bot.Slash.Builder
{
    internal static class SlashBuilder
    {
        private static readonly TypeInfo ModuleType = typeof(RocaBase).GetTypeInfo();
        private static readonly Type Task = typeof(Task);
        private static readonly Type Void = typeof(void);

        private static bool IsModuleCandidate(this TypeInfo type)
        {
            if (type.GetCustomAttribute<CompilerGeneratedAttribute>() != null)
                return false;

            if (!type.IsAssignableTo(ModuleType))
                return false;
            if (!type.DeclaredMethods.Any(IsCommandCandidate) && !type.DeclaredNestedTypes.Any(IsGroupCandidate))
                return false;

            return type.IsClass && !type.ContainsGenericParameters && !type.IsAbstract;
        }

        private static bool IsGroupCandidate(this TypeInfo type) =>
            type.IsModuleCandidate() && type.GetCustomAttribute<RocaModuleAttribute>() != null;

        private static bool IsCommandCandidate(this MethodInfo method)
        {
            if (method.GetCustomAttribute<CompilerGeneratedAttribute>() != null)
                return false;

            if (method.ReturnType != Task && method.ReturnType != Void)
                return false;
            if (method.GetCustomAttribute<RocaCommandAttribute>() == null)
                return false;

            return !method.IsStatic && !method.IsAbstract && !method.IsGenericMethod;
        }

        public static IEnumerable<TypeInfo> FindModules(Assembly assembly) => 
            from type in assembly.DefinedTypes where type.IsPublic || type.IsNestedPublic && type.IsModuleCandidate() && type.DeclaringType == null select type;

        public static IReadOnlyDictionary<Type, Info.ModuleInfo> BuildModules(IEnumerable<TypeInfo> types, SlashService service, IServiceProvider services)
        {
            var list = new Dictionary<Type, Info.ModuleInfo>();

            foreach (var type in types)
            {
                var builder = new ModuleBuilder(service, null);

                BuildModule(builder, type, service);
                BuildGroups(builder, type.DeclaredNestedTypes, service);

                list[type.AsType()] = builder.Build();
            }
            return list;
        }

        private static void BuildGroups(ModuleBuilder builder, IEnumerable<TypeInfo> types, SlashService service)
        {
            foreach (var type in types.Where(IsGroupCandidate))
                builder.AddModule(x =>
                {
                    BuildModule(x, type, service);
                    BuildGroups(x, type.DeclaredNestedTypes, service);
                });
        }

        private static void BuildModule(ModuleBuilder builder, TypeInfo type, SlashService service)
        {
            foreach (var attribute in type.GetCustomAttributes())
                builder.Name = attribute switch
                {
                    RocaModuleAttribute module => module.Name.ToLowerInvariant(),
                    _ => builder.Name
                };

            if (string.IsNullOrWhiteSpace(builder.Name))
                throw new RocaBuilderException("Module must have a name");

            builder.Description = type.GetLocalizer()[CultureInfo.GetCultureInfo("en-US"), $"{type.Name}_desc"];

            foreach (var command in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(IsCommandCandidate))
                builder.AddCommand(x => BuildCommand(x, type, command, service));
        }

        private static void BuildCommand(CommandBuilder builder, TypeInfo type, MethodInfo method, SlashService service)
        {
            foreach (var attribute in method.GetCustomAttributes())
                builder.Name = attribute switch
                {
                    RocaCommandAttribute command => command.Name.ToLowerInvariant(),
                    _ => builder.Name
                };

            if (string.IsNullOrWhiteSpace(builder.Name))
                throw new RocaBuilderException("Command must have a name");

            builder.Description = type.GetLocalizer()[CultureInfo.GetCultureInfo("en-US"), $"{builder.Name}_desc"];

            foreach (var parameter in method.GetParameters())
                builder.AddParameter(x => BuildParameter(x, parameter, builder, service));

            builder.Callback = async (context, args, provider) =>
            {
                var instance = CreateInstance(type, provider);
                instance.Context = context;
                instance.Localizer = type.GetLocalizer();

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
            foreach (var property in type.BaseType?.GetTypeInfo().DeclaredProperties ?? Array.Empty<PropertyInfo>())
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
            builder.Description = type.GetLocalizer()[CultureInfo.GetCultureInfo("en-US"), $"{command.Name}_{builder.Name}_desc"];

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
