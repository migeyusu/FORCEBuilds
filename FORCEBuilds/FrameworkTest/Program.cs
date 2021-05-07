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
            var pluginLoader = new PluginLoader();
            pluginLoader.LoadPlugin("test", @"C:\Users\zhujie\Source\Repos\FORCEBuilds\FORCEBuilds\Implement\bin\Debug", "Implement");
            var instance = pluginLoader.CreateInstance<ITest>("test", "Implement", "Implement.TestClass");
            instance.Message();
            Console.ReadKey();
        }
    }
}
