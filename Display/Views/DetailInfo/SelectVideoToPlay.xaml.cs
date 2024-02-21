using System.Collections.Generic;
using System.Linq;
using Display.Helper.Network;
using Display.Models.Data;
using Display.Models.Media;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views.DetailInfo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SelectVideoToPlay : Page
    {
        private readonly List<Datum> _pickCodeInfoList;
        public SelectVideoToPlay()
        {
            this.InitializeComponent();
        }
        public SelectVideoToPlay(List<Datum> pickCodeInfoList)
        {
            InitializeComponent();

            this._pickCodeInfoList = pickCodeInfoList;
        }

        public void PlayAllVideos()
        {
            PlayVideos(_pickCodeInfoList);
        }

        public void PlaySelectedVideos()
        {
            PlayVideos(ContentListView.SelectedItems.Cast<Datum>().ToList());
        }

        private async void PlayVideos(IEnumerable<Datum> infos)
        {
            var playItems = infos.Select(x=>new MediaPlayItem(x)).ToList();

            await PlayVideoHelper.PlayVideo(playItems, this.XamlRoot, lastPage: this);
        }
    }
}
