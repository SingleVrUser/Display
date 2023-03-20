using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Collections.ObjectModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Display.ContentsPage.Import115DataToLocalDataAccess
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InputCidManually : Page
    {
        public ObservableCollection<string> CidList = new();

        public InputCidManually()
        {
            this.InitializeComponent();
        }

        private void FindCidTip_HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            FindCidTeachingTip.IsOpen = true;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string addCid = InputCid_TextBox.Text;

            tryAddCid(addCid);
        }

        private void tryAddCid(string addCid)
        {
            //long n;
            bool is_num = long.TryParse(addCid, out _);

            if (!is_num)
            {
                InputCid_TextBox.Text = "";
                InputCid_TextBox.PlaceholderText = "cid为纯数字，且在20位以下";
                return;
            }

            if (!CidList.Contains(addCid))
            {
                CidList.Add(addCid);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            string deleteCid = (sender as Button).DataContext as string;
            CidList.Remove(deleteCid);
        }

        private void InputCid_TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            //如果不是回车
            if (e.Key != Windows.System.VirtualKey.Enter)
            {
                return;
            }
            tryAddCid((sender as TextBox).Text);
        }
    }
}
