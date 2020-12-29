using System;
using System.Collections.Concurrent;

namespace FORCEBuild.Net.DistributedStorage.Cache
{
    public class StateMachine
    {
        private static StateMachine machine;

        private static ConcurrentDictionary<MESIStatus, ConcurrentDictionary<OperationType, Func<int, object>>> StatesTransition;

        public static StateMachine Instance {
            get {
                machine = new StateMachine();
                return machine;
            }
        }

        //private StateMachine()
        //{
        //    StatesTransition = new ConcurrentDictionary<MESIStatus, ConcurrentDictionary<OperationType, Func<int, object>>>();
        //    foreach (var value in Enum.GetValues(MESIStatus)) {
                
        //    }
        //}

        //public MESIStatus GetNext()
        //{
            
        //}
    }
}