using System;

namespace FORCEBuild.ORM.Configuration
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OneToManyAttribute:global::System.Attribute
    {
        public string ForeignKey { get; set; }

        public bool Update { get; set; }

        public OneToManyAttribute(string fk,bool update)
        {
            ForeignKey = fk;
            Update = update;
        }
    }
}