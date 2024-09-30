using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;

namespace OpenKNX.Toolbox.Views;

public partial class MainWindow : Window
{
    public static MainWindow? Instance = null; 
    public MainWindow()
    {
        InitializeComponent();
        Instance = this;
        this.Title += " - v" + string.Join('.', System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString().Split('.').Take(3));
    }
}