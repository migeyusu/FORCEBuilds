using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using AutoMapper;
using AutoMapper.Internal;
using FORCEBuild.Helper;
using Microsoft.Extensions.DependencyInjection;

namespace FORCEBuild.Plugin
{
    public class Extension
    {
        /// <summary>
        /// 扩展名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 扩展所在的文件夹路径
        /// </summary>
        public string DirectoryLocation { get; set; }

        /// <summary>
        /// 扩展已实现的接口，用于预加载
        /// <para>当前不支持泛型接口</para>
        /// </summary>
        public IEnumerable<Type> InterfaceTypes { get; set; }
    }

    internal class ExtensionEntry : Extension
    {
        public IEnumerable<Assembly> Assemblies { get; set; }

        /// <summary>
        /// key:interface type
        /// </summary>
        public Dictionary<Type, ExtensionTypePairEntry> ImplementTypes { get; set; }
    }

    internal class ExtensionTypePairEntry
    {
        public Type InterfaceType { get; set; }

        public Type ImplementType { get; set; }
        public bool IsTypeCached { get; set; }

        /// <summary>
        /// 加快invoke
        /// </summary>
        public ConstructorInfo ConstructorInfo { get; set; }
    }

    /// <summary>
    /// load into current appdomain
    /// <para>support container di</para>
    /// </summary>
    public class ExtensionLoader
    {
        private IEnumerable<ExtensionEntry> _extensionEntries;

        public void Initialize(IEnumerable<Extension> extensions)
        {
            var mapper =
                new Mapper(new MapperConfiguration(expression =>
                    expression.CreateMap<Plugin.Extension, ExtensionEntry>()));
            this._extensionEntries = extensions.Select(entry =>
            {
                var directoryInfo = new DirectoryInfo(entry.DirectoryLocation);
                if (!directoryInfo.Exists)
                {
                    throw new DirectoryNotFoundException(nameof(directoryInfo.FullName));
                }

                var assemblies = directoryInfo.GetFiles("*.dll")
                    .Select(info => Assembly.LoadFile(info.FullName));
                var extensionEntry = mapper.Map<ExtensionEntry>(entry);
                extensionEntry.Assemblies = assemblies;
                extensionEntry.ImplementTypes = FindTypes(assemblies, entry.InterfaceTypes)
                    .ToDictionary((pairEntry => pairEntry.InterfaceType));
                return extensionEntry;
            });
        }


        private IEnumerable<ExtensionTypePairEntry> FindTypes(IEnumerable<Assembly> assemblies,
            IEnumerable<Type> interfaceTypes)
        {
            var types = assemblies.SelectMany((assembly => assembly.GetLoadableTypes()));
            var typePairEntries = interfaceTypes.Select((type => new ExtensionTypePairEntry()
            {
                InterfaceType = type,
            }));
            var extensionTypePairEntries = new List<ExtensionTypePairEntry>(interfaceTypes.Count());
            foreach (var type in types)
            {
                var pairEntries = typePairEntries.Where((entry => !entry.IsTypeCached));
                if (!pairEntries.Any())
                {
                    return typePairEntries;
                }

                foreach (var typePairEntry in pairEntries)
                {
                    var interfaceType = typePairEntry.InterfaceType;
                    if (type.IsClass && !type.IsAbstract && interfaceType.IsAssignableFrom(type))
                    {
                        ConstructorInfo targetConstructorInfo;
                        if ((targetConstructorInfo = type.GetConstructor(Type.EmptyTypes)) == null) continue;
                        typePairEntry.ConstructorInfo = targetConstructorInfo;
                        typePairEntry.ImplementType = type;
                        typePairEntry.IsTypeCached = true;
                    }
                }
            }

            return extensionTypePairEntries;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">extension name</typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public T Create<T>(string name)
        {
            var extensionEntry = _extensionEntries.FirstOrDefault((entry => entry.Name == name));
            if (extensionEntry == null)
            {
                throw new ArgumentOutOfRangeException(nameof(name), $"There's no extension named '{name}'");
            }

            var type = typeof(T);
            if (!extensionEntry.ImplementTypes.TryGetValue(type, out var value))
                throw new KeyNotFoundException($"Can't find interface pre defined in extension '{name}'.");
            if (!value.IsTypeCached)
            {
                throw new Exception(
                    $"Can't load class which inherited interface named '{type.Name}' with parameterless constructor");
            }

            return (T) value.ConstructorInfo.Invoke(null);

        }
    }
}