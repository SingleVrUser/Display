
using System.Diagnostics;
using System;
using System.Runtime.InteropServices;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;

namespace Display.Helper
{
    public class CursorHelper
    {

        /// <summary>
        /// 获取鼠标闲置时间
        /// </summary>
        /// <param name="plii"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        /// <summary>
        /// 设置鼠标状态的计数器（非状态）
        /// </summary>
        /// <param name="bShow"></param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "ShowCursor", CharSet = CharSet.Auto)]
        public static extern int ShowCursor(bool bShow);

        [StructLayout(LayoutKind.Sequential)]
        public struct LASTINPUTINFO
        {
            [MarshalAs(UnmanagedType.U4)] public int cbSize;
            [MarshalAs(UnmanagedType.U4)] public uint dwTime;
        }

        private static InputCursor _hiddenCursor;
        public static InputCursor GetHiddenCursor()
        {
            if(_hiddenCursor == null)
                _hiddenCursor = InputDesktopResourceCursor.CreateFromModule("Display", 205);
            return _hiddenCursor;
        }

        private static InputCursor _zoomCursor;
        public static InputCursor GetZoomCursor()
        {
            if(_zoomCursor == null)
                _zoomCursor = InputDesktopResourceCursor.CreateFromModule("Display", 206);
            return _zoomCursor;
        }

        public static long GetIdleTick()
        {
            var vLastInputInfo = new LASTINPUTINFO();
            vLastInputInfo.cbSize = Marshal.SizeOf(vLastInputInfo);
            if (!GetLastInputInfo(ref vLastInputInfo)) return 0;

            Debug.WriteLine($"Environment.TickCount:{Environment.TickCount}");
            return Environment.TickCount - vLastInputInfo.dwTime;
        }
    }
}
