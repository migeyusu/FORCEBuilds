using System;
using System.Collections.Generic;

namespace FORCEBuild.Plugin
{
    public class Extension
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
        /// 扩展已实现的接口，用于预加载
        /// <para>当前不支持泛型接口</para>
        /// </summary>
        public IEnumerable<Type> InterfaceTypes { get; set; }
    }
}