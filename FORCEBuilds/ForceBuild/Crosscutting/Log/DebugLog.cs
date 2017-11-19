using System;
using System.Diagnostics;

namespace FORCEBuild.Crosscutting.Log
{
    public class DebugLog:ILog
    {
        public void Write(string sentence)
        {
            var type = new StackFrame(1).GetMethod().DeclaringType;
            Debug.WriteLine($"From {type.Name}:{sentence}");
        }

        public void Write(Exception ex)
        {
            var type = new StackFrame(1).GetMethod().DeclaringType;
            while (true)
            {
                Debug.WriteLine($"From {type.Name}:[{DateTime.Now}] 异常类型：{ex.GetType()}\r\n异常消息：{ex.Message}\r\n异常信息：{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine("Inner Exception:");
                    ex = ex.InnerException;
                    continue;
                }
                break;
            }
        }
    }
}