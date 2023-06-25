using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Display.Data;
using Display.Models;

namespace Display.ViewModels
{
    internal partial class UploadViewModel : ObservableObject
    {
        private static UploadViewModel _uploadVm;
        public static UploadViewModel Instance => _uploadVm ??= new UploadViewModel();

        public ObservableCollection<UploadSubItem> UploadCollection;

        private static string _saveFolderCid =>  AppSettings.SavePath115Cid;

        private readonly HttpClient _client;



    }
}
