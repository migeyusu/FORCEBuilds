using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace FORCEBuild.Concurrency
{
    public abstract class TaskBasedActor<T>
    {
        private readonly TaskScheduler _scheduler;
        private readonly TaskFactory _taskFactory;
        
        protected TaskBasedActor(TaskScheduler scheduler)
        {
            _scheduler = scheduler;
            this._taskFactory = new TaskFactory(scheduler);
        }

        protected abstract Task ReceiveMessage(T message, CancellationToken token);

        /// <summary>
        /// While taskfactory.TryExecuteTask is a low level api which can't use async await
        /// I tried a lot of approaches but failed! Though it's not elegant, but it works.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <param name="completionSource"></param>
        public void Wrapper(T message, CancellationToken token)
        {
            Task.Run(async () =>
            {
                await ReceiveMessage(message, token);
            }, token).Wait(token);
        }
        
        public Task PostAsync(T message, CancellationToken token = default)
        {
            return _taskFactory.StartNew(() =>
            {
                Wrapper(message,token);
            }, token);
        }

        /// <summary>
        /// 将接收不到异常，并且无法wait
        /// </summary>
        public void Post(T message)
        {
            new Task(() =>
            {
                Wrapper(message,CancellationToken.None);
            }).RunSynchronously(_scheduler);
        }
    }
    
}