using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace FORCEBuild.PInvoke
{
    /// <summary>
    /// 设备上下文
    /// </summary>
    public class Library
    {
        static readonly List<Bitmap> collect = new List<Bitmap>();
        /*      
        平方差匹配      =0,
        标准平方差匹配  =1,
        相关匹配        =2,
        标准相关匹配    =3,
        相关匹配        =4,
        标准相关匹配    =5
        标准相关匹配准确度最高。
        */
        /// <summary>
        /// 模板匹配
        /// </summary>
        /// <param name="temppath">模板地址</param>
        /// <param name="sourcepath">被匹配地址</param>
        /// <param name="pt">匹配位置</param>
        /// <param name="method">匹配方式</param>
        /// <param name="errorcode"></param>
        /// <returns></returns>
        [DllImport("ImgDeal.dll")]
        public static extern bool GetTempletPosition(string temppath, string sourcepath, ref POINTAPI pt, int method, ref int errorcode);
        [DllImport("ImgDeal.dll")]
        public static extern void GetGrayColorHistogram(byte[] sourceimg, int len, float[] histogram, int pass);
        [DllImport("ImgDeal.dll")]
        public static extern float GetCorrelationCoefficient(float[] corA, float[] corB, int len);
        [DllImport("ImgDeal.dll")]
        public static extern float GetImgVariance(IntPtr ip0, IntPtr ip1, int len);
        /// <summary>
        /// 感知哈希指纹，默认输入rgba四色,哈希吗长度64
        /// </summary>
        /// <param name="img"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="hashcode"></param>
        [DllImport("ImgDeal.dll")]
        public static extern void PHash(IntPtr img, int width, int height, byte[] hashcode);
        /// <summary>
        /// 找出两个哈希码区别程度，数组长64
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <returns></returns>
        [DllImport("ImgDeal.dll")]
        public static extern int HanmingDistance(byte[] str1, byte[] str2);
        /// <summary>
        /// unicode编码
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public static string GetStreamHash(byte[] datas)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            var outdatas = md5.ComputeHash(datas);
            return Encoding.Unicode.GetString(outdatas);
        }
        /// <summary>
        /// 返回一个hdc环境句柄和图片句柄
        /// </summary>
        /// <param name="hwnd">窗口句柄</param>
        /// <param name="hdcMem">hdc句柄，需要手动清除</param>
        /// <returns></returns>
        [DllImport("ScreenCapture.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr InnerPaintWindowsCapture(IntPtr hwnd,out IntPtr hdcMem);

        public static Bitmap GetWindowBitmap(IntPtr hwnd)
        {
            var hdc = Win32GDIWindow.BeginPaint(hwnd, out LPPAINTSTRUCT lps);
            var rec = new Rect();
            Win32Hand.GetWindowRect(hwnd, out rec);
            // IntPtr hdc = WindowExternal.GetDC(hwnd);
            var hdcMem = Win32GDI.CreateCompatibleDC(hdc);
            var hbitmap = Win32GDI.CreateCompatibleBitmap(hdc, rec.Width, rec.Height);
            Win32GDIWindow.ReleaseDC(hwnd, hdc);
            Win32GDI.SelectObject(hdcMem, hbitmap);
            //可能造成窗体刷新失败
            Win32GDIWindow.PrintWindow(hwnd, hdcMem, 0);
            //开启dwm后可以BitBlt获得离屏表面
            //GDIExternal.BitBlt(hdcMem, 0, 0, rec.Width, rec.Height,
            //    hdc, 0, 0, TernaryRasterOperations.SRCCOPY);
            var bmp = Image.FromHbitmap(hbitmap);
            Win32GDI.DeleteDC(hdcMem);
            Win32GDIWindow.DeleteObject(hbitmap);
            lps.rcPaint = new Rect { Top = 0, Left = 0, Bottom = rec.Height, Right = rec.Width };
            Win32GDIWindow.EndPaint(hwnd,ref lps);
            #region 预处理
            //IntPtr hdcMem;
            //IntPtr ip = InnerPaintWindowsCapture(hwnd,out hdcMem);
            //Bitmap bmp = Image.FromHbitmap(ip);
            //WindowExternal.DeleteObject(ip);
            //GDIExternal.DeleteDC(hdcMem);
            #endregion
            //if (collect.Count<50)
            //{
            //    collect.Add(bmp);
            //} 
            return bmp;
        }
        public static Bitmap GetWindowBitmap(IntPtr hwnd,Rect rect)
        {
            var hdc = Win32GDIWindow.BeginPaint(hwnd, out LPPAINTSTRUCT lps);
            var hdcMem = Win32GDI.CreateCompatibleDC(hdc);
            var hbitmap = Win32GDI.CreateCompatibleBitmap(hdc, rect.Width, rect.Height);
            Win32GDIWindow.ReleaseDC(hwnd, hdc);
            Win32GDI.SelectObject(hdcMem, hbitmap);
            Win32GDIWindow.PrintWindow(hwnd, hdcMem, 0);
            var bmp = Image.FromHbitmap(hbitmap);
            Win32GDI.DeleteDC(hdcMem);
            Win32GDIWindow.DeleteObject(hbitmap);
            lps.rcPaint = new Rect { Top = 0, Left = 0, Bottom = rect.Height, Right = rect.Width };
            Win32GDIWindow.EndPaint(hwnd, ref lps);
            return bmp;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hwnd"></param>
        /// <returns></returns>
        public static byte[] WindowPHash(IntPtr hwnd)
        {
            var bmp = GetWindowBitmap(hwnd);
            var ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Bmp);
            var ip = Marshal.UnsafeAddrOfPinnedArrayElement(ms.ToArray(), 54);
            var hashcode = new byte[64];
            PHash(ip, bmp.Width, bmp.Height, hashcode);
            return hashcode;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rec">相对窗体的位置</param>
        /// <returns></returns>
        public static string WindowMd5Hash(Rectangle rec,IntPtr hwnd)
        {
            var bmp = GetWindowBitmap(hwnd);
            var ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Bmp);
            var datas = ms.ToArray();
            return GetStreamHash(datas);
        }

        public static byte[] ImgPHash(Image img)
        {
            var ms = new MemoryStream();
            img.Save(ms, ImageFormat.Bmp);
            var datas = ms.ToArray();
            var ip = Marshal.UnsafeAddrOfPinnedArrayElement(datas, 54);
            var phash = new byte[64];
            PHash(ip, img.Width, img.Height, phash);
            return phash;
        }

        public static string ImgMd5Hash(Image img)
        {
            var ms = new MemoryStream();
            img.Save(ms, ImageFormat.Bmp);
            var datas = ms.ToArray();
            return GetStreamHash(datas);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="img"></param>
        /// <param name="rec">要绘制的img的区域</param>
        /// <returns></returns>
        public static string ImgMd5Hash(Image img, Rectangle rec)
        {
            var bmp = new Bitmap(rec.Width, rec.Height);
            Graphics.FromImage(bmp).DrawImage(img, new Rectangle(new Point(0, 0), bmp.Size), rec, GraphicsUnit.Pixel);
         //   bmp.Save(Environment.CurrentDirectory + "\\a.bmp", ImageFormat.Bmp);
            return ImgMd5Hash(bmp);
        }
        public static void SaveImg()
        {
            var i = 0;
            foreach (var item in collect)
            {
                item.Save(Application.StartupPath + "\\" + i + ".bmp");
                i++;
            }
        }

       
    }
}
/// <summary>
/// 直接截取屏幕指定大小
/// </summary>
/// <param name="rec"></param>
//public static byte[] ScreenPHash(Rectangle rec)
//{
//    byte[] datas = GetScreenByteArray(rec);
//    IntPtr ip = Marshal.UnsafeAddrOfPinnedArrayElement(datas, 54);
//    byte[] hashcode = new byte[64];
//    PHash(ip, rec.Width, rec.Height, hashcode);
//    return hashcode;
//}

//public static byte[] GetScreenByteArray(Rectangle rec)
//{
//    Bitmap bmp = new Bitmap(rec.Width, rec.Height);
//    Graphics.FromImage(bmp).CopyFromScreen(rec.Location, new Point(0, 0), rec.Size);
//    MemoryStream ms = new MemoryStream();
//    bmp.Save(ms, ImageFormat.Bmp);
//    return ms.ToArray();
//}
/// <summary>
/// 
/// </summary>
/// <param name="rec">相对屏幕的位置</param>
/// <returns></returns>
//public static string ScreenMd5Hash(Rectangle rec)
//{
//    byte[] datas = GetScreenByteArray(rec);
//    return GetStreamHash(datas);
//}
// LPPAINTSTRUCT ps;
// Rect rec = new Rect();
// HandleExternal.GetWindowRect(hwnd, out rec);
// IntPtr hdc = WindowExternal.BeginPaint(hwnd, out ps);
//// IntPtr hdc = WindowExternal.GetDC(hwnd);
// IntPtr hdcMem = GDIExternal.CreateCompatibleDC(hdc);
// IntPtr hbitmap = GDIExternal.CreateCompatibleBitmap(hdc, rec.Width, rec.Height);
// WindowExternal.ReleaseDC(hwnd, hdc);
// GDIExternal.SelectObject(hdcMem, hbitmap);
// //可能造成窗体刷新失败
// WindowExternal.PrintWindow(hwnd, hdcMem, 0);
// //WindowExternal.UpdateWindow(hwnd);
// //当未开启dwn时，无法启用获取单个窗体截图
// //GDIExternal.BitBlt(hdcMem, 0, 0, rec.Width, rec.Height,
// //    hdc, 0, 0, TernaryRasterOperations.SRCCOPY);
// Bitmap bmp = Image.FromHbitmap(hbitmap);
// GDIExternal.DeleteDC(hdcMem);
// WindowExternal.DeleteObject(hbitmap);
// ps.rcPaint = new Rect() { Top = 0, Left = 0, Bottom = rec.Height, Right = rec.Width };
// WindowExternal.EndPaint(hwnd, ref ps);
/// <summary>
/// win api获取窗体截图
/// </summary>
/// <param name="hwnd"></param>
/// <returns></returns>
//public static Bitmap GetWindowBitmap(IntPtr hwnd)
//{
//    IntPtr hscrdc = WindowExternal.GetDC(hwnd);
//    Rect rec = new Rect();
//    HandleExternal.GetWindowRect(hwnd, out rec);
//    IntPtr hbitmap = GDIExternal.CreateCompatibleBitmap(hscrdc, rec.Right - rec.Left, rec.Bottom - rec.Top);
//    IntPtr hmemdc = GDIExternal.CreateCompatibleDC(hscrdc);
//    GDIExternal.SelectObject(hmemdc, hbitmap);
//    WindowExternal.PrintWindow(hwnd, hmemdc, 0);
//    // External.SendMessage(hwnd, WinMsg.WM_PAINT, hmemdc.ToInt32(), 0);
//    Bitmap bmp = Image.FromHbitmap(hbitmap);
//    GDIExternal.DeleteDC(hscrdc);//删除用过的对象
//    GDIExternal.DeleteDC(hmemdc);//删除用过的对象
//    GDIExternal.DeleteDC(hbitmap);
//    return bmp;
//}