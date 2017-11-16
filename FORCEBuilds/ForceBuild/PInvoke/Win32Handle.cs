using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace FORCEBuild.PInvoke
{
    [StructLayout(LayoutKind.Sequential)]
    public struct POINTAPI
    {
        public int x;
        public int y;
    }
    /// <summary>
    /// 句柄互操作，窗体信息获取
    /// </summary>
    public class Win32Hand
    {
        /// <summary>
        /// 指示句柄对应的窗口是否存在
        /// </summary>
        /// <param name="hwnd"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool IsWindow(IntPtr hwnd);
        /// <summary>
        /// 获取鼠标坐标
        /// </summary>
        /// <param name="lpPoint"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern int GetCursorPos(ref POINTAPI lpPoint);
        /// <summary>
        /// 指定坐标处窗口句柄
        /// </summary>
        /// <param name="xPoint"></param>
        /// <param name="yPoint"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern int WindowFromPoint(int xPoint, int yPoint);
        /// <summary>
        /// 得到窗体标题名
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="lpString"></param>
        /// <param name="nMaxCount"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern int GetWindowText(int hWnd, StringBuilder lpString, int nMaxCount);
        /// <summary>
        /// 得到窗体类名
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="lpString"></param>
        /// <param name="nMaxCount"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern int GetClassName(int hWnd, StringBuilder lpString, int nMaxCount);
        /// <summary>
        /// 根据窗体名或类名找到窗体
        /// </summary>
        /// <param name="lpClassName"></param>
        /// <param name="lpWindowName"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll")]
        public static extern int GetWindowRect(IntPtr hwnd, out Rect lpRect);
        /// <summary>
        /// 返回任务子窗口的窗口或控件的ID值，并不仅是对话框的控件。
        /// 由于顶层窗口没有ID值，因此如果CWnd是一个顶层窗口，则这个函数返回值是没有意义的。
        /// </summary>
        /// <param name="hwnd"></param>
        /// <returns>返回CWnd子窗口的整数标识符;否则返回0。</returns>
        [DllImport("user32.dll")]
        public static extern int GetDlgCtrlID(IntPtr hwnd);
        /// <summary>
        /// 返回桌面窗口的句柄
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetDesktopWindow();
        /// <summary>
        /// 返回与指定窗口有特定关系（如Z序或所有者）的窗口句柄
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="uCmd"></param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindow(IntPtr hWnd, GetWindowCmd uCmd);
        /// <summary>
        /// 根据窗口句柄找到进程id
        /// </summary>
        /// <param name="hwnd">窗口句柄</param>
        /// <param name="ID">进程id</param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);
        /// <summary>
        /// 获得当前线程激活的窗体，如果当前线程无消息，总是返回0
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetActiveWindow();
        /// <summary>
        /// 前台窗体，如果qq一直最前显示，另外窗体被激活后会获得被激活窗体的句柄
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern bool BringWindowToTop(IntPtr hwnd);
        /// <summary>
        /// 窗体是否最大化
        /// </summary>
        /// <param name="hwnd"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern int IsZoomed(IntPtr hwnd);
        /// <summary>
        /// 向窗体发送命令
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="wMsg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hwnd, WinMsg wMsg, IntPtr wParam, int lParam);
        /// <summary>
        /// 窗体是否最小化
        /// </summary>
        /// <param name="hwnd"></param>
        /// <returns>返回1代表最小化，0表示依然窗口化</returns>
        [DllImport("user32.dll")]
        public static extern int IsIconic(IntPtr hwnd);

        public const int GWL_EXSTYLE = -20;//获得扩展窗口风格。
        public const int GWL_HINSTANCE = -6; //获得应用实例的句柄。
        public const int GWL_HWNDPARENT = -8;// 如果父窗口存在，获得父窗口句柄。
        public const int GWL_ID = -12;// 获得窗口标识。
        public const int GWL_STYLE = -16;// 获得窗口风格。
        public const int GWL_USERDATA = -21;// 获得与窗口有关的32位值。每一个窗口均有一个由创建该窗口的应用程序使用的32位值。
        public const int GWL_WNDPROC = -4;// 获得窗口过程的地址，或代表窗口过程的地址的句柄。必须使用CallWindowProc函数调用窗口过程。
        /// <summary>
        /// 获取窗体信息
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="nindex">nIndex取值：意义</param>
        /// <returns>欲取回的信息</returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hwnd, int nindex);

        public static readonly ulong WS_VISIBLE = 0x10000000L;
        public static readonly ulong WS_BORDER = 0x00800000L;
        public static readonly ulong TARGETWINDOW = WS_BORDER | WS_VISIBLE;
        [DllImport("user32.dll")]
        public static extern ulong GetWindowLongA(IntPtr hWnd, int nIndex);
        /// <summary>
        /// 对窗体移动及调整位置
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="hWndInsertAfter">指令</param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="cx"></param>
        /// <param name="cy"></param>
        /// <param name="uFlags"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hwnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        public static readonly IntPtr HWND_TOP = new IntPtr(0);
        public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        public static void SetWindowTopMost(IntPtr hwnd)
        {
            SetWindowPos(hwnd, HWND_TOPMOST, -1, -1, -1, -1,
                SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE);
        }
        public static string GetProcessNameByHwnd(IntPtr hand)
        {
            GetWindowThreadProcessId(hand, out int id);
            return Process.GetProcessById(id).ProcessName;
        }


        public delegate bool EnumWindowsCallback(IntPtr hwnd, int lParam);

        [DllImport("user32.dll")]
        public static extern int EnumWindows(EnumWindowsCallback lpEnumFunc, int lParam);

        /// <summary>
        /// 以inptr.zero（桌面）为父窗口，找到所有子窗口
        /// </summary>
        /// <returns></returns>
        public static List<WindowInfo> GetAllShownWindows()
        {
            var list = new List<WindowInfo>();
            EnumWindows((hwnd, param) =>
            {
                if ((GetWindowLongA(hwnd,GWL_STYLE)&TARGETWINDOW)==TARGETWINDOW)
                {
                    var sb=new StringBuilder(200);
                    GetWindowText(hwnd.ToInt32(), sb, sb.Capacity);
                    var info=new WindowInfo {
                        Handle = hwnd,
                        Title = sb.ToString()
                    };
                    list.Add(info);
                }
                return true;
            }, 0);
            return list;
        }

        public static List<WindowInfo> GetPrcessWindows(int pid)
        {
            var allWindows = GetAllShownWindows();
            var select = new List<WindowInfo>();
            foreach (var info in allWindows)
            {
                GetWindowThreadProcessId(info.Handle, out int cpid);
                if (cpid == pid)
                    select.Add(info);
            }
            return select;
        }
    }

    [Flags]
    public enum SetWindowPosFlags : uint
    {
        /// <summary>
        ///     If the calling thread and the thread that owns the window are attached to different input queues, the system posts the request to the thread that owns the window. This prevents the calling thread from blocking its execution while other threads process the request.
        /// </summary>
        SWP_ASYNCWINDOWPOS = 0x4000,
        /// <summary>
        ///     Prevents generation of the WM_SYNCPAINT message.
        /// </summary>
        SWP_DEFERERASE = 0x2000,
        /// <summary>
        ///     Draws a frame (defined in the window's class description) around the window.
        /// </summary>
        SWP_DRAWFRAME = 0x0020,
        /// <summary>
        ///     Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message to the window, even if the window's size is not being changed. If this flag is not specified, WM_NCCALCSIZE is sent only when the window's size is being changed.
        /// </summary>
        SWP_FRAMECHANGED = 0x0020,
        /// <summary>
        ///     Hides the window.
        /// </summary>
        SWP_HIDEWINDOW = 0x0080,
        /// <summary>
        ///     Does not activate the window. If this flag is not set, the window is activated and moved to the top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter parameter).
        /// </summary>
        SWP_NOACTIVATE = 0x0010,

        /// <summary>
        ///     Discards the entire contents of the client area. If this flag is not specified, the valid contents of the client area are saved and copied back into the client area after the window is sized or repositioned.
        /// </summary>
        SWP_NOCOPYBITS = 0x0100,

        /// <summary>
        ///     Retains the current position (ignores X and Y parameters).
        /// </summary>
        SWP_NOMOVE = 0x0002,

        /// <summary>
        ///     Does not change the owner window's position in the Z order.
        /// </summary>
        SWP_NOOWNERZORDER = 0x0200,

        /// <summary>
        ///     Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to the client area, the nonclient area (including the title bar and scroll bars), and any part of the parent window uncovered as a result of the window being moved. When this flag is set, the application must explicitly invalidate or redraw any parts of the window and parent window that need redrawing.
        /// </summary>
        SWP_NOREDRAW = 0x0008,

        /// <summary>
        ///     Same as the SWP_NOOWNERZORDER flag.
        /// </summary>
        SWP_NOREPOSITION = 0x0200,

        /// <summary>
        ///     Prevents the window from receiving the WM_WINDOWPOSCHANGING message.
        /// </summary>
        SWP_NOSENDCHANGING = 0x0400,

        /// <summary>
        ///     Retains the current size (ignores the cx and cy parameters).
        /// </summary>
        SWP_NOSIZE = 0x0001,

        /// <summary>
        ///     Retains the current Z order (ignores the hWndInsertAfter parameter).
        /// </summary>
        SWP_NOZORDER = 0x0004,

        /// <summary>
        ///     Displays the window.
        /// </summary>
        SWP_SHOWWINDOW = 0x0040,

        // ReSharper restore InconsistentNaming
    }
    //GW_HWNDFIRST = 0; {同级别第一个}  
    //GW_HWNDLAST  = 1; {同级别最后一个}  
    //GW_HWNDNEXT  = 2; {同级别下一个}  
    //GW_HWNDPREV  = 3; {同级别上一个}  
    //GW_OWNER     = 4; {属主窗口}  
    //GW_CHILD     = 5; {子窗口}  
    public enum GetWindowCmd : uint
    {
        GW_HWNDFIRST = 0,
        GW_HWNDLAST = 1,
        GW_HWNDNEXT = 2,
        GW_HWNDPREV = 3,
        GW_OWNER = 4,
        GW_CHILD = 5,
        GW_ENABLEDPOPUP = 6
    }



}
