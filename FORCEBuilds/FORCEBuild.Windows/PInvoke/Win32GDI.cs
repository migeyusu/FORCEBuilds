using System;
using System.Runtime.InteropServices;

namespace FORCEBuild.Windows.PInvoke
{
    public class Win32GDI
    {
        /// <summary>
        /// 选择一对象到指定的设备上下文环境
        /// </summary>
        /// <param name="hdc">handle to DC</param>
        /// <param name="hgdiobj">handle to object</param>
        /// <returns></returns>
        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
        /// <summary>
        /// 删除指定的设备上下文环境
        /// </summary>
        /// <param name="hdc">handle to DC</param>
        /// <returns></returns>
        [DllImport("gdi32.dll")]
        public static extern int DeleteDC(IntPtr hdc);
        /// <summary>
        /// 为指定设备创建上下文驱动环境
        /// </summary>
        /// <param name="lpszDriver">driver name驱动名</param>
        /// <param name="lpszDevice">device name设备名</param>
        /// <param name="lpszOutput">not used; should be NULL</param>
        /// <param name="lpInitData">optional printer data</param>
        /// <returns></returns>
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);
        /// <summary>
        /// 创建一个与指定设备兼容的内存设备上下文环境
        /// </summary>
        /// <param name="hdc">handle to DC</param>
        /// <returns></returns>
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        /// <summary>
        /// 创建与指定的设备环境相关的设备兼容的位图
        /// </summary>
        /// <param name="hdc">handle to DC</param>
        /// <param name="nWidth">width of bitmap, in pixels</param>
        /// <param name="nHeight">height of bitmap, in pixels</param>
        /// <returns></returns>
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
        /// <summary>
        /// 传送位图到指定位置
        /// </summary>
        /// <param name="hdc"></param>
        /// <param name="nXDest"></param>
        /// <param name="nYDest"></param>
        /// <param name="nWidth"></param>
        /// <param name="nHeight"></param>
        /// <param name="hdcSrc"></param>
        /// <param name="nXSrc"></param>
        /// <param name="nYSrc"></param>
        /// <param name="dwRop"></param>
        /// <returns></returns>
        [DllImport("gdi32.dll", EntryPoint = "BitBlt", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BitBlt([In] IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, [In] IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);
        [DllImport("gdi32.dll")]
        public static extern bool LineTo(IntPtr hdc, int nXEnd, int nYEnd);

        [DllImport("gdi32.dll")]
        public static extern bool Rectangle(IntPtr hdc, int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

        [DllImport("user32.dll")]
        public static extern int FrameRect(IntPtr hdc, [In] ref Rect lprc, IntPtr hbr);

        [DllImport("user32.dll")]
        public static extern int FillRect(IntPtr hDC, [In] ref Rect lprc, IntPtr hbr);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LPPAINTSTRUCT
    {
        /// <summary>
        /// 设备描述表句柄
        /// </summary>
        public IntPtr hdc;
        /// <summary>
        /// 擦除状态
        /// </summary>
        public bool fErase;
        /// <summary>
        /// 无效矩形坐标
        /// </summary>
        public Rect rcPaint;
        public bool fRestore;
        public bool fIncUpdate;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] rgbReserved;
    }

    public struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
        public int Width => Right - Left;
        public int Height => Bottom - Top;
    }

    /// <summary>
    ///     Specifies a raster-operation code. These codes define how the color data for the
    ///     source rectangle is to be combined with the color data for the destination
    ///     rectangle to achieve the final color.
    /// </summary>
    public  enum TernaryRasterOperations : uint
    {
        /// <summary>dest = source</summary>
        SRCCOPY = 0x00CC0020,
        /// <summary>dest = source OR dest</summary>
        SRCPAINT = 0x00EE0086,
        /// <summary>dest = source AND dest</summary>
        SRCAND = 0x008800C6,
        /// <summary>dest = source XOR dest</summary>
        SRCINVERT = 0x00660046,
        /// <summary>dest = source AND (NOT dest)</summary>
        SRCERASE = 0x00440328,
        /// <summary>dest = (NOT source)</summary>
        NOTSRCCOPY = 0x00330008,
        /// <summary>dest = (NOT src) AND (NOT dest)</summary>
        NOTSRCERASE = 0x001100A6,
        /// <summary>dest = (source AND pattern)</summary>
        MERGECOPY = 0x00C000CA,
        /// <summary>dest = (NOT source) OR dest</summary>
        MERGEPAINT = 0x00BB0226,
        /// <summary>dest = pattern</summary>
        PATCOPY = 0x00F00021,
        /// <summary>dest = DPSnoo</summary>
        PATPAINT = 0x00FB0A09,
        /// <summary>dest = pattern XOR dest</summary>
        PATINVERT = 0x005A0049,
        /// <summary>dest = (NOT dest)</summary>
        DSTINVERT = 0x00550009,
        /// <summary>dest = BLACK</summary>
        BLACKNESS = 0x00000042,
        /// <summary>dest = WHITE</summary>
        WHITENESS = 0x00FF0062,
        /// <summary>
        /// Capture window as seen on screen.  This includes layered windows 
        /// such as WPF windows with AllowsTransparency="true"
        /// </summary>
        CAPTUREBLT = 0x40000000
    }

}
