using System;

namespace FORCEBuild.Persistence.Configuration
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OneToOneAttribute:global::System.Attribute
    {
        public bool Update { get; set; }
        /// <summary>
        /// 外键列
        /// </summary>
        public string ForeignKey { get; set; }

        public OneToOneAttribute(bool update,string refercolumn)
        {
            Update = update;
            ForeignKey = refercolumn;
        }
    }
}