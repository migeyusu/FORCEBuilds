using System;

namespace FORCEBuild.ORM.Configuration
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