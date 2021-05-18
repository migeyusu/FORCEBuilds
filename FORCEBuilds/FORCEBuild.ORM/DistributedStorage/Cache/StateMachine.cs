using System;
using System.Collections.Concurrent;

namespace FORCEBuild.Persistence.DistributedStorage.Cache
{
    internal class CacheStateMachine
    {
        private static CacheStateMachine _machine;

        private static ConcurrentDictionary<MESIStatus, ConcurrentDictionary<OperationType, Func<int, object>>> StatesTransition;

        public static CacheStateMachine Instance {
            get {
                _machine = new CacheStateMachine();
                return _machine;
            }
        }
    }
}