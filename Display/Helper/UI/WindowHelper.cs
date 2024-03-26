using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using WinRT.Interop;

namespace Display.Helper.UI
{
    public class WindowHelper
    {


        [DllImport("Shcore.dll", SetLastError = true)]
        private static extern int GetDpiForMonitor(nint hMonitor, MonitorDpiType dpiType, out uint dpiX, out uint dpiY);

        private enum MonitorDpiType
        {
            MdtEffectiveDpi = 0,
            MdtAngularDpi = 1,
            MdtRawDpi = 2,
            MdtDefault = MdtEffectiveDpi
        }

        public static double GetScaleAdjustment(object window)
        {
            var hWnd = WindowNative.GetWindowHandle(window);
            var wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            var displayArea = DisplayArea.GetFromWindowId(wndId, DisplayAreaFallback.Primary);
            var hMonitor = Win32Interop.GetMonitorFromDisplayId(displayArea.DisplayId);

            // Get DPI.
            var result = GetDpiForMonitor(hMonitor, MonitorDpiType.MdtDefault, out var dpiX, out _);
            if (result != 0)
            {
                throw new Exception("Could not get DPI for monitor.");
            }

            var scaleFactorPercent = (uint)(((long)dpiX * 100 + (96 >> 1)) / 96);
            return scaleFactorPercent / 100.0;
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(nint hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(nint hWnd);

        public static void SetForegroundWindow(Microsoft.UI.Xaml.Window window)
        {
            // Bring the window to the foreground... first get the window handle...
            var hwnd = WindowNative.GetWindowHandle(window);

            // Restore window if minimized... requires DLL import above
            ShowWindow(hwnd, 0x00000009);

            // And call SetForegroundWindow... requires DLL import above
            SetForegroundWindow(hwnd);
        }

        private static readonly List<Window> ActiveWindows = [];

        public static Window CreateWindow()
        {
            var newWindow = new Window
            {
                SystemBackdrop = new MicaBackdrop()
            };
            TrackWindow(newWindow);
            return newWindow;
        }

        public static void TrackWindow(Window window)
        {
            window.Closed += (_, _) =>
            {
                ActiveWindows.Remove(window);
            };
            ActiveWindows.Add(window);
        }

        public static Window GetWindowForElement(UIElement element)
        {
            return element.XamlRoot == null ?
                null :
                ActiveWindows.FirstOrDefault(window => element.XamlRoot == window.Content.XamlRoot);
        }
    }
}
