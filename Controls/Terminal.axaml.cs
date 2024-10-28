using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OpenKNX.Toolbox.ViewModels;

namespace OpenKNX.Toolbox.Controls
{
    public partial class Terminal : UserControl
    {
        public Terminal()
        {
            InitializeComponent();
            this.DataContext = new TerminalViewModel();
        }
    }
}