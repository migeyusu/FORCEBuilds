using System;
using System.Data;

namespace FORCEBuild.ORM
{
    /// <summary>
    /// 表示描述一对数据库列名和值的对,以后启用更多描述字段优化性能
    /// </summary>
    public struct ColumnValuePair
    {
        public Type Type { get; set; }
        public SqlDbType DbType { get; set; }
        public string Column { get; set; }
        private object _value;
        /// <summary>
        /// 自动设置null
        /// </summary>
        public object Value
        {
            get => _value;
            set => _value = value ?? DBNull.Value;
        }
    }
}
