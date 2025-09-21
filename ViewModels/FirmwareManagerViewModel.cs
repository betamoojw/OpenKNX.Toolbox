using CommunityToolkit.Mvvm.Input;
using OpenKNX.Toolbox;
using OpenKNX.Toolbox.Lib.Data;
using OpenKNX.Toolbox.Lib.Helper;
using OpenKNX.Toolbox.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKNX.Toolbox.ViewModels
{
    public partial class FirmwareManagerViewModel : INotifyPropertyChanged
    {
        public static FirmwareManagerViewModel Instanz { get; } = new FirmwareManagerViewModel();

        public ObservableCollection<ApplicationModel> Applications { get; set; } = new();

        private string _selectedApplication = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string SelectedApplication
        {
            get { return _selectedApplication; }
            set
            {
                _selectedApplication = value;
                Changed("SelectedApplication");
            }
        }
        
        private void Instanz_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(sender != null && e.PropertyName == "SelectedDevice")
            {
                SelectedApplication = ((MainViewModel)sender).SelectedDevice?.AppId ?? string.Empty;
            }
        }

        public async Task Init()
        {
            MainViewModel.Instanz.PropertyChanged += Instanz_PropertyChanged;
            await UpdateFirmwareList();
            UpdateLocalList();
        }

        public void UpdateLocalList()
        {
            foreach(var app in Applications)
            {
                foreach(var release in app.Releases)
                {
                    string releasePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    releasePath = Path.Combine(releasePath, "OpenKNX.Toolbox", "Firmware", app.AppId, release.Version.ToString());

                    release.IsLocalAvailable = Directory.Exists(releasePath);
                }
            }
        }

        public async Task UpdateFirmwareList()
        {
            List<Lib.Models.Application> apps = await GitHubAccess.GetOpenKnxApplicationsAsync();

            Applications.Clear();

            foreach (var app in apps)
            {
                ApplicationModel appModel = new(app);

                foreach(var rel in app.Releases)
                {
                    ReleaseModel releaseModel = new(rel, appModel.AppId);
                    appModel.Releases.Add(releaseModel);
                }

                Applications.Add(appModel);
            }
        }

        private void Changed(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
