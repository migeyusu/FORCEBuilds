using System;
using System.Runtime.Serialization.Formatters.Binary;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using FORCEBuild.Helper;
using FORCEBuild.Net.NamedPipe;
using FORCEBuild.Net.RPC;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace TestProject
{
    public class Program
    {
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging((builder => builder.AddConsole()));
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var logger = serviceProvider.GetService<ILogger<NamedPipeMessageServer>>();
            var windsorContainer = new WindsorContainer();
            windsorContainer.Register(Component.For<ITestRPC>()
                .ImplementedBy<TestRPC>());
            var namedPipeMessageServer = new NamedPipeMessageServer("testPi", new BinaryFormatter(),
                new CallProducePipe(new ServiceHandler(windsorContainer)), 3) { Logger = logger };
            namedPipeMessageServer.Start();
            Console.WriteLine("Start");
            Console.ReadLine();
        }
    }
}