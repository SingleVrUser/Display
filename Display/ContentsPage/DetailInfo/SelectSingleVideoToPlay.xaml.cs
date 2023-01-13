using Data;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage.DetailInfo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SelectSingleVideoToPlay : Page
    {
        private string trueName;

        private List<Datum> pickCodeInfoList { get; set; }
        public SelectSingleVideoToPlay()
        {
            this.InitializeComponent();
        }
        public SelectSingleVideoToPlay(List<Datum> pickCodeInfoList,string trueName)
        {
            this.InitializeComponent();
            this.pickCodeInfoList = pickCodeInfoList;
            this.trueName = trueName;
        }
    }
}
