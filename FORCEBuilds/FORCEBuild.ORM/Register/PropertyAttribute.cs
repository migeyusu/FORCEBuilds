using System;

namespace FORCEBuild.ORM.Register
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyAttribute:global::System.Attribute
    {
        public string Column { get; set; }

        public PropertyAttribute(string column)
        {
            Column = column;
        }
    }
}