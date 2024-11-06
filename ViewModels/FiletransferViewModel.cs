using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using OpenKNX.Toolbox.Lib.Helper;
using OpenKNX.Toolbox.Lib.Platforms;

namespace OpenKNX.Toolbox.ViewModels;

public partial class FiletransferViewModel : ViewModelBase, INotifyPropertyChanged
{
    public ObservableCollection<PlatformDevice> Devices { get; set; } = new ();

    private PlatformDevice? _selectedPlatformDevice { get; set; }
    public PlatformDevice? SelectedPlatformDevice
    {
        get { return _selectedPlatformDevice; }
        set { 
            _selectedPlatformDevice = value;
            NotifyPropertyChanged("SelectedPlatformDevice");
            CanOpenPutty = value != null;
        }
    }

    private bool _canOpenPutty = false;
    public bool CanOpenPutty
    {
        get { return _canOpenPutty; }
        set {
            _canOpenPutty = value;
            NotifyPropertyChanged("CanOpenPutty");
        }
    }

    public async Task UpdatePlatformDevices()
    {
        Devices.Clear();
        foreach (var device in await PlatformHelper.GetDevices())
            Devices.Add(device);
    }
    
    public void OpenPutty()
    {
        string url = $"putty ";
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            using var proc = new Process { StartInfo = { UseShellExecute = true, FileName = url } };
            proc.Start();
            return;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("x-www-browser", url);
            return;
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged; 
    private void NotifyPropertyChanged(string propertyName = "")  
    {  
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }  
}