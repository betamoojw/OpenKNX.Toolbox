using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OpenKNX.Toolbox.ViewModels;

namespace OpenKNX.Toolbox.Controls
{
    public partial class Filetransfer : UserControl
    {
        public Filetransfer()
        {
            InitializeComponent();
            this.DataContext = new FiletransferViewModel();
        }
    }
}