using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using Display.Data;

namespace Display.Models
{
    public partial class Sort115HomeModel:ObservableObject
    {
        public FilesInfo Info;

        [ObservableProperty]
        private string _destinationName;

        [ObservableProperty]
        private string _destinationPathName;

        [ObservableProperty]
        private Status _status = Status.beforeStart;

        public Sort115HomeModel(FilesInfo info)
        {
            Info = info;
        }

        public string Format;

        public int Index;
    }
}
