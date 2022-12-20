// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Org.BouncyCastle.Tls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Control
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
        /// ȡ����ѡ��Type��ɸѡ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
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

                System.Diagnostics.Debug.WriteLine($"����TypeΪ{Type}");

                Type_ToggleSplitButton.Content = Type;
            }
            else
            {
                Type = string.Empty;
                System.Diagnostics.Debug.WriteLine($"ȡ��Type");
                Type_ToggleSplitButton.Content = "���";
            }

            SplitButton_Click?.Invoke(sender, args);
        }

        /// <summary>
        /// ȡ����ѡ��Score��ɸѡ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
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
                System.Diagnostics.Debug.WriteLine($"����ScoreΪ{Score}");
            }
            else
            {
                Score = 0;
                System.Diagnostics.Debug.WriteLine($"ȡ��Score");
                Score_ToggleSplitButton.Content = "����";
            }

            SplitButton_Click?.Invoke(sender, args);
        }

        /// <summary>
        /// /ȡ����ѡ��Year��ɸѡ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
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

                if (tmp == "�Զ���")
                    Year = Year_CustomeTextBox.Text;
                else
                    Year = tmp;

                Year_ToggleSplitButton.Content = Year;
                System.Diagnostics.Debug.WriteLine($"����YearΪ{Year}");
            }
            else
            {
                Year = string.Empty;
                System.Diagnostics.Debug.WriteLine($"ȡ��Year");

                Year_ToggleSplitButton.Content = "���";
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
            //�ѱ�ѡ�У�����
            if (Type_ToggleSplitButton.IsChecked == true && string.IsNullOrEmpty(Type)) return;

            if (!(e.AddedItems.FirstOrDefault() is RadioButton radioButton)) return;

            Type = radioButton.Content.ToString();

            System.Diagnostics.Debug.WriteLine($"Type�޸�Ϊ{Type}");

            Type_ToggleSplitButton.Content = Type;
            Type_ToggleSplitButton.IsChecked = true;

            SelectionChanged?.Invoke(sender, e);
        }

        private void Year_RadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //�ѱ�ѡ�У�����
            if (Year_ToggleSplitButton.IsChecked == true && string.IsNullOrEmpty(Year)) return;

            if (!(e.AddedItems.FirstOrDefault() is RadioButton radioButton)) return;

            Type = radioButton.Content.ToString();

            var tmp = radioButton.Content.ToString();

            if (tmp == "�Զ���")
                Year = Year_CustomeTextBox.Text;
            else
                Year = tmp;

            System.Diagnostics.Debug.WriteLine($"Year�޸�Ϊ{Year}");

            Year_ToggleSplitButton.Content = Year;
            Year_ToggleSplitButton.IsChecked = true;

            SelectionChanged?.Invoke(sender, e);
        }

        private void Score_RadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //�ѱ�ѡ�У�����
            if (Score_ToggleSplitButton.IsChecked == true && Score == 0) return;

            if (!(e.AddedItems.FirstOrDefault() is RadioButton radioButton)) return;

            int tmp;
            if (Int32.TryParse(radioButton.Tag.ToString(), out tmp))
            {
                Score = tmp;
            }

            System.Diagnostics.Debug.WriteLine($"Score�޸�Ϊ{Score}");
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
