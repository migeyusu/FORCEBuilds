namespace FORCEBuild.Data.ManualBinding.Abstraction
{
    public interface IValueProvider<T>
    {
        T Provide();
    }
}