using System;

namespace FORCEBuild.Windows.PInvoke
{
    [Flags]
    public enum AllocationTypes:uint
    {
        Commit = 0x1000,
        Reserve = 0x2000,
        Reset = 0x80000,
        LargePages = 0x20000000,
        Physical = 0x400000,
        TopDown = 0x100000,
        WriteWatch = 0x200000
    }


}