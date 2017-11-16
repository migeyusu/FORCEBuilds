namespace FORCEBuild.DistributedStorage.Transaction
{
    public class TransactionMember
    {
        public TransactionMemberType MemberType { get; set; }

        public ITranscationProducer Producer { get; set; }
    }
}