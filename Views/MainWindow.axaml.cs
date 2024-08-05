using System.ComponentModel;
using Avalonia.Controls;

namespace OpenKNX.Toolbox.Views;

public partial class MainWindow : Window
{
    public static MainWindow? Instance = null; 
    public MainWindow()
    {
        InitializeComponent();
        Instance = this;
    }
}