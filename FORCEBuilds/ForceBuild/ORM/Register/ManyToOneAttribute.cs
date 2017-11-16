using System;

namespace FORCEBuild.ORM.Register
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ManyToOneAttribute:global::System.Attribute
    {
        public string ForeignKey { get; set; }

        public ManyToOneAttribute(string fk)
        {
            ForeignKey = fk;
        }
    }
}