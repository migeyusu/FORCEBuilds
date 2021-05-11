using System;
using PluginInterface;

namespace PluginImplement
{
    public class TestClass:MarshalByRefObject,ITest
    {
        public void Message()
        {
            Console.Write("sadfasfd");
        }
    }
}