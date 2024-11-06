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
        var assembly = System.Reflection.Assembly.GetEntryAssembly();
        if(assembly == null) return;
        var vers = assembly.GetName().Version;
        if(vers == null) return;
        this.Title += " - v" + string.Join('.', vers.ToString().Split('.').Take(3));
    }
}