using System;

namespace FORCEBuild.DistributedStorage.Transaction
{
    public class TransactionThreadContext : IDisposable
    {
        [ThreadStatic]
        private static TransactionThreadContext _instance;
        [ThreadStatic]
        private static int _instanceCounter;
        /// <summary>
        /// 当前会话
        /// </summary>
        public ITransaction Transaction { get; set; }
        
        public static TransactionThreadContext Current
        {
            get {
                if (_instanceCounter == 0)
                {
                    _instance = new TransactionThreadContext {Transaction = new ContextTransaction()};
                }
                _instanceCounter++;
                return _instance;
            }
        }

        private static void Release()
        {
            _instanceCounter--;
            if (_instanceCounter != 0) return;
            _instance?.Dispose();
            _instance = null;
        }

        public void Dispose()
        {
            Release();
        }

    }
    
}