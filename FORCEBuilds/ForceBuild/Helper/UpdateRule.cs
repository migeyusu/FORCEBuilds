using System.Collections.Generic;
using System.Reflection;

namespace FORCEBuild.Helper
{
    public class UpdateRule
    {
        /// <summary>
        /// 仅保留可读写集合
        /// </summary>
        public List<PropertyInfo> PropertyInfos { get; set; }
        
    }
}