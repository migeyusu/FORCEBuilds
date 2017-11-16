using System.Collections.Generic;

namespace FORCEBuild.DistributedStorage.Transaction
{
    /// <summary>
    /// 
    /// </summary>
    public class ContextTransaction:ITransaction
    {
        /// <summary>
        /// 运行在单线程环境下,不需要锁机制
        /// </summary>
        public Dictionary<ITranscationProducer,TransactionMember> MembersDictionary { get; set; }

        private List<TransactionMember> members;
        private bool _isInTransaction;

        public bool IsInTransaction {
            get { return _isInTransaction; }
            set {
                _isInTransaction = value;
                if (value) return;
                members.Clear();
                MembersDictionary.Clear();
            }
        }

        ///// <summary>
        ///// 由首个producers提交
        ///// </summary>
        //public IPersistence Persistence { get; set; }

        public ContextTransaction()
        {
            MembersDictionary = new Dictionary<ITranscationProducer, TransactionMember>();
            
        }

        public void Register(ITranscationProducer producer,TransactionMemberType type=TransactionMemberType.UPDATE)
        {
            if (MembersDictionary.ContainsKey(producer)) {

                var member = MembersDictionary[producer];
                members.Remove(member);
                members.Add(member);
            }
            else {
                var member = new TransactionMember()
                {
                    MemberType = type,
                    Producer = producer
                };
                members.Add(member);
                MembersDictionary.Add(producer,member);
            }
            
        }
        

        public void Commit()
        {
            if (members.Count==0) {
                return;
            }
           // members[0].Producer.Storeage.Enqueue(members);
        }

        public void Rollback()
        {
            foreach (var producer in members) {
                producer.Producer.Rollback();
            }
        }

        
    }
}