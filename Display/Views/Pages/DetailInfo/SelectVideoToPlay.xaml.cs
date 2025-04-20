using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataAccess.Models.Entity;
using Display.Helper.Network;
using Display.Models.Api.OneOneFive.File;
using Display.Models.Dto.Media;
using Display.Models.Dto.OneOneFive;
using Display.Models.Vo.OneOneFive;
using FileInfo = DataAccess.Models.Entity.FileInfo;

namespace Display.Views.Pages.DetailInfo;

public sealed partial class SelectVideoToPlay
{
    private readonly List<FileInfo> _pickCodeInfoList;
    public SelectVideoToPlay()
    {
        this.InitializeComponent();
    }
    public SelectVideoToPlay(List<FileInfo> pickCodeInfoList)
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
        PlayVideos(ContentListView.SelectedItems.Cast<FileInfo>().ToList());
    }

    private async void PlayVideos(IEnumerable<FileInfo> infos)
    {
        var playItems = infos.Select(x => new MediaPlayItem(new DetailFileInfo(x))).ToList();

        await PlayVideoHelper.PlayVideo(playItems, this.XamlRoot, lastPage: this);
    }
}