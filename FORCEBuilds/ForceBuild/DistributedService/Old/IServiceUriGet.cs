using System;

namespace FORCEBuild.RPC3._0
{
    /// <summary>
    /// 缓存形式
    /// </summary>
    public interface IServiceUriGet
    {
        /// <summary>
        /// 根据接口取得指定的终结点
        /// </summary>
        /// <param name="type">接口</param>
        /// <returns></returns>
        ServiceUri GetServiceUri(Type type);
    }
}