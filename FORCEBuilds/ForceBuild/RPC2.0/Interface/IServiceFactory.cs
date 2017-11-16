using System;
using System.ComponentModel.Design;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using FORCEBuild.Net;
using FORCEBuild.Net.Service;

namespace FORCEBuild.RPC2._0.Interface
{
    /// <summary>
    /// 服务工厂，远程客户端调用以创建服务
    /// </summary>
    public interface IServiceFactory
    {
        IException ExceptionCatcher { get; set; }
        
        T CreateService<T>();
    }
}