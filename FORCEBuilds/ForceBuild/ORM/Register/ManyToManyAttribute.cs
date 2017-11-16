using System;

namespace FORCEBuild.ORM.Register
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ManyToManyAttribute:global::System.Attribute
    {
        public string PrimaryKay { get; set; }
        public string ForeignKey { get; set; }
        public string Table { get; set; }
        /// <summary>
        /// 表示更新
        /// </summary>
        public bool Update { get; set; }

        public ManyToManyAttribute(string pk,string fk,string table,bool update)
        {
            PrimaryKay = pk;
            ForeignKey = fk;
            Table = table;
            Update = update;
        }
    }
}