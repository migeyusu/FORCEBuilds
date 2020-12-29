using System;

namespace FORCEBuild.Serialization
{
    [Serializable]
    internal class XSoapSerializeRoot
    {
        public Type ClassType { get; set; }
        public XSoapSerializeItem[] Elements { get; set; }
        public XSoapSerializeRoot Clone()
        {
            var sr = new XSoapSerializeRoot
            {
                ClassType = ClassType,
                Elements = new XSoapSerializeItem[Elements.Length]
            };
            for (var i = 0; i < Elements.Length; ++i)
            {
                sr.Elements[i] = Elements[i].Clone();
            }
            return sr;
        }
    }
}
