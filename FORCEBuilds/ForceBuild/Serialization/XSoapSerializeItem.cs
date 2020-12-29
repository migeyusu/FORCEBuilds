using System;
using System.Reflection;

namespace FORCEBuild.Serialization
{
    /// <summary>
    /// 每个item都已经包含了解析类所需要的所有数据
    /// </summary>
    [Serializable]
    internal class XSoapSerializeItem
    {
        /// <summary>
        /// 包括array和继承迭代器接口
        /// </summary>
        public bool IsIEnumerable { get; set; }
        public bool IsContainCustom { get; set; }
        /// <summary>
        /// 集合的单个类型
        /// </summary>
        public Type ItemType { get; set; }
        public Type Type { get; set; }
        public object Data { get; set; }
        public PropertyInfo Property { get; set; }

        public XSoapSerializeItem Clone()
        {
            return new XSoapSerializeItem
            {
                IsIEnumerable = IsIEnumerable,
                IsContainCustom = IsContainCustom,
                ItemType = ItemType,
                Type = Type,
                Property = Property
            };
        }
    }
}
