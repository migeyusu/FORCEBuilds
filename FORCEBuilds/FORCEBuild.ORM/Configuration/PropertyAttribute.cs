using System;

namespace FORCEBuild.Persistence.Configuration
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