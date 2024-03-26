using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Display.Controls;
using Display.Models.Disk._115;
using Display.Views.Settings;
using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using Display.Controls.UserController;
using Settings18Page = Display.Views.Sort115.Settings18Page;

namespace Display.ViewModels
{
    public partial class SortSettingsViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(NumNameSample))]
        [NotifyPropertyChangedFor(nameof(SingleVideoNameSample))]
        [NotifyPropertyChangedFor(nameof(MultipleVideoNameSample))]
        private string _numNameFormat = "{字母}-{数字}";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(MultipleVideoNameSample))]
        private string _separator = "-";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(MultipleVideoNameSample))]
        private int _separatorNumSelectedIndex = 1;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(NumNameSample))]
        [NotifyPropertyChangedFor(nameof(SingleVideoNameSample))]
        [NotifyPropertyChangedFor(nameof(MultipleVideoNameSample))]
        private int _numNameFormatSelectedIndex = 1;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SingleVideoNameSample))]
        private string _singleVideoNameFormat = "[{番号}] {标题} {演员}";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(MultipleVideoNameSample))]
        private string _multipleVideoNameFormat = "[{番号}] {标题} {演员} {分隔符}{序号}";

        private Sort115Settings _settings;

        public Sort115Settings Settings => _settings ??= Settings18Page.Settings;

        private Settings18Page _currentPage;

        public void SetPage(Settings18Page currentPage)
        {
            _currentPage = currentPage;

        }

        public string NumNameSample => GetNumNameSample(NumNameFormat, NumNameFormatSelectedIndex);
        public string SingleVideoNameSample => GetSingleVideoNameSample(SingleVideoNameFormat, numName: NumNameSample);

        public string MultipleVideoNameSample =>
            GetMultipleVideoNameSample(MultipleVideoNameFormat, numName: NumNameSample, Separator, SeparatorNumSelectedIndex);

        private static string GetSingleVideoNameSample(string srcName, string numName)
        {
            return srcName.Replace("{标题}", "标题标题")
                .Replace("{番号}", numName)
                .Replace("{演员}", "小红")
                .Replace("{年份}", "2020");
        }

        private static string GetMultipleVideoNameSample(string srcName, string numName, string separator, int multipleNumSelectedIndex)
        {
            var partNum = (PartNum)multipleNumSelectedIndex switch
            {
                PartNum.Chinese => "一",
                PartNum.Arabic => "1",
                PartNum.English => "a",
                PartNum.CapsEnglish => "A",
                PartNum.Roman => "I",
                _ => "1"
            };

            return GetSingleVideoNameSample(srcName, numName: numName).Replace("{分隔符}", separator)
                .Replace("{序号}", partNum);
        }

        public static string GetNumNameSample(string srcName, int numNameFormatSelectedIndex, string letter = "abp", string num = "123")
        {
            var result = srcName.Replace("{字母}", letter)
                .Replace("{数字}", num);
            return (Models.Disk._115.Sort115Settings.NumNameCapFormat)numNameFormatSelectedIndex switch
            {
                Models.Disk._115.Sort115Settings.NumNameCapFormat.Upper => result.ToUpper(),
                Models.Disk._115.Sort115Settings.NumNameCapFormat.Lower => result.ToLower(),
                _ => result
            };
        }

        [RelayCommand]
        private void ChangedSingleVideoSaveExplorerItem()
        {
            var contentPage = new SelectedFolderPage();

            var newContentDialog = CreateSelectedFolderDialog(contentPage);

            newContentDialog.PrimaryButtonClick += (_, _) =>
            {
                Debug.WriteLine("点击了确定");
                Settings.SingleVideoSaveExplorerItem = contentPage.GetCurrentFolder();
            };
        }

        [RelayCommand]
        private void ChangedMultipleVideoSaveExplorerItem()
        {
            var contentPage = new SelectedFolderPage();

            var newContentDialog = CreateSelectedFolderDialog(contentPage);

            newContentDialog.PrimaryButtonClick += (_, _) =>
            {
                Debug.WriteLine("点击了确定");
                Settings.MultipleVideoSaveExplorerItem = contentPage.GetCurrentFolder();
            };
        }

        private CustomContentDialog CreateSelectedFolderDialog(SelectedFolderPage contentPage)
        {
            var tmpContent = _currentPage.Content;

            var newContentDialog = new CustomContentDialog(contentPage);

            var cancelHandler = new EventHandler<RoutedEventArgs>((_, _) =>
            {
                _currentPage.Content = tmpContent;
            });

            newContentDialog.PrimaryButtonClick += cancelHandler;
            newContentDialog.CancelButtonClick += cancelHandler;

            _currentPage.Content = newContentDialog;

            return newContentDialog;
        }
    }
}