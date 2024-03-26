using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Controls;

namespace Display.Controls.CustomController;

internal class CursorGrid : Grid
{
    /// <summary>
    /// 暴露 InputCursor
    /// </summary>
    public InputCursor Cursor
    {
        get => ProtectedCursor;
        set => ProtectedCursor = value;
    }
}