using System;

namespace FORCEBuild.Windows.PInvoke
{
    public struct WindowInfo
    {
        public string Title { get; set; }
        public IntPtr Handle { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}