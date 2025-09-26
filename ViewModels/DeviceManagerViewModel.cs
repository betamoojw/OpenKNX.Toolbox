using OpenKNX.Toolbox.Models;
using OpenKNX.Toolbox.Lib.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Management.Automation;
using Tmds.MDns;
using Wpf.Ui.Controls;
using OpenKNX.Toolbox.Lib.Helper;
using System.Diagnostics.Eventing.Reader;

namespace OpenKNX.Toolbox.ViewModels
{
    class DeviceManagerViewModel : INotifyPropertyChanged
    {
        public static DeviceManagerViewModel Instanz { get; } = new DeviceManagerViewModel();

        public ObservableCollection<DeviceModel> DeviceModels { get; } = new ObservableCollection<DeviceModel>();

        private DeviceModel? _selectedDevice;
        public DeviceModel? SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                _selectedDevice = value;
                Changed("SelectedDevice");
                FirmwareManagerViewModel.Instanz.SelectedApplication = value?.AppId ?? string.Empty;
            }
        }

        private int _updatesAvailable = 0;
        public int UpdatesAvailable
        {
            get => _updatesAvailable;
            set
            {
                _updatesAvailable = value;
                Changed("UpdatesAvailable");
            }
        }

        ServiceBrowser? _serviceBrowser;

        private Dictionary<string, string> AppToImage = new Dictionary<string, string>()
        {
            { "$A11E", "https://github.com/OpenKNX/OpenKNX/wiki/media/devices/REG1-Eth-V1.thumb.jpg" },
            { "$A401", "https://github.com/OpenKNX/OpenKNX/wiki/media/devices/REG1-Dali.thumb.jpg" }
        };

        public event PropertyChangedEventHandler? PropertyChanged;

        public void Init()
        {
            _serviceBrowser = new ServiceBrowser();
            _serviceBrowser.ServiceAdded += DeviceFound;
            _serviceBrowser.StartBrowse("_openknx._tcp");

            DeviceModels.CollectionChanged += DeviceModels_CollectionChanged;
        }

        private void DeviceModels_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateUpdatesAvailable();
        }

        private void DeviceFound(object? sender, ServiceAnnouncementEventArgs e)
        {
            DeviceModel model = new DeviceModel()
            {
                Name = e.Announcement.Hostname,
                IP = e.Announcement.Addresses.First().ToString(),
                IsIP = true
            };

            foreach (string txt in e.Announcement.Txt)
            {
                if (!txt.Contains("="))
                    continue;

                string key = txt.Split("=")[0];
                string value = txt.Split("=")[1];

                switch (key)
                {
                    case "configured":
                        model.Configured = value == "1";
                        break;
                    case "ota":
                        model.Port = int.Parse(value);
                        break;
                    case "address":
                        model.Address = value;
                        break;
                    case "firmware":
                        model.AppId = value;
                        if (AppToImage.ContainsKey(value))
                            model.ImageUrl = AppToImage[value];
                        else
                            model.ImageUrl = "http://icon-library.com/images/placeholder-icon/placeholder-icon-15.jpg";
                        break;
                    case "version":
                        model.FirmwareVersion = new SemanticVersion(value);
                        model.FirmwareVersionString = value;
                        break;
                    case "serial":
                        model.SerialNumber = value;
                        break;
                }
            }

            if (DeviceModels.Any(d => d.SerialNumber == model.SerialNumber))
                return;

            if (model.Port == 2040)
                model.Architecture = Lib.Data.ArchitectureType.RP2040;
            else if (model.Port == 3232)
                model.Architecture = Lib.Data.ArchitectureType.ESP32;
            else
                MainViewModel.Instanz.ShowError("Unbekannter Port", "Von einem Gerät wurde ein unbekannter Port angegeben: " + model.Port);

            ApplicationModel? app = FirmwareManagerViewModel.Instanz.Applications.FirstOrDefault(a => a.AppId == model.AppId);
            if (app != null)
            {
                model.FirmwareName = app.Name;
                model.UpdateVersion = GetNewstVersion(app, model.FirmwareVersion);
                if(model.UpdateVersion != null)
                    model.UpdateVersionString = model.UpdateVersion.ToString();
            } else
            {
                RepositoryMapping? map = GitHubAccess.GetRepoMappingByAppId(model.AppId);
                if(map != null)
                    model.FirmwareName = map.Name;
            }

                DeviceModels.Add(model);

            UpdateUpdatesAvailable();
        }

        private SemanticVersion? GetNewstVersion(ApplicationModel app, SemanticVersion? currentVersion)
        {
            if (currentVersion == null)
                return null;
            SemanticVersion? newest = null;
            foreach (ReleaseModel release in app.Releases)
            {
                SemanticVersion ver = new SemanticVersion(release.Version);
                if (newest == null || ver > newest)
                    newest = ver;
            }
            if (newest != null && newest > currentVersion)
                return newest;
            return null;
        }

        private void UpdateUpdatesAvailable()
        {
            UpdatesAvailable = DeviceModels.Count(d => d.UpdateVersion != null);
        }

        private void Changed(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
