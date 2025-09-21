using CommunityToolkit.Mvvm.Input;
using OpenKNX.Toolbox.Classes.Actions;
using OpenKNX.Toolbox.Lib.Models;
using OpenKNX.Toolbox.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace OpenKNX.Toolbox.Models
{
    public partial class ReleaseModel: INotifyPropertyChanged
    {
        public string Name { get; set; } = string.Empty;
        public DateTime PublishedAt { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public bool IsPrerelease { get; set; } = false;
        public SemanticVersion Version { get; set; }
        public string VersionString { get; set; } = string.Empty;
        private string AppId { get; set; } = string.Empty;

        private bool _isLocalAvailable = false;
        public bool IsLocalAvailable
        {
            get { return _isLocalAvailable; }
            set
            {
                _isLocalAvailable = value;
                Changed("IsLocalAvailable");
            }
        }

        public ReleaseModel(AppRelease release, string appId)
        {
            Name = release.Name;
            PublishedAt = release.PublishedAt;
            FileUrl = release.FileUrl;
            IsPrerelease = release.IsPrerelease;
            Version = release.Version ?? new SemanticVersion(0);
            VersionString = Version?.ToString() ?? "Unbekannt";
            AppId = appId;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void Changed(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        [RelayCommand]
        public void DownloadRelease()
        {
            string destination = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            destination = Path.Combine(destination, "OpenKNX.Toolbox", "Firmware", AppId, Version.ToString());
            if(!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }
            destination = Path.Combine(destination, Name);
            ActionsViewModel.Instanz.AddAction(new DownloadAction($"Download {Name}", FileUrl, destination));
        }
    }
}
