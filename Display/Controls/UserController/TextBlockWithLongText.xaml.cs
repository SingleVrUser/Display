using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Controls.UserController
{
    public sealed partial class TextBlockWithLongText : UserControl
    {
        //显示的文本，使用 {x:bind } 赋值时一开始会为 null
        public string StringValue
        {
            get => (string)GetValue(StringValueProperty);
            set => SetValue(StringValueProperty, value);
        }
        public static readonly DependencyProperty StringValueProperty =
            DependencyProperty.Register(nameof(StringValue), typeof(string), typeof(TextBlockWithLongText), null);

        //文本是否可选中
        public bool IsFirstTextSelectionEnabled { get; set; } = true;
        public bool IsSecondTextSelectionEnabled { get; set; } = true;

        public TextBlockWithLongText()
        {
            this.InitializeComponent();
        }

    }
}
