namespace FORCEBuild.Data.ManualBinding.Abstraction
{
    public interface IValueReceiver<T>
    {
        void Receive(T x);
    }
}