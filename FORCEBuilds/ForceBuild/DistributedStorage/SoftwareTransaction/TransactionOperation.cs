namespace FORCEBuild.DistributedStorage.Transaction
{
    public class TransactionOperation
    {
        public int ORMID { get; set; }

        public string PropertyName { get; set; }

        public dynamic Value { get; set; }
        
        public OperationType OperationType { get; set; }
    }
}