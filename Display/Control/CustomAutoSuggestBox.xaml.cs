using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml.Media.Animation;
using System.Numerics;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml.Shapes;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Control
{
    public sealed partial class CustomAutoSuggestBox : UserControl
    {
        public CustomAutoSuggestBox()
        {
            this.InitializeComponent();
        }

        //输入的Text改变
        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && sender.Text != "")
            {
                //List<VideoInfo> resultVideoInfo = new List<VideoInfo>();

                //Set the ItemsSource to be your filtered dataset
                string searchText = sender.Text;

                //过滤非法元素
                searchText = searchText.Replace("'", "");

                List<VideoInfo> item = FileMatch.getVideoInfoFromType(selectFoundMethodButton.Content.ToString(), searchText);

                if (item.Count == 0)
                {
                    //object context = new();
                    //Resources.TryGetValue("notFoundedSuggestionBox", out context);
                    NavViewSearchBox.ItemTemplate = Resources["notFoundedSuggestionBox"] as DataTemplate;
                    sender.ItemTemplate = null;
                    sender.ItemsSource = new List<string>() { "未找到" };
                }
                else
                {
                    object context;
                    Resources.TryGetValue("FoundedSuggestionBox", out context);
                    NavViewSearchBox.ItemTemplate = context as DataTemplate;
                    //NavViewSearchBox.ItemTemplate = 
                    sender.ItemsSource = item;
                }

            }
        }

        //提交请求（按下Enter）
        public event TypedEventHandler<AutoSuggestBox, AutoSuggestBoxQuerySubmittedEventArgs> QuerySubmitted;
        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            //var nowItem = args.ChosenSuggestion as VideoInfo;
            if ( args.QueryText != "Data.VideoInfo")
            {
                sender.DataContext = selectFoundMethodButton.Content;
                QuerySubmitted?.Invoke(sender, args);
            }

            //初始化搜索框
            NavViewSearchBox.Text = "";
            sender.ItemsSource = null;
        }

        //选中AutoSuggest的选项值
        public event TypedEventHandler<AutoSuggestBox, AutoSuggestBoxSuggestionChosenEventArgs> SuggestionChosen;
        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            //准备动画
            ConnectedAnimation animation = ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("ForwardConnectedAnimation", sender);

            SuggestionChosen?.Invoke(sender, args);
        }

        private void NavViewSearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (selectFoundMethodButton.Visibility == Visibility.Collapsed)
            {
                openAutoSuggestionBoxStoryboard.Begin();
            }
        }

        private void NavViewSearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (selectFoundMethodButton.FocusState != FocusState.Pointer)
            {
                closeAutoSuggestionBoxStoryboard.Begin();
            }
        }

        private void FoundNameButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectFoundMethodButton.Content?.ToString() != "番号")
            {
                selectFoundMethodButton.Content = "番号";
            }
        }

        private void FoundActorButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectFoundMethodButton.Content?.ToString() != "演员")
            {
                selectFoundMethodButton.Content = "演员";
            }
        }

        private void LabelNameButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectFoundMethodButton.Content?.ToString() != "标签")
            {
                selectFoundMethodButton.Content = "标签";
            }
        }
    }
}
