using System;
using System.Reflection;

namespace FORCEBuild.Serialization
{

    [Serializable]
    public class XBinarySerializePair
    {
        public bool IsCustom { get; set; }
        public PropertyInfo Property { get; set; }
        public Type Type { get; set; }
        public bool IsIEnumerable { get; set; }
        public object Value { get; set; }
        public XBinarySerializePair Clone()
        {
            return new XBinarySerializePair {
                IsCustom = this.IsCustom,
                IsIEnumerable = this.IsIEnumerable
            };
        }
    }
}