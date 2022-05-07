using FORCEBuild.Net.Abstraction;

namespace FORCEBuild.Net.NamedPipe
{
    public interface INamedPipeMessageServer : IMessageServer
    {
        string PipeName { get; set; }

        int MaxConnections { get; set; }
    }
}