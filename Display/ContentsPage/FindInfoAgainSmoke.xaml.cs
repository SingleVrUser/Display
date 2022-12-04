// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Data;
using Display.Control;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FindInfoAgainSmoke : Page
    {
        private string cidName { get; set; }

        private ObservableCollection<VideoInfo> VideoInfos = new();

        public FindInfoAgainSmoke(string cidName)
        {
            this.InitializeComponent();

            this.cidName = cidName;

            this.Loaded += PageLoaded;
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            FindInfos();
        }

        private GetInfoFromNetwork _searchNetwork;

        private GetInfoFromNetwork SearchNetwork
        {
            get
            {
                if (_searchNetwork == null)
                    _searchNetwork = new GetInfoFromNetwork();
                return _searchNetwork;
            }
            set => _searchNetwork = value;
        }

        async void FindInfos()
        {
            List<VideoInfo> infos = new();

            ReCheckProgressRing.Visibility = Visibility.Visible;

            if (!cidName.ToLower().Contains("fc") && JavBus_CheckBox.IsChecked == true)
            {
                var info = await SearchNetwork.SearchInfoFromJavBus(cidName);
                if(info!=null)
                    infos.Add(info);
            }
            if (!cidName.ToLower().Contains("fc") && JavLibDmm_CheckBox.IsChecked == true)
            {
                var info = await SearchNetwork.SearchInfoFromLibreDmm(cidName);
                if(info!=null)
                    infos.Add(info);
            }


            if (cidName.ToLower().Contains("fc") && Fc2Hub_CheckBox.IsChecked == true)
            {
                var info = await SearchNetwork.SearchInfoFromFc2Hub(cidName);
                if (info != null)
                    infos.Add(info);
            }
            if (JavDB_CheckBox.IsChecked == true)
            {
                var info = await SearchNetwork.SearchInfoFromJavDB(cidName);
                if (info != null)
                    infos.Add(info);
            }

            if (infos.Count > 0)
            {
                VideoInfos.Clear();
                infos.ForEach(info => VideoInfos.Add(info));
            }

            ReCheckProgressRing.Visibility = Visibility.Collapsed;

        }

        private void RefreshButtonClicked(object sender, RoutedEventArgs e)
        {
            FindInfos();
        }
    }
}
