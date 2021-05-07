using System;
using InterfaceDefine;

namespace Implement
{
    public class TestClass:MarshalByRefObject,ITest
    {
        public void Message()
        {
            Console.Write("sadfasfd");
        }
    }
}