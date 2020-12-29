using System;
using System.Collections.Generic;

namespace FORCEBuild.Net.DistributedStorage
{
    /// <summary>
    /// domain model工厂类需要预先知道可用类型定义
    /// 2017.5.14停止使用该接口
    /// </summary>
    public interface ModelFactory
    {
        Dictionary<Type, ClassDefine> DefinePairs { get; set; }

        object Create(ClassDefine define,bool cannotify=true); 

         T Get<T>();

         UpdateScheduler Scheduler { get; set; }
    }

}
