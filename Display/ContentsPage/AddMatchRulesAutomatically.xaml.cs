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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddMatchRulesAutomatically : Page
    {
        public ObservableCollection<KeywordMatchName> AddMatchNameList = new();

        public AddMatchRulesAutomatically(List<KeywordMatchName> keywordMatchNames)
        {
            this.InitializeComponent();

            MatchKeywordFromOriginalName(keywordMatchNames);
        }

        private void MatchKeywordFromOriginalName(List<KeywordMatchName> keywordMatchNames)
        {
            foreach (var keywordMatchName in keywordMatchNames)
            {
                var originalName = keywordMatchName.OriginalName;
                var nextName = Data.FileMatch.DeleteSomeKeywords(originalName);
                var reg = new Regex(@"([a-zA-Z]+)[-_]?\d{3,6}", RegexOptions.IgnoreCase);

                Match match_keywords = reg.Match(nextName);
                if (match_keywords.Success)
                {
                    string keyword = match_keywords.Groups[1].Value;

                    var i = AddMatchNameList.Where(x => x.Keyword == keyword).FirstOrDefault();
                    if (i == null)
                    {
                        AddMatchNameList.Add(new KeywordMatchName() { OriginalName = originalName, Keyword = keyword });
                    }

                    //AddMatchNameList.Add();
                }

            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var deleteItem = (sender as Button).DataContext as KeywordMatchName;

            //部分设备使用会报错，添加SelectionMode = "None"可用
            AddMatchNameList.Remove(deleteItem);
        }
    }
}
