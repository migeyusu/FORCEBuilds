using System;

namespace FORCEBuild.ORM.Register
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