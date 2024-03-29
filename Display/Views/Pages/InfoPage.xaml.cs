using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Display.Views.Pages;

public sealed partial class InfoPage
{
    public InfoPage(Dictionary<string, string> infos)
    {
        InitializeComponent();

        InitData(infos);
    }

    private void InitData(Dictionary<string, string> infos)
    {
        var i = 0;
        var titleTextBlockStyle = Resources["TitleTextBlockStyle"] as Style;
        var contentTextBlockStyle = Resources["ContentTextBlockStyle"] as Style;
        foreach (var info in infos)
        {
            RootGrid.RowDefinitions.Add(new RowDefinition());

            var titleTextBlock = new TextBlock
            {
                Text = info.Key,
                Style = titleTextBlockStyle
            };
            titleTextBlock.SetValue(Grid.RowProperty, i);
            var contentTextBlock = new TextBlock
            {
                Text = info.Value,
                Style = contentTextBlockStyle
            };
            contentTextBlock.SetValue(Grid.RowProperty, i);
            contentTextBlock.SetValue(Grid.ColumnProperty, 1);

            RootGrid.Children.Add(titleTextBlock);
            RootGrid.Children.Add(contentTextBlock);

            i++;
        }
    }

    public static async System.Threading.Tasks.Task ShowInContentDialog(XamlRoot xamlRoot, Dictionary<string, string> infos, string title = "")
    {
        var content = new InfoPage(infos);

        var dialog = new ContentDialog
        {
            XamlRoot = xamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            CloseButtonText = "返回",
            DefaultButton = ContentDialogButton.Close,
            Content = content
        };

        if (!string.IsNullOrEmpty(title))
        {
            dialog.Title = title;
        }

        await dialog.ShowAsync();
    }
}