using CommunityToolkit.Mvvm.ComponentModel;
using Display.Models;

namespace Display.ViewModels
{
    public partial class SortSettingsViewModel: Sort115Settings
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

        public string NumNameSample => GetNumNameSample(NumNameFormat,NumNameFormatSelectedIndex);
        public string SingleVideoNameSample => GetSingleVideoNameSample(SingleVideoNameFormat,numName: NumNameSample);

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



            return GetSingleVideoNameSample(srcName,numName: numName).Replace("{分隔符}", separator)
                .Replace("{序号}", partNum);
        }

        public static string GetNumNameSample(string srcName, int numNameFormatSelectedIndex, string letter="abp", string num = "123")
        {
            var result = srcName.Replace("{字母}", letter)
                .Replace("{数字}", num);
            return (NumNameCapFormat)numNameFormatSelectedIndex switch
                {
                    NumNameCapFormat.Upper => result.ToUpper(),
                    NumNameCapFormat.Lower => result.ToLower(),
                    _ => result
                };
        } 
    }
}