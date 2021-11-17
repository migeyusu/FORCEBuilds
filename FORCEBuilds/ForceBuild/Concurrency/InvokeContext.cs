using System.Threading;

namespace FORCEBuild.Concurrency
{
    /// <summary>
    /// safe ui context without check if on ui thread. 
    /// </summary>
    public class InvokeContext   
    {
        public SynchronizationContext SynchronizationContext { get;  }
        public int ThreadID { get;  }

        public InvokeContext(int threadId, SynchronizationContext synchronizationContext)
        {
            ThreadID = threadId;
            SynchronizationContext = synchronizationContext;
        }
    }
}