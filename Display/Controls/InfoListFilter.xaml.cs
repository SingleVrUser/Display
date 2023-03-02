// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using Windows.Foundation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Controls
{
    public sealed partial class InfoListFilter : UserControl
    {
        private static readonly DependencyProperty YearProperty =
            DependencyProperty.Register("Year", typeof(string), typeof(VideoCoverDisplay), null);
        private static readonly DependencyProperty ScoreProperty =
            DependencyProperty.Register("Score", typeof(int), typeof(VideoCoverDisplay), null);
        private static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(string), typeof(VideoCoverDisplay), null);

        public string Year
        {
            get { return (string)GetValue(YearProperty); }
            set { SetValue(YearProperty, value); }
        }
        public int Score
        {
            get { return (int)GetValue(ScoreProperty); }
            set { SetValue(ScoreProperty, value); }
        }
        public string Type
        {
            get { return (string)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public InfoListFilter()
        {
            this.InitializeComponent();
        }

        public event TypedEventHandler<SplitButton, SplitButtonClickEventArgs> SplitButton_Click;

        public event SelectionChangedEventHandler SelectionChanged;

        /// <summary>
        /// 取消或选择Type的筛选
        /// </summary>
        /// <param Name="sender"></param>
        /// <param Name="args"></param>
        private void Type_ToggleSplitButton_Click(SplitButton sender, SplitButtonClickEventArgs args)
        {
            if (!(sender is ToggleSplitButton splitButton)) return;

            if (splitButton.IsChecked)
            {
                if (!(Type_RadioButtons.SelectedItem is RadioButton radioButton))
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
                System.Diagnostics.Debug.WriteLine($"取消Type");
                Type_ToggleSplitButton.Content = "类别";
            }

            SplitButton_Click?.Invoke(sender, args);
        }

        /// <summary>
        /// 取消或选择Score的筛选
        /// </summary>
        /// <param Name="sender"></param>
        /// <param Name="args"></param>
        private void Score_ToggleSplitButton_Click(SplitButton sender, SplitButtonClickEventArgs args)
        {
            if (!(sender is ToggleSplitButton splitButton)) return;

            if (splitButton.IsChecked)
            {
                if (!(Score_RadioButtons.SelectedItem is RadioButton radioButton))
                {
                    radioButton = (RadioButton)Score_RadioButtons.Items.First();
                }

                int tmp;
                if (Int32.TryParse(radioButton.Tag.ToString(), out tmp))
                {
                    Score = tmp;
                }

                Score_ToggleSplitButton.Content = radioButton.Content;
                System.Diagnostics.Debug.WriteLine($"设置Score为{Score}");
            }
            else
            {
                Score = 0;
                System.Diagnostics.Debug.WriteLine($"取消Score");
                Score_ToggleSplitButton.Content = "评分";
            }

            SplitButton_Click?.Invoke(sender, args);
        }

        /// <summary>
        /// 取消或选择Year的筛选
        /// </summary>
        /// <param Name="sender"></param>
        /// <param Name="args"></param>
        private void Year_ToggleSplitButton_Click(SplitButton sender, SplitButtonClickEventArgs args)
        {
            if (!(sender is ToggleSplitButton splitButton)) return;

            if (splitButton.IsChecked)
            {
                if (!(Year_RadioButtons.SelectedItem is RadioButton radioButton))
                {
                    radioButton = (RadioButton)Year_RadioButtons.Items.First();
                }
                var tmp = radioButton.Content.ToString();

                if (tmp == "自定义")
                    Year = Year_CustomeTextBox.Text;
                else
                    Year = tmp;

                Year_ToggleSplitButton.Content = Year;
                System.Diagnostics.Debug.WriteLine($"设置Year为{Year}");
            }
            else
            {
                Year = string.Empty;
                System.Diagnostics.Debug.WriteLine($"取消Year");

                Year_ToggleSplitButton.Content = "年份";
            }

            SplitButton_Click?.Invoke(sender, args);
        }

        public void UncheckAllToggleSplitButton()
        {
            if (Year_ToggleSplitButton.IsChecked == true) Year_ToggleSplitButton.IsChecked = false;
            if (Score_ToggleSplitButton.IsChecked == true) Score_ToggleSplitButton.IsChecked = false;
            if (Type_ToggleSplitButton.IsChecked == true) Type_ToggleSplitButton.IsChecked = false;
        }

        private void Type_RadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //已被选中，跳过
            if (Type_ToggleSplitButton.IsChecked == true && string.IsNullOrEmpty(Type)) return;

            if (!(e.AddedItems.FirstOrDefault() is RadioButton radioButton)) return;

            Type = radioButton.Content.ToString();

            System.Diagnostics.Debug.WriteLine($"Type修改为{Type}");

            Type_ToggleSplitButton.Content = Type;
            Type_ToggleSplitButton.IsChecked = true;

            SelectionChanged?.Invoke(sender, e);
        }

        private void Year_RadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //已被选中，跳过
            if (Year_ToggleSplitButton.IsChecked == true && string.IsNullOrEmpty(Year)) return;

            if (!(e.AddedItems.FirstOrDefault() is RadioButton radioButton)) return;

            Type = radioButton.Content.ToString();

            var tmp = radioButton.Content.ToString();

            if (tmp == "自定义")
                Year = Year_CustomeTextBox.Text;
            else
                Year = tmp;

            System.Diagnostics.Debug.WriteLine($"Year修改为{Year}");

            Year_ToggleSplitButton.Content = Year;
            Year_ToggleSplitButton.IsChecked = true;

            SelectionChanged?.Invoke(sender, e);
        }

        private void Score_RadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //已被选中，跳过
            if (Score_ToggleSplitButton.IsChecked == true && Score == 0) return;

            if (!(e.AddedItems.FirstOrDefault() is RadioButton radioButton)) return;

            int tmp;
            if (Int32.TryParse(radioButton.Tag.ToString(), out tmp))
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
}
