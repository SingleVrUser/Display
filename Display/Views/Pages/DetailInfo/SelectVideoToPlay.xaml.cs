using System.Collections.Generic;
using System.Linq;
using Display.Helper.Network;
using Display.Models.Api.OneOneFive.File;
using Display.Models.Dto.Media;
using Display.Models.Dto.OneOneFive;

namespace Display.Views.Pages.DetailInfo;

public sealed partial class SelectVideoToPlay
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
        var playItems = infos.Select(x => new MediaPlayItem(x)).ToList();

        await PlayVideoHelper.PlayVideo(playItems, this.XamlRoot, lastPage: this);
    }
}