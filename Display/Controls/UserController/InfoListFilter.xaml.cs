using System.Linq;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Display.Controls.UserController;

public sealed partial class InfoListFilter
{
    private static readonly DependencyProperty YearProperty =
        DependencyProperty.Register("Year", typeof(string), typeof(VideoCoverDisplay), null);
    private static readonly DependencyProperty ScoreProperty =
        DependencyProperty.Register("Score", typeof(int), typeof(VideoCoverDisplay), null);
    private static readonly DependencyProperty TypeProperty =
        DependencyProperty.Register("Type", typeof(string), typeof(VideoCoverDisplay), null);

    public string Year
    {
        get => (string)GetValue(YearProperty);
        private set => SetValue(YearProperty, value);
    }
    public int Score
    {
        get => (int)GetValue(ScoreProperty);
        private set => SetValue(ScoreProperty, value);
    }
    public string Type
    {
        get => (string)GetValue(TypeProperty);
        private set => SetValue(TypeProperty, value);
    }

    public InfoListFilter()
    {
        InitializeComponent();
    }

    public event TypedEventHandler<SplitButton, SplitButtonClickEventArgs> SplitButtonClick;

    public event SelectionChangedEventHandler SelectionChanged;

    /// <summary>
    /// 取消或选择Type的筛选
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void Type_ToggleSplitButton_Click(SplitButton sender, SplitButtonClickEventArgs args)
    {
        if (sender is not ToggleSplitButton splitButton) return;

        if (splitButton.IsChecked)
        {
            if (Type_RadioButtons.SelectedItem is not RadioButton radioButton)
            {
                radioButton = (RadioButton)Type_RadioButtons.Items.First();
            }
            Type = radioButton.Content.ToString();

            System.Diagnostics.Debug.WriteLine($"设置Type为{Type}");

            Type_ToggleSplitButton.Content = Type;
        }
        else
        {
            Type = string.Empty;
            System.Diagnostics.Debug.WriteLine("取消Type");
            Type_ToggleSplitButton.Content = "类别";
        }

        SplitButtonClick?.Invoke(sender, args);
    }

    /// <summary>
    /// 取消或选择Score的筛选
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void Score_ToggleSplitButton_Click(SplitButton sender, SplitButtonClickEventArgs args)
    {
        if (sender is not ToggleSplitButton splitButton) return;

        if (splitButton.IsChecked)
        {
            if (Score_RadioButtons.SelectedItem is not RadioButton radioButton)
            {
                radioButton = (RadioButton)Score_RadioButtons.Items.First();
            }

            if (int.TryParse(radioButton.Tag.ToString(), out var tmp))
            {
                Score = tmp;
            }

            Score_ToggleSplitButton.Content = radioButton.Content;
            System.Diagnostics.Debug.WriteLine($"设置Score为{Score}");
        }
        else
        {
            Score = 0;
            System.Diagnostics.Debug.WriteLine("取消Score");
            Score_ToggleSplitButton.Content = "评分";
        }

        SplitButtonClick?.Invoke(sender, args);
    }

    /// <summary>
    /// 取消或选择Year的筛选
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void Year_ToggleSplitButton_Click(SplitButton sender, SplitButtonClickEventArgs args)
    {
        if (sender is not ToggleSplitButton splitButton) return;

        if (splitButton.IsChecked)
        {
            if (Year_RadioButtons.SelectedItem is not RadioButton radioButton)
            {
                radioButton = (RadioButton)Year_RadioButtons.Items.First();
            }
            var tmp = radioButton.Content.ToString();

            Year = tmp == "自定义" ? Year_CustomeTextBox.Text : tmp;

            Year_ToggleSplitButton.Content = Year;
            System.Diagnostics.Debug.WriteLine($"设置Year为{Year}");
        }
        else
        {
            Year = string.Empty;
            System.Diagnostics.Debug.WriteLine("取消Year");

            Year_ToggleSplitButton.Content = "年份";
        }

        SplitButtonClick?.Invoke(sender, args);
    }

    public void UncheckAllToggleSplitButton()
    {
        if (Year_ToggleSplitButton.IsChecked) Year_ToggleSplitButton.IsChecked = false;
        if (Score_ToggleSplitButton.IsChecked) Score_ToggleSplitButton.IsChecked = false;
        if (Type_ToggleSplitButton.IsChecked) Type_ToggleSplitButton.IsChecked = false;
    }

    private void Type_RadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        //已被选中，跳过
        if (Type_ToggleSplitButton.IsChecked && string.IsNullOrEmpty(Type)) return;

        if (e.AddedItems.FirstOrDefault() is not RadioButton radioButton) return;

        Type = radioButton.Content.ToString();

        System.Diagnostics.Debug.WriteLine($"Type修改为{Type}");

        Type_ToggleSplitButton.Content = Type;
        Type_ToggleSplitButton.IsChecked = true;

        SelectionChanged?.Invoke(sender, e);
    }

    private void Year_RadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        //已被选中，跳过
        if (Year_ToggleSplitButton.IsChecked && string.IsNullOrEmpty(Year)) return;

        if (e.AddedItems.FirstOrDefault() is not RadioButton radioButton) return;

        Type = radioButton.Content.ToString();

        var tmp = radioButton.Content.ToString();

        Year = tmp == "自定义" ? Year_CustomeTextBox.Text : tmp;

        System.Diagnostics.Debug.WriteLine($"Year修改为{Year}");

        Year_ToggleSplitButton.Content = Year;
        Year_ToggleSplitButton.IsChecked = true;

        SelectionChanged?.Invoke(sender, e);
    }

    private void Score_RadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        //已被选中，跳过
        if (Score_ToggleSplitButton.IsChecked && Score == 0) return;

        if (e.AddedItems.FirstOrDefault() is not RadioButton radioButton) return;

        if (int.TryParse(radioButton.Tag.ToString(), out var tmp))
        {
            Score = tmp;
        }

        System.Diagnostics.Debug.WriteLine($"Score修改为{Score}");
        Score_ToggleSplitButton.Content = radioButton.Content;
        Score_ToggleSplitButton.IsChecked = true;

        SelectionChanged?.Invoke(sender, e);
    }

    public event TextChangedEventHandler TextChanged;
    private void Year_CustomeTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!(sender is TextBox textBox)) return;

        Year = textBox.Text;
        Year_ToggleSplitButton.Content = Year;

        TextChanged?.Invoke(textBox, e);
    }

}