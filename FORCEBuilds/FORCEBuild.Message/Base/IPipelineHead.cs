namespace FORCEBuild.Message.Base
{
    public interface IPipelineHead<T>
    {
        void Request(T x);  
    }
}