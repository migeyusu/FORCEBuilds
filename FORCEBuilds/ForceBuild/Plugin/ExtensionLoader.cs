using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using AutoMapper;
using AutoMapper.Internal;
using Castle.Core.Internal;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using FORCEBuild.Helper;


namespace FORCEBuild.Plugin
{
    /// <summary>
    /// load into current appdomain
    /// <para>support container di</para>
    /// </summary>
    public class ExtensionLoader
    {
        /// <summary>
        /// 可通过该接口注入特定服务
        /// </summary>
        public IWindsorContainer MergedContainer { get; set; }

        private readonly IWindsorContainer _internalContainer;

        /// <summary>
        /// 是否允许扩展的dll替换原有的assembly
        /// </summary>
        public bool IsAllowDuplicatedDll { get; set; } = false;

        /// <summary>
        /// 表示依赖容器是否隔离
        /// </summary>
        public bool IsContainerIsolation { get; private set; }

        public IEnumerable<ExtensionEntry> ExtensionEntries { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container">附加的容器</param>
        /// <param name="isContainerIsolation">依赖容器是否隔离，如果是，将使用一个新的内部容器</param>
        public ExtensionLoader(IWindsorContainer container, bool isContainerIsolation = false)
        {
            this.IsContainerIsolation = isContainerIsolation;
            if (container != null)
            {
                if (isContainerIsolation)
                {
                    _internalContainer = new WindsorContainer();
                    MergedContainer.AddChildContainer(_internalContainer);
                }
                else
                {
                    _internalContainer = MergedContainer;
                }
            }
            else
            {
                _internalContainer = new WindsorContainer();
            }
        }

        public ExtensionLoader() : this(new WindsorContainer())
        {
        }

        public void Load(IEnumerable<Extension> extensions)
        {
            var mapper =
                new Mapper(new MapperConfiguration(expression =>
                    expression.CreateMap<Plugin.Extension, ExtensionEntry>()));
            this.ExtensionEntries = extensions.Select(entry =>
            {
                var directoryInfo = new DirectoryInfo(entry.DirectoryLocation);
                if (!directoryInfo.Exists)
                {
                    throw new DirectoryNotFoundException(nameof(directoryInfo.FullName));
                }

                var assemblyNames = directoryInfo.GetFiles("*.dll")
                    .Select(info => AssemblyName.GetAssemblyName(info.FullName));
                IEnumerable<Assembly> assemblies;
                if (IsAllowDuplicatedDll)
                {
                    assemblies = assemblyNames.Select(name => Assembly.Load(name)).ToArray();
                }
                else
                {
                    var loadedAssemblyNames = AppDomain.CurrentDomain.GetAssemblies()
                        .Select(assembly => assembly.GetName().Name);
                    assemblies = assemblyNames
                        .Where(name => !loadedAssemblyNames.Contains(name.Name))
                        .Select(name => Assembly.Load(name)).ToArray();
                }

                var extensionEntry = mapper.Map<ExtensionEntry>(entry);
                extensionEntry.Assemblies = assemblies;
                extensionEntry.LoadedPairEntries =
                    FindTypes(assemblies, entry.InterfaceTypes, _internalContainer, entry.Name)
                        .ToDictionary(pairEntry => pairEntry.InterfaceType);
                return extensionEntry;
            }).ToArray();
        }


        private static IEnumerable<ExtensionTypePairEntry> FindTypes(IEnumerable<Assembly> assemblies,
            IEnumerable<Type> interfaceTypes, IWindsorContainer container, string extensionName)
        {
            var types = assemblies.SelectMany((assembly => assembly.GetLoadableTypes()));
            var typePairEntries = interfaceTypes.Select((type =>
                    new ExtensionTypePairEntry()
                    {
                        InterfaceType = type,
                    })
            ).ToArray();
            foreach (var rawType in types)
            {
                var pairEntries = typePairEntries.Where((entry => !entry.IsTypeFind));
                if (!pairEntries.Any())
                {
                    return typePairEntries;
                }

                foreach (var typePairEntry in pairEntries)
                {
                    var interfaceType = typePairEntry.InterfaceType;
                    if (rawType.IsClass && !rawType.IsAbstract && interfaceType.IsAssignableFrom(rawType))
                    {
                        typePairEntry.ImplementType = rawType;
                        typePairEntry.IsTypeFind = true;
                        container.Register(Component.For(interfaceType)
                            .ImplementedBy(rawType)
                            .Named(extensionName)
                            .LifestyleTransient());
                    }
                }
            }

            return typePairEntries;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">extension name</param>
        /// <returns></returns>
        public T Create<T>(string name)
        {
            if (ExtensionEntries.IsNullOrEmpty())
            {
                return default;
            }
            //validate first
            var extensionEntry = ExtensionEntries.FirstOrDefault((entry => entry.Name == name));
            if (extensionEntry == null)
            {
                return default; // throw new ArgumentOutOfRangeException(nameof(name), $"There's no extension named '{name}'");
            }

            var type = typeof(T);
            if (!extensionEntry.LoadedPairEntries.TryGetValue(type, out var value))
                return default;
                // throw new KeyNotFoundException($"Can't find interface pre defined in extension '{name}'.");
            if (!value.IsTypeFind)
            {
                return default;
                /*throw new Exception(
                    $"Can't load class which inherited interface named '{type.Name}' with parameterless constructor");*/
            }

            return _internalContainer.Resolve<T>(name);
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            if (ExtensionEntries.IsNullOrEmpty())
            {
                return Enumerable.Empty<T>();
            }
            return _internalContainer.ResolveAll<T>();
        }
    }
}