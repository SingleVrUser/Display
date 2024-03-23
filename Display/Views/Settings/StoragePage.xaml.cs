using Display.Models.Data.Enums;
using Display.Models.Data;
using Display.Models.Settings.Options;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using Display.Helper.FileProperties.Name;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using System.IO;
using Microsoft.UI.Xaml;

namespace Display.Views.Settings;

public sealed partial class StoragePage : RootPage
{

    private SavePath[] _savePaths;

    public StoragePage()
    {
        this.InitializeComponent();

        InitOptions();
    }

    private void InitOptions()
    {

        void SaveSamePathAction()
        {
            ShowTeachingTip("选择目录与原目录相同，未修改");
        }

        // 保存路径
        _savePaths =
        [
            new SavePath(AppSettings.DataAccessSavePath)
            {
                Name = "数据文件",
                Own = SavePathEnum.Data,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SaveAction = newPath =>
                    UpdateDataAccessPath(AppSettings.DataAccessSavePath, newPath),
                SaveSamePathAction = SaveSamePathAction
            },
            new SavePath(AppSettings.ImageSavePath)
            {
                Name = "封面图片",
                Own = SavePathEnum.CoverImage,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SaveAction = newPath => UpdateCoverImagePath(AppSettings.ImageSavePath, newPath),
                SaveSamePathAction = SaveSamePathAction
            },
            new SavePath(AppSettings.ActorInfoSavePath)
            {
                Name = "演员头像",
                Own = SavePathEnum.ActorImage,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SaveAction = newPath => UpdateActorImagePath(AppSettings.ActorInfoSavePath, newPath),
                SaveSamePathAction = SaveSamePathAction
            },
            new SavePath(AppSettings.SubSavePath)
            {
                Name = "字幕文件",
                Own = SavePathEnum.Subtitles,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SaveAction = path => AppSettings.SubSavePath = path,
                SaveSamePathAction = SaveSamePathAction
            }
        ];

    }


    /// <summary>
    /// 尝试更新演员保存目录
    /// </summary>
    /// <param name="srcPath"></param>
    /// <param name="dstPath"></param>
    private async void UpdateActorImagePath(string srcPath, string dstPath)
    {
        //需要修改的地址
        AppSettings.ActorInfoSavePath = dstPath;

        //检查数据库的是否需要修改
        string imagePath = DataAccess.Get.GetOneActorProfilePath();
        if (string.IsNullOrEmpty(imagePath))
        {
            ShowTeachingTip("保存地址修改完成");
            return;
        }

        var imageRelativePath = Path.GetRelativePath(srcPath, imagePath);
        var isSrcPathError = imageRelativePath.Split('\\').Length > 2;

        // 数据库的图片地址无需修改
        if (!isSrcPathError && imagePath.Replace(srcPath, dstPath) == imagePath && File.Exists(imagePath)) return;

        //提醒修改数据文件
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = "提醒",
            PrimaryButtonText = "修改",
            CloseButtonText = "不修改"
        };

        var updateImagePathPage = new UpdateImagePath(imagePath, srcPath, dstPath);
        dialog.Content = updateImagePathPage;

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            //修改数据库图片地址
            DataAccess.Update.UpdateActorProfilePath(updateImagePathPage.SrcPath, updateImagePathPage.DstPath);
            ShowTeachingTip("修改完成，部分修改内容重启后生效");
        }
        else
        {
            ShowTeachingTip("演员保存地址修改完成");
        }
    }



    /// <summary>
    /// 尝试更新图片保存目录
    /// </summary>
    /// <param name="srcPath"></param>
    /// <param name="dstPath"></param>
    private async void UpdateCoverImagePath(string srcPath, string dstPath)
    {
        AppSettings.ImageSavePath = dstPath;

        //检查数据库的是否需要修改
        string imagePath = DataAccess.Get.GetOneImagePath();
        if (string.IsNullOrEmpty(imagePath))
        {
            ShowTeachingTip("图片保存地址修改完成");
            return;
        }

        var imageRelativePath = Path.GetRelativePath(srcPath, imagePath);
        var isSrcPathError = imageRelativePath.Split('\\').Length > 2;

        // 数据库的图片地址无需修改
        if (!isSrcPathError && imagePath.Replace(srcPath, dstPath) == imagePath && File.Exists(imagePath)) return;

        //提醒修改数据文件
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = "提醒",
            PrimaryButtonText = "修改",
            CloseButtonText = "不修改"
        };

        var updateImagePathPage = new UpdateImagePath(imagePath, srcPath, dstPath);
        dialog.Content = updateImagePathPage;

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            //修改数据库图片地址
            DataAccess.Update.UpdateAllImagePath(updateImagePathPage.SrcPath, updateImagePathPage.DstPath);
            ShowTeachingTip("修改完成，部分修改内容重启后生效");
        }
        else
        {
            ShowTeachingTip("图片保存地址修改完成");
        }
    }



    /// <summary>
    /// 更新数据库路径，同时更新保存路径中已存在的数据库文件
    /// </summary>
    /// <param name="src"></param>
    /// <param name="dst"></param>
    private async void UpdateDataAccessPath(string src, string dst)
    {
        AppSettings.DataAccessSavePath = dst;

        await UpdateDataAccessFile(src, dst);
    }


    /// <summary>
    /// 更新数据库文件
    /// </summary>
    /// <param name="src"></param>
    /// <param name="dst"></param>
    /// <returns></returns>
    private async Task UpdateDataAccessFile(string src, string dst)
    {
        ContentDialog dialog = new()
        {
            XamlRoot = XamlRoot,
            Title = "确认修改",
            PrimaryButtonText = "确认",
            CloseButtonText = "不修改",
            DefaultButton = ContentDialogButton.Primary
        };

        //内容
        RichTextBlock textHighlightingRichTextBlock = new();

        Paragraph paragraph = new();
        paragraph.Inlines.Add(new Run { Text = "修改将进行以下操作：" });
        paragraph.Inlines.Add(new LineBreak());
        paragraph.Inlines.Add(new Run { Text = "复制原数据文件到目标目录（如果目标目录下没有数据文件）" });

        textHighlightingRichTextBlock.Blocks.Add(paragraph);

        dialog.Content = textHighlightingRichTextBlock;

        dialog.Content = textHighlightingRichTextBlock;
        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary) return;

        var dstDbFilepath = DataAccess.NewDbPath(dst);

        var textFileExists = "数据文件已存在，未复制原数据文件";
        if (dstDbFilepath != null && !File.Exists(dstDbFilepath))
        {
            File.Copy(DataAccess.NewDbPath(src), dstDbFilepath, false);
            textFileExists = "原数据文件已复制到指定目录";
        }

        RichTextBlock successRichTextBlock = new();
        Paragraph successParagraph = new();
        successParagraph.Inlines.Add(new Run { Text = "数据文件存放目录修改完成，重启后生效。" });
        successParagraph.Inlines.Add(new LineBreak());
        successParagraph.Inlines.Add(new Run { Text = textFileExists, Foreground = new SolidColorBrush(Colors.OrangeRed) });
        successParagraph.Inlines.Add(new LineBreak());
        successParagraph.Inlines.Add(new Run { Text = "如需删除原数据文件，请关闭应用后自行删除" });
        successRichTextBlock.Blocks.Add(successParagraph);

        ShowTeachingTip(successRichTextBlock);
        await Task.Delay(3000);
        FileMatch.LaunchFolder(src);
    }

}