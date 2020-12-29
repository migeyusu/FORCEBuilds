namespace FORCEBuild.Persistence.DistributedStorage.Cache
{
    public enum OperationType : byte
    {
        LocalRead = 0,
        LocalWrite = 1,
        RemoteRead = 2,
        RemoteWrite = 3,
    }
}