using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace FORCEBuild.UI.Winform.Feature
{
    /// <summary>
    /// win8下无法使用
    /// </summary>
    public static class AreoExtend
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Margins
        {
            public int left;
            public int right;
            public int top;
            public int buttom;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct DwmBlurbehind
        {
            public uint dwFlags;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fEnable;
            public IntPtr hRgnBlur;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fTransitionOnMaximized;
            public const uint DwmBbEnable = 0x01;
            public const uint DwmBbBlurregion = 0x02;
            public const uint DwmBbTransitiononmaximized = 0x04;
        }
        [DllImport("dwmapi.dll")]
        public static extern int DwmIsCompositionEnabled(ref int en);
        [DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref Margins margin);
        [DllImport("dwmapi.dll")]//, CharSet = CharSet.Auto, PreserveSig = false, CallingConvention = CallingConvention.Cdecl
        public static extern void DwmEnableBlurBehindWindow(IntPtr hWnd, ref DwmBlurbehind pBlurBehind);
        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern void DwmEnableComposition(bool bEnable);
        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern void DwmGetColorizationColor(out int pcrColorization, [MarshalAs(UnmanagedType.Bool)]out bool pfOpaqueBlend);
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        [DllImport("user32.dll")]
        public static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);
        [StructLayout(LayoutKind.Sequential)]
        public struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }
        public enum WindowCompositionAttribute
        {
            WcaUndefined = 0,
            WcaNcrenderingEnabled = 1,
            WcaNcrenderingPolicy = 2,
            WcaTransitionsForcedisabled = 3,
            WcaAllowNcpaint = 4,
            WcaCaptionButtonBounds = 5,
            WcaNonclientRtlLayout = 6,
            WcaForceIconicRepresentation = 7,
            WcaExtendedFrameBounds = 8,
            WcaHasIconicBitmap = 9,
            WcaThemeAttributes = 10,
            WcaNcrenderingExiled = 11,
            WcaNcadornmentinfo = 12,
            WcaExcludedFromLivepreview = 13,
            WcaVideoOverlayActive = 14,
            WcaForceActivewindowAppearance = 15,
            WcaDisallowPeek = 16,
            WcaCloak = 17,
            WcaCloaked = 18,
            WcaAccentPolicy = 19,
            WcaFreezeRepresentation = 20,
            WcaEverUncloaked = 21,
            WcaVisualOwner = 22,
            WcaLast = 23
        }
        public enum AccentState
        {
            AccentDisabled = 0,
            AccentEnableGradient = 1,
            AccentEnableTransparentgradient = 2,
            AccentEnableBlurbehind = 3,
            AccentInvalidState = 4
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }
        private static void Win10AreoEffectOn(IntPtr hwnd)
        {
            var ap = new AccentPolicy {
                AccentState = AccentState.AccentEnableBlurbehind,
                AccentFlags = 0,
                AnimationId = 0,
                GradientColor = 0
            };
            var size = Marshal.SizeOf(ap);
            var apptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(ap, apptr, false);
            var data = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAttribute.WcaAccentPolicy,
                SizeOfData = size,
                Data = apptr
            };
            SetWindowCompositionAttribute(hwnd, ref data);
            Marshal.FreeHGlobal(apptr);
        }

        private static void Win10AreoEffectOff(IntPtr hwnd)
        {
            var ap = new AccentPolicy {
                AccentState = AccentState.AccentDisabled,
                AccentFlags = 0,
                AnimationId = 0,
                GradientColor = 0
            };
            var size = Marshal.SizeOf(ap);
            var apptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(ap, apptr, false);
            var data = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAttribute.WcaAccentPolicy,
                SizeOfData = size,
                Data = apptr
            };
            SetWindowCompositionAttribute(hwnd, ref data);
            Marshal.FreeHGlobal(apptr);
        }

        public static void Win7AreoExtend(this Form form)
        {
            var hwnd = form.Handle;
            var en = 0;
            var x = -1;
            var mg = new Margins
            {
                buttom = x,
                left = x,
                top = x,
                right = x
            };
            DwmIsCompositionEnabled(ref en);
            if (en < 0)
                DwmEnableComposition(true);
            DwmExtendFrameIntoClientArea(hwnd, ref mg);
        }

        private static void Win7AreoEffectOn(IntPtr hwnd)
        {
            var db = new DwmBlurbehind
            {
                fEnable = true,
                hRgnBlur = IntPtr.Zero,
                fTransitionOnMaximized = false,
                dwFlags = 1,
            };
            DwmEnableBlurBehindWindow(hwnd, ref db);
        }

        private static void Win7AreoEffectOff(IntPtr hwnd)
        {
            var db = new DwmBlurbehind
            {
                fEnable = false,
                hRgnBlur = IntPtr.Zero,
                fTransitionOnMaximized = false,
                dwFlags = 1,
            };
            DwmEnableBlurBehindWindow(hwnd, ref db);
        }

        private static void InternalAreoEffectOn(IntPtr hwnd)
        {
            var os = Environment.OSVersion.Version;
            if (os.Major == 6)
            {
                switch (os.Minor)
                {
                    case 1:
                        Win7AreoEffectOn(hwnd);
                        return;
                    case 2:
                        Win10AreoEffectOn(hwnd);
                        return;
                }
            }
            else if (os.Major == 10 && os.Minor == 0)
            {

                Win10AreoEffectOn(hwnd);
                return;
            }
        }
        public static void AreoEffectOn(this Window window)
        {
            var hwnd=new WindowInteropHelper(window).Handle;
            InternalAreoEffectOn(hwnd);
        }

        public static void AreoEffectOn(this Form form)
        {
            InternalAreoEffectOn(form.Handle);
        }

        private static void InternalAreoEffectOff(IntPtr hwnd)
        {
            var os = Environment.OSVersion.Version;
            if (os.Major == 6)
            {
                switch (os.Minor)
                {
                    case 1:
                        Win7AreoEffectOff(hwnd);
                        return;
                    case 2:
                        Win10AreoEffectOff(hwnd);
                        return ;
                }
            }
            else if (os.Major == 10 && os.Minor == 0)
            {
                Win10AreoEffectOff(hwnd);
            }
        }


        public static void AreoEffectOff(this Window window)
        {
            var hwnd=new WindowInteropHelper(window).Handle;
            InternalAreoEffectOn(hwnd);
        }

        public static void AreoEffectOff(this Form form)
        {
            InternalAreoEffectOff(form.Handle);
        }

    }
}
