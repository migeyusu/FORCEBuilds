using System;
using System.Runtime.Serialization.Formatters.Binary;
using FORCEBuild.Net.NamedPipe;
using FORCEBuild.Net.RPC;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace TestProject
{
    public class NamedPipeTest
    {
        private readonly ITestOutputHelper _output;

        public NamedPipeTest(ITestOutputHelper output)
        {
            this._output = output;
        }

        [Fact]
        public void LongConnection()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging((builder => builder.AddDebug()))
                .AddTransient<ITestRPC, TestRPC>();
            using (var serviceProvider = serviceCollection.BuildServiceProvider())
            {
                var logger = serviceProvider.GetService<ILogger<NamedPipeMessageServer>>();
                var serviceLogger = serviceProvider.GetService<ILogger<LongConnectionNamedPipeMessageClient>>();
                using (var server = new NamedPipeMessageServer("testPi", new BinaryFormatter(),
                           new CallProducePipe(new ServiceHandler(serviceProvider)), 3)
                       {
                           Logger = logger,
                           IsLongConnection = true
                       })
                {
                    server.Start();
                    using (var client = new LongConnectionNamedPipeMessageClient("testPi",
                               TimeSpan.FromMilliseconds(3000), new BinaryFormatter(), serviceLogger))
                    {
                        var proxyServiceFactory = new ProxyServiceFactory(client);
                        var testRpc = proxyServiceFactory.CreateService<ITestRPC>();
                        var add = testRpc.Add(1, 1);
                        _output.WriteLine(add.ToString());
                        var i = testRpc.Add(1, 2);
                        _output.WriteLine(i.ToString());
                        var x = testRpc.Add(3, 4);
                        _output.WriteLine(x.ToString());
                    }

                    server.Stop();
                }
            }
        }
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