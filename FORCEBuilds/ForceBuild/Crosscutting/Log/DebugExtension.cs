#define customdebug

using System;
using System.Diagnostics;


namespace FORCEBuild.Crosscutting.Log
{
    public static class DebugExtension
    {
        public static void WriteLine(string message)
        {
#if customdebug
            var type = new StackFrame(1).GetMethod().DeclaringType;
            Debug.WriteLine($"From {type.Name}:{message}");
#endif
        }
        public static void WriteLine(Exception exception)
        {
#if customdebug
            var type = new StackFrame(1).GetMethod().DeclaringType;
            while (true)
            {
                Debug.WriteLine($"From {type.Name}[{DateTime.Now}] 异常类型：{exception.GetType()}\r\n异常消息：{exception.Message}\r\n异常信息：{exception.StackTrace}");
                if (exception.InnerException != null)
                {
                    Debug.WriteLine("Inner Exception:");
                    exception = exception.InnerException;
                    continue;
                }
                break;
            }
#endif
        }
    }
}