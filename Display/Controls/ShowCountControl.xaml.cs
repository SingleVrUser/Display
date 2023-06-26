
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Display.Controls;

public sealed partial class ShowCountControl : UserControl
{
    public static readonly DependencyProperty CurrentCountProperty =
            DependencyProperty.Register(nameof(CurrentCount), typeof(int), typeof(ShowCountControl), null);
    public static readonly DependencyProperty AllCountProperty =
            DependencyProperty.Register(nameof(AllCount), typeof(int), typeof(ShowCountControl), null);


    public int CurrentCount
    {
        get => (int)GetValue(CurrentCountProperty);
        set => SetValue(CurrentCountProperty, value);
    }
    public int AllCount
    {
        get => (int)GetValue(AllCountProperty);
        set => SetValue(AllCountProperty, value);
    }

    public ShowCountControl()
    {
        this.InitializeComponent();
    }

    public event RoutedEventHandler Clicked;
    private void ToTopButton_Click(object sender, RoutedEventArgs e)
    {
        Clicked?.Invoke(sender, e);
    }
}
