using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UpdateImagePath : Page
    {

        public string srcPath { get; private set; }
        public string dstPath { get; private set; }
        public string imagePath { get; }

        public UpdateImagePath(string newImagePath, string newSrcPath, string newDstPath)
        {
            srcPath = newSrcPath;
            dstPath = newDstPath;
            imagePath = newImagePath;

            this.InitializeComponent();

        }

        private string getDstImagePath(string newSrcPath, string newDstPath)
        {
            if (newSrcPath == "") return "😥";
            return imagePath.Replace(newSrcPath, newDstPath);
        }
    }
}
