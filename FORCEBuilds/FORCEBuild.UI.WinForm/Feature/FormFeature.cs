using System;
using System.Runtime.InteropServices;

namespace FORCEBuild.UI.Winform.Feature
{
    public class FormFeature
    {
        const int CsDropShadow = 0x20000;
        const int GclStyle = -26;
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SetClassLong(IntPtr hwnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetClassLong(IntPtr hwnd, int nIndex); 

        public static void SetFormShadow(IntPtr handle)
        {
            SetClassLong(handle, GclStyle, GetClassLong(handle, GclStyle) | CsDropShadow);
        }
    }
}
