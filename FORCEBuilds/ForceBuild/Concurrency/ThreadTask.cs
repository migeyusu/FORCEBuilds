using System;
using System.Threading;

namespace FORCEBuild.Concurrency
{
    //线程记录表
    public struct ThreadRecord
    {
        //   Highest：1，AboveNormal：2，Normal：3，BelowNormal：4，Lowest：5
        public bool unusualState;//绑定逻辑核心/修改优先级即改变
        public ThreadPriority TP;//优先级
        public uint TLogicCore;//绑定逻辑核心
        public IntPtr TrdPtr;//线程句柄
        public long elapsedTime;//任务经历时间
    }

    public class ThreadTask  //无传参线程池
    {
        private static int ThreadLength;//最大并行线程数
        private static ThreadRecord[] ThreadTable;//线程状态表
        private static Thread[] childThread;//线程体数组的序列是线程池唯一标识符
        private static AutoResetEvent[] ARE;//线程信号
        private static IntStack ThreadStack;//可用线程栈(不管是否阻塞，有任务即出栈)
        public delegate void ThreadDeleagte();
        //public static intStack.actionDeleage comAction;//设置所有线程完成后的事件
        //comaction为子线程，非UI线程，操纵需要用invoke
        public static ThreadDeleagte[] TBodyDlt; //函数体
        public ThreadTask(int len)
        {
            ThreadLength = len;
            childThread = new Thread[ThreadLength];//所有序号以0起步
            ThreadTable = new ThreadRecord[ThreadLength];
            ARE = new AutoResetEvent[ThreadLength];//控制信号
            ThreadStack = new IntStack(ThreadLength);//线程栈,入栈表示被挂起
            TBodyDlt = new ThreadDeleagte[ThreadLength];
        }
        public bool ThreadStop()//全部线程关闭并重启
        {
            try
            {
                ThreadShutDown();
                ThreadStack.StackReset();//栈清空
                for (var i = 0; i < ThreadLength; i++)
                {
                    var td = new Thread(delegate() { ThreadBody(i); });
                    childThread[i] = td;
                    childThread[i].Start();
                    ThreadStack.Push(i);
                    Thread.Sleep(50);
                }
                IntStack.actionWork = true;
                return true;
            }
            catch
            {
                return false;
            }

        }
        public bool ThreadStop(int x)//关闭指定ID线程,堆外线程有效，否则false
        {
            IntStack.actionWork = false;
            try
            {
                if (ThreadStack.InOrNot(x))
                {
                    return false;
                }
                childThread[x].Abort();
                var t1 = new Thread(delegate() { ThreadBody(x); });
                childThread[x] = t1;
                childThread[x].Start();
                ThreadStack.Push(x);
                IntStack.actionWork = true;
                return true;
            }
            catch
            {
                return false;
            }
            
        }
        public void ThreadReady()//线程准备
        {
            IntStack.actionWork = false;
            for(var i = 0; i < ThreadLength; i++)
            {
                var childare = new AutoResetEvent(false);
                ARE[i] = childare;
                var t1 = new Thread(delegate() { ThreadBody(i); });
                ThreadTable[i].unusualState = false;
                childThread[i] = t1;
                childThread[i].Start();
                ThreadStack.Push(i);//线程入栈
                Thread.Sleep(50);//降低cpu使用率
            }
            IntStack.actionWork = true;//线程栈开始响应
        }
        private void ThreadBody(int CrtThead)//CrtThead表示在线程数组中的位置
        {
            
            var ThreadID = CrtThead;
            ThreadTable[ThreadID].TrdPtr = ThreadControl.GetCurrentThread();//线程句柄写入表
            while (true)
            {
                ARE[ThreadID].WaitOne();
                TBodyDlt[ThreadID]();
                if(ThreadTable[ThreadID].unusualState)
                {
                    ThreadControl.SetThreadAffinityMask(ThreadTable[ThreadID].TrdPtr, new UIntPtr(0));
                    childThread[ThreadID].Priority = ThreadPriority.Normal;
                    ThreadTable[ThreadID].unusualState = false;
                }
                ThreadStack.Push(ThreadID);//线程变成可用状态，置于最后
            }
        }
        public int TaskIn(ThreadDeleagte TB, bool GoThrough=true,string ThreadLogicCore="",
            ThreadPriority Tp=ThreadPriority.Normal)//绑定核心，线程优先级,是否立即运行
        {
            IntStack.actionWork = true;
            var s = -1;
            if (!ThreadStack.Pop(ref s))
            {
                return -1;//返回-1，则说明线程已用完，进入等待队列
            }
            if(ThreadLogicCore!="")
            {
                ThreadControl.SetThreadAffinityMask(ThreadTable[s].TrdPtr, (UIntPtr)Convert.ToInt32(ThreadLogicCore,2));
                ThreadTable[s].unusualState = true;
            }
            if(Tp!=ThreadPriority.Normal)
            { 
                childThread[s].Priority = Tp;
                ThreadTable[s].unusualState = true;
            }
               
            TBodyDlt[s] = new ThreadDeleagte(TB);
            if(!GoThrough)//此时被阻塞的线程仍然属于“不可用状态”，已出堆
            {
                return s;
            }
            ARE[s].Set();
            return s;//返回线程序号
        }
        public bool TaskStart(int x)//指定已有任务但被阻塞线程运行，不能对栈内线程作用
        {
            if(ThreadStack.InOrNot(x))
            {
                return false;
            }
            ARE[x].Set();
            return true;
        }
        public void ThreadShutDown()//全部线程关闭
        {
            for (var i = 0; i < ThreadLength; ++i)
            {
                childThread[i].Abort();
                Thread.Sleep(50);
            }
        }
        public string ThreadStatus()//输出线程状态
        {
            var str1 = "";
            for (var i = 0; i < ThreadLength; i++)
            {
                str1 = str1 + i.ToString() + ":" + childThread[i].ThreadState.ToString() + ",";
            }
            return str1;
        }
        public string ThreadStatus(int x)//返回特定ID线程状态
        {
            return childThread[x].ThreadState.ToString();
        }
         ~ThreadTask()//释放资源
        {
            ThreadTable = null;
            childThread = null;
            ARE = null;
            ThreadStack = null;
            TBodyDlt = null;
        }
    }
    ///线程安全的标记栈，用于记录线程的状态和数量
    public class IntStack
    {
        public delegate void ActionDeleage();
        public  static ActionDeleage addAction;
        public static  bool actionWork=false;
        private static readonly object locker = new object();
        private static int stackLen;
        private int[] ary;//从0开始
        public int StackTop = -1;//最大可用栈顶
        public int CrtTop = -1;//当前元素栈顶
        public bool InOrNot(int x)//查询元素是否在栈内
        {
            for(var i=0;i<=CrtTop;i++)
            {
                if (ary[i] == x)
                    return true; 
            }
            return false;
        }
        public void StackReset()//丢弃全栈数据
        {
            CrtTop = -1;
        }
        public void StackReset(int i)//重置全栈
        {
            stackLen = i;
            ary = new int[stackLen];
            StackTop = stackLen - 1;
            CrtTop = -1;
        }
        public IntStack(int i)
        {
            stackLen = i;
            ary = new int[stackLen];
            StackTop = stackLen-1;
        }
        public bool Push(int x)
        {
            lock (locker)
            {
                if (CrtTop == StackTop)
                    return false;
                ++CrtTop;
                ary[CrtTop] = x;
                if(CrtTop==StackTop )
                if(addAction!=null && actionWork==true)
                { 
                    addAction();
                    //actionWork = false;
                }
                return true;
            }
        }
        public bool Pop(ref int x)
        {
            if (CrtTop < 0)
                return false;
            x = ary[CrtTop];
            --CrtTop;
            return true;
        }
        public bool Peek(ref int x)
        {
            if (CrtTop < 0)
                return false;
            x = ary[CrtTop];
            return true;
        }
    }
}
