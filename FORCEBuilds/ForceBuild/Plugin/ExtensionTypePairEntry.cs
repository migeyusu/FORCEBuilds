using System;

namespace FORCEBuild.Plugin
{
    public class ExtensionTypePairEntry
    {
        public Type InterfaceType { get; set; }

        public Type ImplementType { get; set; }

        public bool IsTypeFind { get; set; }
    }
}