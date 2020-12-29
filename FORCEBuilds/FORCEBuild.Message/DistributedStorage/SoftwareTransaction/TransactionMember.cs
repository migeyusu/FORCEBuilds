namespace FORCEBuild.Net.DistributedStorage.SoftwareTransaction
{
    public class TransactionMember
    {
        public TransactionMemberType MemberType { get; set; }

        public ITranscationProducer Producer { get; set; }
    }
}