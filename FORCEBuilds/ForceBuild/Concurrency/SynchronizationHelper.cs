﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace FORCEBuild.Concurrency
{
    /* SynchronizationContext是一个比dispatcher更通用的后台线程调用UI对象，
     * begininvoke：通过向一个队列发送带有优先级的delegate，被sta thread调用实现UI刷新（异步）
     * invoke：阻塞sta thread直接强迫执行（同步）
     * 在wpf下获取SynchronizationContext.Current得到的是DispatcherSynchronizationContext
     * 在winform下获取得到的是WindowsFormsSynchronizationContext*/


    /// <summary>
    /// 框架跨线程操作基础，包括集合类
    /// </summary>
    public static class SynchronizationHelper
    {
        public static InvokeContextAwaiter GetAwaiter(this InvokeContext context)
        {
            return new InvokeContextAwaiter(context);
        }
        
        public static InvokeContext InvokeContext { get; private set; }
        
        [Obsolete("Use InvokeContext")]
        public static SynchronizationContext UiContext { get; private set; }

        public static SynchronizationContextAwaiter GetAwaiter(this SynchronizationContext context)
        {
            return new SynchronizationContextAwaiter(context);
        }
        
        public static TaskScheduler FromUISynchronizationContext()
        {
            return new CustomSynchronizationContextTaskScheduler(UiContext);
        }

        public static DefaultTaskBasedActor<T> TaskBasedActor<T>(Action<T,CancellationToken> action)
        {
            return new DefaultTaskBasedActor<T>(new CustomSynchronizationContextTaskScheduler(UiContext),action);
        }
        
        /// <summary>
        /// 必须在STA主线程下初始化
        /// </summary>
        [STAThread]
        public static void Initialize()
        {
            if (UiContext != null)
                return;
            if (SynchronizationContext.Current == null)
            {
                SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            }

            var synchronizationContext = SynchronizationContext.Current;
            InvokeContext = new InvokeContext(Thread.CurrentThread.ManagedThreadId, synchronizationContext);
            UiContext = synchronizationContext;
        }

        public static void Invoke(Action<object> action, object state)
        {
            if (UiContext == null)
            {
                throw new NullReferenceException("未初始化同步帮助类");
            }

            UiContext.Send(o => action(o), state);
        }

        public static void InvokeAsync(Action<object> action, object state)
        {
            if (UiContext == null)
            {
                throw new NullReferenceException("未初始化同步帮助类");
            }

            UiContext.Post(o => action(o), state);
        }
    }
}