// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Data;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage.DetailInfo;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SelectSingleSubFileToSelected : Page
{
    private string trueName;


    private List<SubInfo> subInfoList { get; set; }

    public SelectSingleSubFileToSelected()
    {
        this.InitializeComponent();
    }

    public SelectSingleSubFileToSelected(List<SubInfo> subList,string trueName)
    {
        this.InitializeComponent();

        subInfoList = subList;
        this.trueName = trueName;
    }

}
