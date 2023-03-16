// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Threading.Tasks;
using Windows.Foundation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Controls
{
    public sealed partial class FileAutoSuggestBox : UserControl
    {
        public FileAutoSuggestBox()
        {
            this.InitializeComponent();
        }


        public event TypedEventHandler<AutoSuggestBox, AutoSuggestBoxTextChangedEventArgs> TextChanged;
        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            TextChanged?.Invoke(sender, args);
        }

        //显示或隐藏搜索框
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if(!(sender is Button button))
            {
                return;
            }

            button.IsEnabled= false;

            //关闭
            if (MyAutoSuggestBox.Visibility == Visibility.Visible && MyAutoSuggestBox.Width==300)
            {
                CloseStoryborard.Begin();

            }
            //伸展
            else if (MyAutoSuggestBox.Visibility == Visibility.Collapsed)
            {
                OpenStoryborard.Begin();

                await Task.Delay(300);
                MyAutoSuggestBox.Focus(FocusState.Keyboard);
            }

            button.IsEnabled = true;
        }
    }
}
