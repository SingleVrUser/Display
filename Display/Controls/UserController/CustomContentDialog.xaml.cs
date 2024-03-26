// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Controls.UserController
{
    public sealed partial class CustomContentDialog
    {
        public CustomContentDialog(UIElement contentPage, string title = "")
        {
            this.InitializeComponent();

            if (!string.IsNullOrEmpty(title))
            {
                var titleTextBlock = new TextBlock()
                {
                    Style = this.Resources["BoldTextBlock"] as Style,
                    Text = title
                };
                RootGrid.Children.Add(titleTextBlock);

                var titleRowDefinition = new RowDefinition
                {
                    Height = GridLength.Auto
                };

                RootGrid.RowDefinitions.Insert(0, titleRowDefinition);
                //titleTextBlock.SetValue(Grid.RowProperty, 0);
                contentPage.SetValue(Grid.RowProperty, 1);
                ButtonGrid.SetValue(Grid.RowProperty, 2);
            }
            else
            {
                contentPage.SetValue(Grid.RowProperty, 0);
                ButtonGrid.SetValue(Grid.RowProperty, 1);
            }

            RootGrid.Children.Add(contentPage);
        }

        public event EventHandler<RoutedEventArgs> PrimaryButtonClick;
        private void PrimaryButton_OnClick(object sender, RoutedEventArgs e)
        {
            PrimaryButtonClick?.Invoke(sender, e);
        }

        public event EventHandler<RoutedEventArgs> CancelButtonClick;
        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            CancelButtonClick?.Invoke(sender, e);
        }

    }
}
