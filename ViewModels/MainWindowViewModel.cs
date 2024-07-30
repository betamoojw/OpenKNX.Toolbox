using OpenKNX.Toolbox.Lib.Helper;
using OpenKNX.Toolbox.Lib.Models;

namespace OpenKNX.Toolkit.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
#pragma warning disable CA1822 // Mark members as static
    public string Greeting => "Welcome to Avalonia!";
#pragma warning restore CA1822 // Mark members as static

    public ReleaseContentModel ReleaseContent { get; set;}


    public MainWindowViewModel()
    {
        ReleaseContent = ReleaseContentHelper.GetReleaseContent(@"C:\Users\mikeg\Desktop\test\LogicModule-Release-3.3.1\data\");
    }

}
