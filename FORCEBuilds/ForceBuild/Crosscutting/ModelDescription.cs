using System;
using System.Reflection;

namespace FORCEBuild.Crosscutting
{
    /// <summary>
    /// 可以提供中文注释
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IDescriptionAttribute:Attribute
    {
        public string Description { get; set; }
        public bool Single { get; set; }
        public IDescriptionAttribute(string description, bool single = false)
        {
            Description = description;
            Single = single;
        }
    }
    public class ModelDescription
    {
        public ModelDescription()
        {
            
        }
        public static IDescriptionAttribute GetInfo(object model,string name)
        {
            var pi = model.GetType().GetProperty(name);
            var idpa = pi.GetCustomAttribute<IDescriptionAttribute>();
            return idpa;
        }
    }
}
