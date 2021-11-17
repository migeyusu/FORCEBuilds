using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace FORCEBuild.Concurrency
{
    public struct SynchronizationContextAwaiter : INotifyCompletion
    {
        private static readonly SendOrPostCallback _postCallback = state => ((Action)state)();

        private readonly SynchronizationContext _context;
        public SynchronizationContextAwaiter(SynchronizationContext context)
        {
            _context = context;
        }

        public bool IsCompleted
        {
            get
            {
                return _context == SynchronizationContext.Current;
            }
        }

        public void OnCompleted(Action continuation)
        {
            _context.Post(_postCallback, continuation);
        }

        public void GetResult() { }
    }
}