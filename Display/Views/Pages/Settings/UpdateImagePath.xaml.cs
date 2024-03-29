using Microsoft.UI.Xaml.Controls;


namespace Display.Views.Settings
{
    public sealed partial class UpdateImagePath : Page
    {
        public string SrcPath { get; private set; }
        public string DstPath { get; private set; }
        public string ImagePath { get; }

        public UpdateImagePath(string newImagePath, string newSrcPath, string newDstPath)
        {
            SrcPath = newSrcPath;
            DstPath = newDstPath;
            ImagePath = newImagePath;

            this.InitializeComponent();

        }

        private string GetDstImagePath(string newSrcPath, string newDstPath)
        {
            return newSrcPath == string.Empty ? "😥" : ImagePath.Replace(newSrcPath, newDstPath);
        }
    }
}
