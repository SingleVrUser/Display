// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System.Collections.Generic;
using Display.Models.Dto.OneOneFive;

namespace Display.Views.Pages.OfflineDown;

public sealed partial class FailListPage
{
    private readonly List<AddTaskUrlInfo> _urlInfos;

    public FailListPage(List<AddTaskUrlInfo> urlInfos)
    {
        InitializeComponent();

        _urlInfos = urlInfos;

    }
}