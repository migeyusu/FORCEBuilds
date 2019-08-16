using System;

namespace FORCEBuild.ORM.Configuration
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyAttribute:Attribute
    {
        public string Column { get; set; }

        public PropertyAttribute(string column)
        {
            Column = column;
        }
    }
}