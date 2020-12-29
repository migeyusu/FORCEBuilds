using System;

namespace FORCEBuild.Persistence.Configuration
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ForeignOneAttribute:global::System.Attribute
    {
        public string ForeignKey { get; set; }

        public ForeignOneAttribute(string fk)
        {
            ForeignKey = fk;
        }
    }
}