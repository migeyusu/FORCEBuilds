using System;

namespace FORCEBuild.Persistence.Configuration
{
    [AttributeUsage(AttributeTargets.Class)]
    public class OrmAttribute:global::System.Attribute
    {
        public string Table { get; set; }

        public OrmAttribute(string table)
        {
            Table = table;
        }
    }
}