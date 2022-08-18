using Data;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ActorsPage : Page
    {
        public ObservableCollection<ActorsInfo> actorinfo = new();
        Dictionary<string, List<string>> ActorsInfoDict = new();

        public ActorsPage()
        {
            this.InitializeComponent();

            BasicGridView.ItemsSource = actorinfo;
            Page_Loaded();
        }

        private async void Page_Loaded()
        {
            ProgressRing.IsActive = true;
            List<VideoInfo> VideoInfoList = await Task.Run(() => DataAccess.LoadAllVideoInfo(-1));

            foreach (var VideoInfo in VideoInfoList)
            {
                string actor_str = VideoInfo.actor;

                var actor_list = actor_str.Split(",");
                foreach (var actor in actor_list)
                {
                    //当前名称不存在
                    if (!ActorsInfoDict.ContainsKey(actor))
                    {
                        ActorsInfoDict.Add(actor, new List<string>());
                    }
                    ActorsInfoDict[actor].Add(VideoInfo.truename);
                }

            }

            foreach (var item in ActorsInfoDict)
            {
                actorinfo.Add(new ActorsInfo
                {
                    name = item.Key,
                    count = item.Value.Count,
                });
            }

            ProgressRing.IsActive = false;
        }

        /// <summary>
        /// 跳转至演员详情页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BasicGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (BasicGridView.ContainerFromItem(e.ClickedItem) is GridViewItem container)
            {
                var actorinfo = container.Content as ActorsInfo;
                string[] TypeAndName = { "actor", actorinfo.name };
                Frame.Navigate(typeof(ActorInfoPage), TypeAndName);
            }
        }

        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
        }

        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
        }
    }


}
