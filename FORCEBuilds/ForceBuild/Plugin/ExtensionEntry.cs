using System;
using System.Collections.Generic;
using System.Reflection;

namespace FORCEBuild.Plugin
{
    public class ExtensionEntry : Extension
    {
        public IEnumerable<Assembly> Assemblies { get; set; }

        /// <summary>
        /// key:interface type
        /// </summary>
        public Dictionary<Type, ExtensionTypePairEntry> LoadedPairEntries { get; set; }
    }
}