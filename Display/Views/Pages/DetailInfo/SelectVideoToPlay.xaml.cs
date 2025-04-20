using System.Collections.Generic;
using System.Linq;
using DataAccess.Models.Entity;
using Display.Helper.Network;
using Display.Models.Api.OneOneFive.File;
using Display.Models.Dto.Media;
using Display.Models.Dto.OneOneFive;

namespace Display.Views.Pages.DetailInfo;

public sealed partial class SelectVideoToPlay
{
    private readonly List<FilesInfo> _pickCodeInfoList;
    public SelectVideoToPlay()
    {
        this.InitializeComponent();
    }
    public SelectVideoToPlay(List<FilesInfo> pickCodeInfoList)
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
        PlayVideos(ContentListView.SelectedItems.Cast<FilesInfo>().ToList());
    }

    private async void PlayVideos(IEnumerable<FilesInfo> infos)
    {
        var playItems = infos.Select(x => new MediaPlayItem(x)).ToList();

        await PlayVideoHelper.PlayVideo(playItems, this.XamlRoot, lastPage: this);
    }
}