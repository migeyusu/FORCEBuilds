using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace FORCEBuild.Concurrency
{
    public struct InvokeContextAwaiter: INotifyCompletion
    {
        private InvokeContext _context;

        private static readonly SendOrPostCallback _postCallback = state => ((Action)state)();
        public InvokeContextAwaiter(InvokeContext context)
        {
            _context = context;
        }

        public bool IsCompleted
        {
            get { return Thread.CurrentThread.ManagedThreadId == _context.ThreadID; }
        }
        public void OnCompleted(Action continuation)
        {
            _context.SynchronizationContext.Post(_postCallback,continuation);
        }
        
        public void GetResult() { }
    }
}