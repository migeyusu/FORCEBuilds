using System;
using System.Runtime.InteropServices;

namespace FORCEBuild.Windows.PInvoke
{
    /// <summary>
    /// 窗体同绘制有关操作
    /// </summary>
    public class Win32GDIWindow
    {
        [DllImport("user32.dll")]
        public static extern IntPtr BeginPaint(IntPtr hWnd, out LPPAINTSTRUCT lpPaint);
        [DllImport("user32.dll")]
        public static extern IntPtr EndPaint(IntPtr hWnd, ref LPPAINTSTRUCT lpPaint);
        /// <summary>
        /// 复制可视化的窗体到一个特定的设备上下文,无法获取d3d窗口
        /// </summary>
        /// <param name="hwnd">Window to copy,Handle to the window that will be copied. </param>
        /// <param name="hdcBlt">HDC to print into,Handle to the device context. </param>
        /// <param name="nFlags">Optional flags,Specifies the drawing options. It can be one of the following values</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, UInt32 nFlags);
        /// <summary>
        /// 刷新窗体
        /// </summary>
        /// <param name="hwnd"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool UpdateWindow(IntPtr hwnd);
        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);
        /// <summary>
        /// 取得包括标题栏的整个窗口的绘图句柄
        /// </summary>
        /// <param name="hwnd"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hwnd);
        /// <summary>
        /// 必须和getdc成对出现
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="hDC"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
       public  static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);
        /// <summary>Deletes a logical pen, brush, font, bitmap, region, or palette, freeing all system resources associated with the object. After the object is deleted, the specified handle is no longer valid.</summary>
        /// <param name="hObject">A handle to a logical pen, brush, font, bitmap, region, or palette.</param>
        /// <returns>
        ///   <para>If the function succeeds, the return value is nonzero.</para>
        ///   <para>If the specified handle is not valid or is currently selected into a DC, the return value is zero.</para>
        /// </returns>
        /// <remarks>
        ///   <para>Do not delete a drawing object (pen or brush) while it is still selected into a DC.</para>
        ///   <para>When a pattern brush is deleted, the bitmap associated with the brush is not deleted. The bitmap must be deleted independently.</para>
        /// </remarks>
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        [DllImport("dwmapi.dll")]
        public static extern int DwmRegisterThumbnail(IntPtr dest, IntPtr src, out IntPtr thumb);

        [DllImport("dwmapi.dll")]
        public static extern int DwmUnregisterThumbnail(IntPtr thumb);

        [StructLayout(LayoutKind.Sequential)]
        public struct PSIZE
        {
            public int x;
            public int y;
        }

        [DllImport("dwmapi.dll")]
        public static extern int DwmQueryThumbnailSourceSize(IntPtr thumb, out PSIZE size);

        [StructLayout(LayoutKind.Sequential)]
        public struct DWM_THUMBNAIL_PROPERTIES
        {
            public int dwFlags;
            public Rect rcDestination;
            public Rect rcSource;
            public byte opacity;
            public bool fVisible;
            public bool fSourceClientAreaOnly;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public Rect(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("dwmapi.dll")]
        public static extern int DwmUpdateThumbnailProperties(IntPtr hThumb, ref DWM_THUMBNAIL_PROPERTIES props);
    }
}
