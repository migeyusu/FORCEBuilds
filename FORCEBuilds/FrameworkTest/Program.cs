using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FORCEBuild.Plugin;
using InterfaceDefine;

namespace FrameworkTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var extensionLoader = new ExtensionLoader();
            extensionLoader.Initialize(new[]
            {
                new Extension()
                {
                    DirectoryLocation = @"C:\Users\zhujie\Source\Repos\FORCEBuilds\FORCEBuilds\Implement\bin\Debug",
                    Name = "Test",
                    InterfaceTypes = new[] {typeof(ITest)},
                }
            });
            var test = extensionLoader.Create<ITest>("Test");
            test.Message();
            Console.ReadKey();
        }
    }
}