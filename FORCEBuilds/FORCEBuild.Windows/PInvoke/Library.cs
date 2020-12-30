using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FORCEBuild.Windows.PInvoke
{
    /// <summary>
    /// 设备上下文
    /// </summary>
    public class Library
    {
        static readonly List<Bitmap> collect = new List<Bitmap>();

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
            lps.rcPaint = new Rect {Top = 0, Left = 0, Bottom = rec.Height, Right = rec.Width};
            Win32GDIWindow.EndPaint(hwnd, ref lps);

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

        public static Bitmap GetWindowBitmap(IntPtr hwnd, Rect rect)
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
            lps.rcPaint = new Rect {Top = 0, Left = 0, Bottom = rect.Height, Right = rect.Width};
            Win32GDIWindow.EndPaint(hwnd, ref lps);
            return bmp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rec">相对窗体的位置</param>
        /// <returns></returns>
        public static string WindowMd5Hash(Rectangle rec, IntPtr hwnd)
        {
            var bmp = GetWindowBitmap(hwnd);
            var ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Bmp);
            var datas = ms.ToArray();
            return GetStreamHash(datas);
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
    }
}