
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;
using Display.Data;
using Display.Helper;
using Display.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage.DetailInfo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SelectVideoToPlay : Page
    {
        private List<Datum> pickCodeInfoList { get; set; }
        public SelectVideoToPlay()
        {
            this.InitializeComponent();
        }
        public SelectVideoToPlay(List<Datum> pickCodeInfoList)
        {
            InitializeComponent();

            this.pickCodeInfoList = pickCodeInfoList;
        }

        public void PlayAllVideos()
        {
            PlayVideos(pickCodeInfoList);
        }

        public void PlaySelectedVideos()
        {
            PlayVideos(ContentListView.SelectedItems.Cast<Datum>().ToList());
        }

        private async void PlayVideos(List<Datum> infos)
        {
            var playItems = infos.Select(x=>new MediaPlayItem(x.PickCode,x.Name,FilesInfo.FileType.File)).ToList();

            await PlayVideoHelper.PlayVideo(playItems, this.XamlRoot, lastPage: this);
        }
    }
}
