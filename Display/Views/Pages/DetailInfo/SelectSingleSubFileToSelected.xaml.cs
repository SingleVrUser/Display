using System.Collections.Generic;
using Display.Models.Dto.OneOneFive;
using Display.Models.Vo;

namespace Display.Views.Pages.DetailInfo;

public sealed partial class SelectSingleSubFileToSelected
{
    private string TrueName { get; }

    private List<SubInfo> SubInfoList { get; }

    public SelectSingleSubFileToSelected()
    {
        InitializeComponent();
    }

    public SelectSingleSubFileToSelected(List<SubInfo> subList, string trueName)
    {
        InitializeComponent();

        SubInfoList = subList;
        TrueName = trueName;
    }

}
