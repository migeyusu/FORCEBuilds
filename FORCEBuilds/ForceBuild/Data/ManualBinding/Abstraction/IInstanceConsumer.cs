namespace FORCEBuild.Data.ManualBinding.Abstraction
{
    public interface IInstanceConsumer<T>
    {
        void Consume(T t);
    }
}