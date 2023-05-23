using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Controls;

namespace Display.Controls
{
    internal class CursorGrid : Grid
    {
        public InputCursor Cursor
        {
            get => ProtectedCursor;

            set => ProtectedCursor = value;
        }
    }
}
