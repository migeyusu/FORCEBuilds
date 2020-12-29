using System;

namespace FORCEBuild.Persistence.Serialization
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Property)]
    public class XSoapElementAttribute : Attribute { }
}
