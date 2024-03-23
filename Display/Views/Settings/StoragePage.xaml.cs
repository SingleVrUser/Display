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
            ShowTeachingTip("ѡ��Ŀ¼��ԭĿ¼��ͬ��δ�޸�");
        }

        // ����·��
        _savePaths =
        [
            new SavePath(AppSettings.DataAccessSavePath)
            {
                Name = "�����ļ�",
                Own = SavePathEnum.Data,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SaveAction = newPath =>
                    UpdateDataAccessPath(AppSettings.DataAccessSavePath, newPath),
                SaveSamePathAction = SaveSamePathAction
            },
            new SavePath(AppSettings.ImageSavePath)
            {
                Name = "����ͼƬ",
                Own = SavePathEnum.CoverImage,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SaveAction = newPath => UpdateCoverImagePath(AppSettings.ImageSavePath, newPath),
                SaveSamePathAction = SaveSamePathAction
            },
            new SavePath(AppSettings.ActorInfoSavePath)
            {
                Name = "��Աͷ��",
                Own = SavePathEnum.ActorImage,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SaveAction = newPath => UpdateActorImagePath(AppSettings.ActorInfoSavePath, newPath),
                SaveSamePathAction = SaveSamePathAction
            },
            new SavePath(AppSettings.SubSavePath)
            {
                Name = "��Ļ�ļ�",
                Own = SavePathEnum.Subtitles,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SaveAction = path => AppSettings.SubSavePath = path,
                SaveSamePathAction = SaveSamePathAction
            }
        ];

    }


    /// <summary>
    /// ���Ը�����Ա����Ŀ¼
    /// </summary>
    /// <param name="srcPath"></param>
    /// <param name="dstPath"></param>
    private async void UpdateActorImagePath(string srcPath, string dstPath)
    {
        //��Ҫ�޸ĵĵ�ַ
        AppSettings.ActorInfoSavePath = dstPath;

        //������ݿ���Ƿ���Ҫ�޸�
        string imagePath = DataAccess.Get.GetOneActorProfilePath();
        if (string.IsNullOrEmpty(imagePath))
        {
            ShowTeachingTip("�����ַ�޸����");
            return;
        }

        var imageRelativePath = Path.GetRelativePath(srcPath, imagePath);
        var isSrcPathError = imageRelativePath.Split('\\').Length > 2;

        // ���ݿ��ͼƬ��ַ�����޸�
        if (!isSrcPathError && imagePath.Replace(srcPath, dstPath) == imagePath && File.Exists(imagePath)) return;

        //�����޸������ļ�
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = "����",
            PrimaryButtonText = "�޸�",
            CloseButtonText = "���޸�"
        };

        var updateImagePathPage = new UpdateImagePath(imagePath, srcPath, dstPath);
        dialog.Content = updateImagePathPage;

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            //�޸����ݿ�ͼƬ��ַ
            DataAccess.Update.UpdateActorProfilePath(updateImagePathPage.SrcPath, updateImagePathPage.DstPath);
            ShowTeachingTip("�޸���ɣ������޸�������������Ч");
        }
        else
        {
            ShowTeachingTip("��Ա�����ַ�޸����");
        }
    }



    /// <summary>
    /// ���Ը���ͼƬ����Ŀ¼
    /// </summary>
    /// <param name="srcPath"></param>
    /// <param name="dstPath"></param>
    private async void UpdateCoverImagePath(string srcPath, string dstPath)
    {
        AppSettings.ImageSavePath = dstPath;

        //������ݿ���Ƿ���Ҫ�޸�
        string imagePath = DataAccess.Get.GetOneImagePath();
        if (string.IsNullOrEmpty(imagePath))
        {
            ShowTeachingTip("ͼƬ�����ַ�޸����");
            return;
        }

        var imageRelativePath = Path.GetRelativePath(srcPath, imagePath);
        var isSrcPathError = imageRelativePath.Split('\\').Length > 2;

        // ���ݿ��ͼƬ��ַ�����޸�
        if (!isSrcPathError && imagePath.Replace(srcPath, dstPath) == imagePath && File.Exists(imagePath)) return;

        //�����޸������ļ�
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            Title = "����",
            PrimaryButtonText = "�޸�",
            CloseButtonText = "���޸�"
        };

        var updateImagePathPage = new UpdateImagePath(imagePath, srcPath, dstPath);
        dialog.Content = updateImagePathPage;

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            //�޸����ݿ�ͼƬ��ַ
            DataAccess.Update.UpdateAllImagePath(updateImagePathPage.SrcPath, updateImagePathPage.DstPath);
            ShowTeachingTip("�޸���ɣ������޸�������������Ч");
        }
        else
        {
            ShowTeachingTip("ͼƬ�����ַ�޸����");
        }
    }



    /// <summary>
    /// �������ݿ�·����ͬʱ���±���·�����Ѵ��ڵ����ݿ��ļ�
    /// </summary>
    /// <param name="src"></param>
    /// <param name="dst"></param>
    private async void UpdateDataAccessPath(string src, string dst)
    {
        AppSettings.DataAccessSavePath = dst;

        await UpdateDataAccessFile(src, dst);
    }


    /// <summary>
    /// �������ݿ��ļ�
    /// </summary>
    /// <param name="src"></param>
    /// <param name="dst"></param>
    /// <returns></returns>
    private async Task UpdateDataAccessFile(string src, string dst)
    {
        ContentDialog dialog = new()
        {
            XamlRoot = XamlRoot,
            Title = "ȷ���޸�",
            PrimaryButtonText = "ȷ��",
            CloseButtonText = "���޸�",
            DefaultButton = ContentDialogButton.Primary
        };

        //����
        RichTextBlock textHighlightingRichTextBlock = new();

        Paragraph paragraph = new();
        paragraph.Inlines.Add(new Run { Text = "�޸Ľ��������²�����" });
        paragraph.Inlines.Add(new LineBreak());
        paragraph.Inlines.Add(new Run { Text = "����ԭ�����ļ���Ŀ��Ŀ¼�����Ŀ��Ŀ¼��û�������ļ���" });

        textHighlightingRichTextBlock.Blocks.Add(paragraph);

        dialog.Content = textHighlightingRichTextBlock;

        dialog.Content = textHighlightingRichTextBlock;
        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary) return;

        var dstDbFilepath = DataAccess.NewDbPath(dst);

        var textFileExists = "�����ļ��Ѵ��ڣ�δ����ԭ�����ļ�";
        if (dstDbFilepath != null && !File.Exists(dstDbFilepath))
        {
            File.Copy(DataAccess.NewDbPath(src), dstDbFilepath, false);
            textFileExists = "ԭ�����ļ��Ѹ��Ƶ�ָ��Ŀ¼";
        }

        RichTextBlock successRichTextBlock = new();
        Paragraph successParagraph = new();
        successParagraph.Inlines.Add(new Run { Text = "�����ļ����Ŀ¼�޸���ɣ���������Ч��" });
        successParagraph.Inlines.Add(new LineBreak());
        successParagraph.Inlines.Add(new Run { Text = textFileExists, Foreground = new SolidColorBrush(Colors.OrangeRed) });
        successParagraph.Inlines.Add(new LineBreak());
        successParagraph.Inlines.Add(new Run { Text = "����ɾ��ԭ�����ļ�����ر�Ӧ�ú�����ɾ��" });
        successRichTextBlock.Blocks.Add(successParagraph);

        ShowTeachingTip(successRichTextBlock);
        await Task.Delay(3000);
        FileMatch.LaunchFolder(src);
    }

}