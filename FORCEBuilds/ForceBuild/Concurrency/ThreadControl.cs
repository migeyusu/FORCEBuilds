using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Xunit;

namespace FORCEBuild.Concurrency
{
    public class ThreadControl
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetCurrentThread();
        [DllImport("kernel32.dll")]
        public static extern UIntPtr SetThreadAffinityMask(IntPtr hThread, UIntPtr dwThreadAffinityMask);
        public static void MyThreadToCore(string cores)//绑定当前线程核心，从1开始计数
        {
                SetThreadAffinityMask(GetCurrentThread(),(UIntPtr)Convert.ToInt32(cores,2));    
        }
        public static void MyProcessHigh()//设置本进程优先级最高
        {
            var myProcess =Process.GetCurrentProcess();
            myProcess.PriorityClass = System.Diagnostics.ProcessPriorityClass.High;
        }

    }

}
