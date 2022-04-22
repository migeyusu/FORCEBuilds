using FORCEBuild.Net.RPC;

namespace TestProject
{
    public class NamedPipeTest
    {
    }

    [RemoteInterface]
    public interface ITestRPC
    {
        int Add(int a, int b);
    }


    public class TestRPC : ITestRPC
    {
        public int Add(int a, int b)
        {
            return a + b;
        }
    }
}