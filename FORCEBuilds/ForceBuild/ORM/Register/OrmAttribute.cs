using System;

namespace FORCEBuild.ORM.Register
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