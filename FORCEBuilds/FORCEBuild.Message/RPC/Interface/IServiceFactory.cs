namespace FORCEBuild.Message.RPC.Interface
{
    /// <summary>
    /// 服务工厂，远程客户端调用以创建服务
    /// </summary>
    public interface IServiceFactory
    {
        T CreateService<T>();
    }
}