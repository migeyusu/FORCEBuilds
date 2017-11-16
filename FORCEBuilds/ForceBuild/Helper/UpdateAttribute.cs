using System;
using System.Runtime.InteropServices;

namespace FORCEBuild.Helper
{
    /// <summary>
    /// 标记需要更新的属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class UpdateAttribute:Attribute
    {
        
    }
}