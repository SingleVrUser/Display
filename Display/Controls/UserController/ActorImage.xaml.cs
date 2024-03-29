using Display.Helper.Date;
using Display.Models.Data;
using Display.Views;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using Display.Models.Dto.OneOneFive;
using Display.Views.Pages;

namespace Display.Controls.UserController;

public sealed partial class ActorImage
{
    public ActorInfo ActorInfo;

    private readonly string _releaseTime;

    public ActorImage(ActorInfo actorInfo, string releaseTime)
    {
        InitializeComponent();

        ActorInfo = actorInfo;
        _releaseTime = releaseTime;

        //是否可点击
        if (string.IsNullOrEmpty(actorInfo.Name)) return;

        //是否喜欢
        if (actorInfo.IsLike == 1)
        {
            LikeFontIcon.Visibility = Visibility.Visible;
        }

        //右键菜单
        RootGrid.ContextFlyout = Resources["LikeMenuFlyout"] as MenuFlyout;

        //监听Pointer后改变鼠标
        RootGrid.PointerEntered += Grid_PointerEntered;
        RootGrid.PointerExited += Grid_PointerExited;

        //显示作品时年龄
        TryShowActorAge(actorInfo.Birthday);
    }

    private void TryShowActorAge(string birthday)
    {
        var ageDiff = DateHelper.CalculateTimeStrDiff(birthday, _releaseTime);

        if (ageDiff == TimeSpan.Zero) return;

        WorkYearDiffTextBlock.Text = ((int)ageDiff.TotalDays / 365).ToString();
        WorkYearDiffTextBlock.Visibility = Visibility.Visible;
    }


    public event RoutedEventHandler Click;
    private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
    {
        Click?.Invoke(sender, e);
    }

    private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
    }

    private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
    }

    private void LikeMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        var isLike = 0;

        //通过当前状态判断将要设置的值
        switch (LikeFontIcon.Visibility)
        {
            case Visibility.Visible:
                LikeFontIcon.Visibility = Visibility.Collapsed;
                break;
            case Visibility.Collapsed:
                LikeFontIcon.Visibility = Visibility.Visible;
                isLike = 1;
                break;
        }

        DataAccess.Update.UpdateSingleDataFromActorInfo(ActorInfo.Id.ToString(), "is_like", isLike.ToString());
    }

    private async void GetInfoMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        GetActorInfoProgressRing.Visibility = Visibility.Visible;

        var newInfo = await ActorsPage.GetActorInfo(ActorInfo);
        if (newInfo == null)
        {
            GetActorInfoProgressRing.Visibility = Visibility.Collapsed;
            return;
        }

        //更新头像
        if (!string.IsNullOrEmpty(newInfo.ProfilePath))
        {
            ActorInfo.ProfilePath = newInfo.ProfilePath;
        }

        //更新年龄
        TryShowActorAge(newInfo.Birthday);

        GetActorInfoProgressRing.Visibility = Visibility.Collapsed;
    }
}