using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OpenKNX.Toolbox.Controls
{
    public partial class Creator : UserControl
    {
        public Creator()
        {
            InitializeComponent();
            this.DataContext = new ViewModels.CreatorViewModel();
        }
    }
}