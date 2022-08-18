using Data;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CheckMatchMethod : Page
    {
        public VideoMatchInfo videoMatchInfo;

        public CheckMatchMethod(List<MatchVideoResult> matchResultList)
        {
            this.InitializeComponent();

            reMatchProgressRing.IsActive = true;
            videoMatchInfo = new(matchResultList);

            reMatchProgressRing.IsActive = false;
        }

        public IAsyncOperation<ContentDialogResult> ShowContentDialog()
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Title = "检索";
            dialog.XamlRoot = this.XamlRoot;
            dialog.PrimaryButtonText = "确认";
            dialog.CloseButtonText = "取消";
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.Content = "即将搜刮视频信息，点击确认后开始";

            return dialog.ShowAsync();
        }

        private void MatchKeywordModify_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            //如果不是回车
            if (e.Key != Windows.System.VirtualKey.Enter) return;

            string newKeyword = ((TextBox)sender).Text;
            bool result = videoMatchInfo.tryAddKeyword(newKeyword);
            if (result)
            {
                CommonTeachingTip.Content = $"规则列表 <= {newKeyword}";
                CommonTeachingTip.IsOpen = true;
            }
        }

        private void ChangeKeywordString_Click(object sender, RoutedEventArgs e)
        {
            videoMatchInfo.tryUpdateKeyword(ChangeKeywordString_TextBox.Text);
        }

        /// <summary>
        /// 重新匹配
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ReMatchButton_Click(object sender, RoutedEventArgs e)
        {
            reMatchProgressRing.IsActive = true;
            List<KeywordMatchName> allNameList = new();

            foreach(var item in videoMatchInfo.SuccessNameCollection.allMatchNameList)
            {
                allNameList.Add(item);
            }

            foreach(var item in videoMatchInfo.FailNameCollection.allMatchNameList)
            {
                allNameList.Add(item);
            }

            List<KeywordMatchName>[] NameList = await Task.Run(() =>
            {
                List<KeywordMatchName> SuccessNameCollection = new();
                List<KeywordMatchName> FailNameCollection = new();

                foreach (var info in allNameList)
                {

                    //根据视频名称匹配番号
                    var VideoName = FileMatch.MatchName(info.OriginalName);

                    //未匹配
                    if (VideoName == null)
                    {
                        FailNameCollection.Add(new KeywordMatchName() { OriginalName = info.OriginalName });
                        continue;
                    }

                    //匹配后，查询是否重复匹配
                    var existsResult = videoMatchInfo.FailNameCollection.Where(x => x.MatchName == VideoName).FirstOrDefault();

                    if (existsResult == null)
                    {
                        SuccessNameCollection.Add(new KeywordMatchName() { OriginalName = info.OriginalName, MatchName = VideoName });
                    }
                }

                List<KeywordMatchName>[] NameList = { SuccessNameCollection, FailNameCollection };

                return NameList;
            });


            videoMatchInfo.SuccessNameCollection.Clear();
            videoMatchInfo.SuccessNameCollection.allMatchNameList = new();
            foreach (var item in NameList[0])
            {
                videoMatchInfo.SuccessNameCollection.AddActualList(item);
            }
            
            videoMatchInfo.FailNameCollection.Clear();
            videoMatchInfo.FailNameCollection.allMatchNameList=new();

            foreach (var item in NameList[1])
            {
                videoMatchInfo.FailNameCollection.AddActualList(item);
            }


            reMatchProgressRing.IsActive = false;
        }
        private void MatchSuccess_Expander_Expanding(Expander sender, ExpanderExpandingEventArgs args)
        {
            //VisualStateManager.GoToState(this, "OpenMatchSuccessResult", true);
            OpenMatchSuccessRusult_Storyboard.Begin();
            //var i = MatchResult_Grid;
        }

        private void MatchFail_Expander_Expanding(Expander sender, ExpanderExpandingEventArgs args)
        {
            OpenMatchFailRusult_Storyboard.Begin();
            //VisualStateManager.GoToState(this, "OpenMatchFailResult", true);
        }

        private void MatchSuccessExpander_Collapsed(Expander sender, ExpanderCollapsedEventArgs args)
        {
            //VisualStateManager.GoToState(this, "CloseMatchResults", true);
            //CloseMatchSuccessResult_Storyboard.Begin();
            CloseMatchSuccessResult_Storyboard.Begin();
            //var i = MatchResult_Grid;
        }

        private async void AddRulesButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();

            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            dialog.XamlRoot = this.XamlRoot;
            dialog.Title = "自动添加";
            dialog.PrimaryButtonText = "添加";
            dialog.CloseButtonText = "返回";
            dialog.DefaultButton = ContentDialogButton.Primary;

            var AddMatchRulesAutomaticallyPage = new AddMatchRulesAutomatically(videoMatchInfo.FailNameCollection.allMatchNameList);
            dialog.Content = AddMatchRulesAutomaticallyPage;

            var result = await dialog.ShowAsync();

            if(result == ContentDialogResult.Primary)
            {

                foreach(var newKeywordItem in AddMatchRulesAutomaticallyPage.AddMatchNameList)
                {
                    videoMatchInfo.tryAddKeyword(newKeywordItem.Keyword.ToLower());
                }
 
            }

        }
    }

    public class VideoMatchInfo : INotifyPropertyChanged
    {
        public VideoMatchInfo(List<MatchVideoResult> matchResultList)
        {
            SuccessNameCollection = new();
            FailNameCollection = new();

            foreach (var item in matchResultList)
            {
                //匹配成功
                if (item.status && item.statusCode == 1)
                {
                    SuccessNameCollection.AddActualList(new KeywordMatchName() { OriginalName = item.OriginalName, MatchName = item.MatchName });
                }

                //匹配失败
                else if (!item.status && item.statusCode == -1)
                {
                    FailNameCollection.AddActualList(new KeywordMatchName() { OriginalName = item.OriginalName });
                }
            }

        }

        public IncrementalLoadingdMatchNameCollection FailNameCollection;

        public IncrementalLoadingdMatchNameCollection SuccessNameCollection;

        public string KeywordString
        {
            get
            {
                return AppSettings.MatchVideoKeywordsString;
            }
            set
            {
                AppSettings.MatchVideoKeywordsString = value;
                OnPropertyChanged();
                //OnPropertyChanged("KeywordCount");
            }
        }

        private ObservableCollection<string> _keywordCollection;
        public ObservableCollection<string> KeywordCollection
        {
            get
            {
                if(_keywordCollection == null)
                {
                    var KeywordStringList = KeywordString.Split(',');
                    _keywordCollection = new ObservableCollection<string>();
                    foreach (var keyword in KeywordStringList)
                    {
                        _keywordCollection.Add(keyword);
                    }

                }

                return _keywordCollection;
            }
            set
            {
                _keywordCollection = value;
            }
        }

        public bool tryAddKeyword(string newKeyword)
        {
            bool result = false;

            var keywordsList = KeywordString.Split(",");

            if (!keywordsList.Contains(newKeyword))
            {
                KeywordString += $",{newKeyword}";
                KeywordCollection.Add(newKeyword);
                result = true;
            }

            return result;
        }

        public void tryUpdateKeyword(string newKeywordString)
        {
            KeywordString = newKeywordString;

            var KeywordStringList = newKeywordString.Split(',');

            KeywordCollection.Clear();
            foreach (var keyword in KeywordStringList)
            {
                KeywordCollection.Add(keyword);
            }
        }

        public int KeywordCount
        { get
            { 
                return KeywordCollection.Count;
            } 
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    //继承ISupportIncrementalLoading接口，增量加载数据
    public class IncrementalLoadingdMatchNameCollection : ObservableCollection<KeywordMatchName>, ISupportIncrementalLoading
    {
        private int startIndex = 0;
        public int actualCount
        {
            get
            {
                return allMatchNameList.Count;
            }
        }

        public List<KeywordMatchName> allMatchNameList = new();

        public bool HasMoreItems
        {
            get
            {
                if (startIndex < actualCount)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return InnerLoadMoreItemsAsync(count).AsAsyncOperation();
        }

        private async Task<LoadMoreItemsResult> InnerLoadMoreItemsAsync(uint expectedCount)
        {
            //ObservableCollection<ExampleItem> exampleItems;
            int addCount = 100;

            int endCount = startIndex + addCount;
            if (endCount > actualCount)
            {
                endCount = actualCount;
                addCount = endCount - startIndex;
            }

            for (int i = startIndex; i < endCount; i++)
            {
                Add(allMatchNameList[i]);
            }
            await Task.Delay(1000);

            startIndex = endCount;
            return new LoadMoreItemsResult
            {
                Count = (uint)addCount
            };
        }

        public void AddActualList(KeywordMatchName item)
        {
            allMatchNameList.Add(item);
        }
    }

    public class KeywordMatchName
    {
        public string OriginalName;
        private string _keyword = string.Empty;
        public string Keyword
        { 
            get
            {
                string keyword = string.Empty;
                if (string.IsNullOrEmpty(_keyword))
                {
                    Match match_keyword = Regex.Match(MatchName, @"(\w+)[-_]?\d", RegexOptions.IgnoreCase);
                    if (match_keyword.Success)
                    {
                        keyword = match_keyword.Groups[1].Value;
                    }
                }
                else
                {
                    keyword = _keyword;
                }

                return keyword;
            }
            set
            {
                _keyword = value;
            }
        }
        public string MatchName;
    }


    //public class MatchKeyword
    //{
    //    ObservableCollection<string> KeywordCollection;
    //    List<MatchKeyword> MatchKeywords;
    //}
}
