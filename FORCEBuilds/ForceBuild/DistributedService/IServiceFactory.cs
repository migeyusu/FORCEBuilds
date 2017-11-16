namespace FORCEBuild.DistributedService
{
    public interface IServiceFactory
    {
        T Create<T>() where T : class;
    }
}