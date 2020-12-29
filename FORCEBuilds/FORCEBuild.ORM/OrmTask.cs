using System;
using System.Threading;

namespace FORCEBuild.Persistence
{
    internal class OrmTask:IDisposable
    {
        /// <summary>
        /// 根据不同类型决定是否异步
        /// </summary>
        public OrmTaskType TaskType { get; set; }

        public AutoResetEvent AutoResetEvent { get; set; }

        public Action MethodDelegate { get; set; }

        public dynamic ReturnValue { get; set; }

    //    public OrmMix OrmMix { get; set; }

        public OrmTask()
        {
            AutoResetEvent = new AutoResetEvent(false);
        }

        public void Dispose()
        {
            AutoResetEvent?.Set();
            AutoResetEvent?.Dispose();
        }
    }
}