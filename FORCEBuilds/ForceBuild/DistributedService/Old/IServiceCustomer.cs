using System;

namespace FORCEBuild.RPC3._0
{
    public interface IServiceCustomer:IServiceUriGet
    {
        /// <summary>
        /// 过滤器，针对ServiceProvider
        /// </summary>
        Guid Filter { get; set; }

        void Start();

        void Start(int port);

        void End();

    }
}