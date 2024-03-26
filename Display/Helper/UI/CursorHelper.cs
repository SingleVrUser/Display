using Microsoft.UI.Input;
using System;
using System.Runtime.InteropServices;

namespace Display.Helper.UI;

internal static class CursorHelper
{
    private const string ModuleName = "Display";

    /// <summary>
    /// 获取鼠标闲置时间
    /// </summary>
    /// <param name="inputInfo"></param>
    /// <returns></returns>
    [DllImport("user32.dll")]
    public static extern bool GetLastInputInfo(ref LastInputInfo inputInfo);

    /// <summary>
    /// 设置鼠标状态的计数器（非状态）
    /// </summary>
    /// <param name="bShow"></param>
    /// <returns></returns>
    [DllImport("user32.dll", EntryPoint = "ShowCursor", CharSet = CharSet.Auto)]
    public static extern int ShowCursor(bool bShow);

    [StructLayout(LayoutKind.Sequential)]
    public struct LastInputInfo
    {
        [MarshalAs(UnmanagedType.U4)] public int cbSize;
        [MarshalAs(UnmanagedType.U4)] public uint dwTime;
    }

    private static InputCursor _hiddenCursor;
    public static InputCursor GetHiddenCursor()
    {
        if (_hiddenCursor == null)
            _hiddenCursor = InputDesktopResourceCursor.CreateFromModule(ModuleName, 205);
        return _hiddenCursor;
    }

    private static InputCursor _zoomCursor;
    public static InputCursor GetZoomCursor()
    {
        if (_zoomCursor == null)
            _zoomCursor = InputDesktopResourceCursor.CreateFromModule(ModuleName, 206);
        return _zoomCursor;
    }

    public static long GetIdleTick()
    {
        var vLastInputInfo = new LastInputInfo();
        vLastInputInfo.cbSize = Marshal.SizeOf(vLastInputInfo);
        if (!GetLastInputInfo(ref vLastInputInfo)) return 0;

        return Environment.TickCount - vLastInputInfo.dwTime;
    }
}