namespace FORCEBuild.DistributedService
{
    public enum CallType : byte
    {
        Test = 1,
        Call = 3,
        Info = 4,
        /// <summary>
        /// 心跳
        /// </summary>
        Heart = 5,
    }
}
