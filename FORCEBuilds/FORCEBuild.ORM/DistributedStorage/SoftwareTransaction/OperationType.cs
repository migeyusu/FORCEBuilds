namespace FORCEBuild.Persistence.DistributedStorage.SoftwareTransaction
{
    public enum OperationType:byte
    {
        //
        // 摘要:
        //     One or more items were added to the collection.
        Add = 0,

        //
        // 摘要:
        //     One or more items were removed from the collection.
        Remove = 1,

        //
        // 摘要:
        //     One or more items were replaced in the collection.
        Replace = 2,

        //
        // 摘要:
        //     One or more items were moved within the collection.
        Move = 3,

        //
        // 摘要:
        //     The content of the collection changed dramatically.
        Reset = 4,

        //
        // 摘要:
        //     当前属性发生改变.
        Set = 5,
    }
}