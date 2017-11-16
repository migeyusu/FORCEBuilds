using System;


namespace FORCEBuild.Windows.PInvoke
{
    [Flags]
    public enum FreeTypes:uint
    {
        Decommit = 0x4000,
        Release = 0x8000
    }


}