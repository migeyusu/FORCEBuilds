using System;
using System.Collections.Generic;
using System.Reflection;

namespace FORCEBuild.Plugin
{
    public class ExtensionEntry
    {
        /// <summary>
        /// 扩展名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 扩展所在的文件夹路径
        /// </summary>
        public string DirectoryLocation { get; set; }

        /// <summary>
        /// 扩展类型
        /// </summary>
        public IEnumerable<Type> Types { get; set; }
    }

    public class ExtensionLoader
    {
        public void Initialize()
        {
        }
    }
}