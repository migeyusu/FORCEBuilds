using System;

namespace FORCEBuild.Crosscutting.Validation
{
    /// <summary>
    /// 定义赋值过滤规则
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public abstract class XValidaterAttribute:Attribute
    {
        public abstract void Validate(object val,string name);
    }
}
