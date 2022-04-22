using FORCEBuild.Net.Abstraction;

namespace FORCEBuild.Net.NamedPipe
{
    public interface INamedPipeMessageClient : IMessageClient
    {
        string PipeName { get; set; }
    }
}