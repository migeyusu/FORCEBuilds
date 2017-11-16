using System;

namespace FORCEBuild.PInvoke
{
    [Flags]
    public enum WinMsg : int
    {
        WM_PAINT = 0x0F,
        WM_PRINT = 0x0317,
    }
}
