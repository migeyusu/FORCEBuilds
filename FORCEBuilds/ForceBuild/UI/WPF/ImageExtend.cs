using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace FORCEBuild.UI.WPF
{
    public static class ImageExtend
    {
        public static BitmapSource ToBitmapSource(this Bitmap image)
        {
            return Imaging.CreateBitmapSourceFromHBitmap(image.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }
    }
}