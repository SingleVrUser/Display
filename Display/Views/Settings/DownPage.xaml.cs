using System;
using System.Net;
using Display.Models.Data;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Text.RegularExpressions;
using Windows.Storage.Pickers;
using Display.Helper.FileProperties.Name;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.Views.Settings;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class DownPage : Page
{
    public DownPage()
    {
        this.InitializeComponent();
    }


    private async void BitCometSettingsCheck_Click(object sender, RoutedEventArgs e)
    {
        var bitCometSettings = CheckBitCometSettingsFormat(BitCometDownApiTextBox.Text);

        if (bitCometSettings == null)
            return;

        BitCometCheckStatus.status = Status.Doing;

        var isOk = await IsBitCometSettingOK(bitCometSettings.UserName, bitCometSettings.Password, bitCometSettings.ApiUrl);

        BitCometCheckStatus.status = isOk ? Status.Success : Status.Error;
    }


    private DownApiSettings CheckBitCometSettingsFormat(string fullApiUrl)
    {
        DownApiSettings bitCometSettings = new();

        if (string.IsNullOrWhiteSpace(fullApiUrl))
        {
            //ShowTeachingTip("���벻��Ϊ��");
            Debug.WriteLine("���벻��Ϊ��");

            return null;
        }

        var isMatch = Regex.Match(fullApiUrl, "^(https?://)(\\w+):(\\w+)@([\\w.]+:(\\d+))/?$");

        if (!isMatch.Success)
        {
            //ShowTeachingTip("�����ʽ�Ƿ���ȷ");
            Debug.WriteLine("�����ʽ�Ƿ���ȷ");
            return null;
        }

        bitCometSettings.ApiUrl = $"{isMatch.Groups[1].Value}{isMatch.Groups[4].Value}";
        bitCometSettings.UserName = isMatch.Groups[2].Value;
        bitCometSettings.Password = isMatch.Groups[3].Value;

        return bitCometSettings;
    }


    /// <summary>
    /// ����BitComet����
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BitCometSettingsSave_Click(object sender, RoutedEventArgs e)
    {
        var bitCometSettings = CheckBitCometSettingsFormat(BitCometDownApiTextBox.Text);

        if (bitCometSettings == null)
            return;

        AppSettings.BitCometSettings = bitCometSettings;
    }

    private async void BitCometSavePathChange_Click(object sender, RoutedEventArgs e)
    {
        FolderPicker folderPicker = new();
        folderPicker.FileTypeFilter.Add("*");
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.AppMainWindow);
        folderPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

        var folder = await folderPicker.PickSingleFolderAsync();

        if (folder == null) return;

        if (folder.Path == AppSettings.BitCometSavePath)
        {
            //ShowTeachingTip("ѡ��Ŀ¼��ԭĿ¼��ͬ��δ�޸�");
            Debug.WriteLine("ѡ��Ŀ¼��ԭĿ¼��ͬ��δ�޸�");
        }
        else
        {
            BitCometSavePathTextBox.Text = folder.Path;

            AppSettings.BitCometSavePath = folder.Path;
        }
    }

    private void BitCometSavePathClear_Click(object sender, RoutedEventArgs e)
    {
        AppSettings.BitCometSavePath = string.Empty;
        BitCometSavePathTextBox.Text = null;
    }


    private void BitCometSavePathOpen_Click(object sender, RoutedEventArgs e)
    {
        var bitCometSavePath = AppSettings.BitCometSavePath;

        if (string.IsNullOrEmpty(bitCometSavePath))
            return;
        FileMatch.LaunchFolder(bitCometSavePath);
    }

    private async void Aria2SettingsCheck_Click(object sender, RoutedEventArgs e)
    {
        var aria2Settings = CheckAria2SettingsFormat(Aria2DownApiTextBox.Text);

        if (aria2Settings == null)
            return;

        Aria2CheckStatus.status = Status.Doing;

        var isOK = await IsAriannaSettingOK(aria2Settings.Password, aria2Settings.ApiUrl);

        Aria2CheckStatus.status = isOK ? Status.Success : Status.Error;
    }

    private void Aria2tSettingsSave_Click(object sender, RoutedEventArgs e)
    {
        var aria2Settings = CheckAria2SettingsFormat(Aria2DownApiTextBox.Text);

        if (aria2Settings == null)
            return;

        AppSettings.Aria2Settings = aria2Settings;
    }
    private async void Aria2SavePathChange_Click(object sender, RoutedEventArgs e)
    {
        FolderPicker folderPicker = new();
        folderPicker.FileTypeFilter.Add("*");
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.AppMainWindow);
        folderPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

        var folder = await folderPicker.PickSingleFolderAsync();

        if (folder == null) return;

        if (folder.Path == AppSettings.Aria2SavePath)
        {
            //ShowTeachingTip("ѡ��Ŀ¼��ԭĿ¼��ͬ��δ�޸�");
            Debug.WriteLine("ѡ��Ŀ¼��ԭĿ¼��ͬ��δ�޸�");
        }
        else
        {
            Aria2SavePathTextBox.Text = folder.Path;

            AppSettings.Aria2SavePath = folder.Path;
        }
    }

    private void Aria2SavePathClear_Click(object sender, RoutedEventArgs e)
    {
        AppSettings.Aria2SavePath = string.Empty;
        Aria2SavePathTextBox.Text = null;
    }

    private void Aria2SavePathOpen_Click(object sender, RoutedEventArgs e)
    {
        var Aria2SavePath = AppSettings.Aria2SavePath;

        if (Aria2SavePath == null)
            return;
        FileMatch.LaunchFolder(Aria2SavePath);
    }

    private async Task<bool> IsAriannaSettingOK(string pwd, string url)
    {
        var isOK = false;

        Aria2Request requclass = new()
        {
            jsonrpc = "2.0",
            method = "aria2.getVersion",
            id = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString(),
            _params = [pwd]
        };

        var myContent = JsonConvert.SerializeObject(requclass);

        myContent = myContent.Replace("_params", "params");

        var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
        var Content = new StringContent(myContent, Encoding.UTF8, "application/json");

        try
        {
            var rep = await httpClient.PostAsync(url, Content);

            if (rep.IsSuccessStatusCode)
            {
                var content = await rep.Content.ReadAsStringAsync();

                if (!string.IsNullOrWhiteSpace(content))
                    isOK = true;
            }
            else
            {
                if (rep.StatusCode == HttpStatusCode.BadRequest)
                {
                    //ShowTeachingTip("����ʧ��", "����Secret");
                    Debug.WriteLine("����ʧ��\", \"����Secret");
                }
                else if (rep.StatusCode == HttpStatusCode.NotFound)
                {
                    //ShowTeachingTip("�����ַ����", "���� ��ַ�Ƿ��� \"/jsonrpc\" ��β");
                    Debug.WriteLine("�����ַ����\", \"���� ��ַ�Ƿ��� \\\"/jsonrpc\\\" ��β");
                }
                else if (rep.StatusCode == HttpStatusCode.BadGateway)
                {
                    //ShowTeachingTip("�˿�����", "����˿�");
                    Debug.WriteLine("�˿�����\", \"����˿�");
                }
            }
        }
        catch (HttpRequestException ex)
        {
            //����
            //ShowTeachingTip("����ʧ��", $"{ex.Message}�������ַ�Ͷ˿�");
            Debug.WriteLine("����ʧ��\", $\"{ex.Message}�������ַ�Ͷ˿�");
        }
        catch (TaskCanceledException ex)
        {
            //����
            //ShowTeachingTip("����ʱ", $"{ex.Message}�������ַ");
            Debug.WriteLine("����ʱ\", $\"{ex.Message}�������ַ");
        }


        return isOK;
    }


    private DownApiSettings CheckAria2SettingsFormat(string fullApiUrl)
    {
        DownApiSettings aria2Settings = new();

        if (string.IsNullOrWhiteSpace(fullApiUrl))
        {
            //ShowTeachingTip("���벻��Ϊ��");
            Debug.WriteLine("���벻��Ϊ��");

            return null;
        }

        var isMatch = Regex.Match(fullApiUrl, "^https?://(\\w+:\\w+)@[\\w.]+(:\\d+)?/jsonrpc$");

        if (!isMatch.Success)
        {
            //ShowTeachingTip("�����ʽ�Ƿ���ȷ");
            Debug.WriteLine("�����ʽ�Ƿ���ȷ");
            return null;
        }

        aria2Settings.ApiUrl = isMatch.Value;
        //aria2Settings.UserName = isMatch.Groups[2].Value;
        aria2Settings.Password = isMatch.Groups[1].Value;

        return aria2Settings;
    }


    private async Task<bool> IsBitCometSettingOK(string user, string pwd, string url)
    {
        bool isOK = false;

        var handler = new HttpClientHandler()
        {
            UseDefaultCredentials = true,
            Credentials = new NetworkCredential(user, pwd)
        };

        HttpClient httpClient = new(handler);

        try
        {
            var rep = await httpClient.GetAsync(url + "/panel/task_add_httpftp");

            if (rep.IsSuccessStatusCode)
            {
                var content = await rep.Content.ReadAsStringAsync();

                if (!string.IsNullOrWhiteSpace(content))
                    isOK = true;
            }
            else
            {
                if (rep.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    //ShowTeachingTip("��֤ʧ��", "�����û���������");
                    Debug.WriteLine("��֤ʧ��\", \"�����û���������");
                }
            }
        }
        catch (UriFormatException ex)
        {
            //����
            //ShowTeachingTip("��ҳ����ʧ��", $"{ex.Message}�������ַ�Ͷ˿�");
            Debug.WriteLine("��ҳ����ʧ��\", $\"{ex.Message}�������ַ�Ͷ˿�");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }

        return isOK;
    }


}