using CommunityToolkit.Mvvm.Input;
using OpenKNX.Toolbox.Classes.Actions;
using OpenKNX.Toolbox.Lib.Data;
using OpenKNX.Toolbox.Lib.Platforms;
using OpenKNX.Toolbox.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace OpenKNX.Toolbox.Models
{
    public partial class DeviceModel
    {
        public string Name { get; set; } = string.Empty;
        public string FirmwareName { get; set; } = string.Empty;
        public string FirmwareVersionString { get; set; } = string.Empty;
        public SemanticVersion? FirmwareVersion { get; set; }
        public string IP { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string AppId { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public bool Configured { get; set; } = false;
        public int Port { get; set; } = 0;
        public string SerialNumber { get; set; } = string.Empty;
        public SemanticVersion? UpdateVersion { get; set; } = null;
        public string UpdateVersionString { get; set; } = string.Empty;
        public bool IsIP { get; set; } = false;
        public ArchitectureType Architecture { get; set; }

        [RelayCommand]
        public void DoOpenWebsite()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = $"http://{IP}",
                UseShellExecute = true // wichtig für .NET Core / .NET 5+
            });
        }

        [RelayCommand]
        public void DoUpdate()
        {
            if(UpdateVersion == null)
            {
                MainViewModel.Instanz.ShowError("Update nicht möglich", "Es ist keine Update-Version ausgewählt.");
                return;
            }

            ApplicationModel? app = FirmwareManagerViewModel.Instanz.Applications.FirstOrDefault(a => a.AppId == AppId);
            if(app == null)
            {
                MainViewModel.Instanz.ShowError("Update nicht möglich", $"Die Anwendung mit der ID {AppId} ist nicht in der lokalen Datenbank vorhanden.");
                return;
            }

            ReleaseModel? release = app.Releases.FirstOrDefault(r => r.Version != null && r.Version.CompareTo(UpdateVersion) == 0);
            if(release == null)
            {
                MainViewModel.Instanz.ShowError("Update nicht möglich", $"Die Anwendungsversion {UpdateVersion} ist nicht in der lokalen Datenbank vorhanden.");
                return;
            }

            if(!release.IsLocalAvailable)
            {
                MainViewModel.Instanz.ShowError("Update nicht möglich", "Die Anwendungsversion ist noch nicht lokal vorhanden und wird jetzt heruntergeladen.");

                string destination = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                destination = Path.Combine(destination, "OpenKNX", "Firmware", AppId, release.Version.ToString());
                if (!Directory.Exists(destination))
                {
                    Directory.CreateDirectory(destination);
                }
                destination = Path.Combine(destination, Name);
                ActionsViewModel.Instanz.AddAction(new DownloadAction($"Download {Name}", release.FileUrl, destination));
                return;
            }

            if(release.ContentModel == null)
            {
                MainViewModel.Instanz.ShowError("Update nicht möglich", "Die Anwendungsversion enthält kein ContentModel");
                return;
            }



            IEnumerable<Product> products = release.ContentModel.Products.Where(p => p.Architecture == Architecture);

            if (products.Count() == 0)
            {
                MainViewModel.Instanz.ShowError("Update nicht möglich", "Es wurden keine passenden Produkte gefunden.");
                return;
            }

            Product? selectedProduct = null;
            if (products.Count() == 1)
                selectedProduct = products.ElementAt(0);
            else if(products.Count() > 1)
            {
                MainViewModel.Instanz.ShowError("Update nicht möglich", "Es wurden mehrere Produkte gefunden.");
                return;
            }
            if(selectedProduct == null)
            {
                MainViewModel.Instanz.ShowError("Update nicht möglich", "Es wurde kein Produkt ausgewählt.");
                return;
            }

            PlatformDevice device;
            if (IsIP)
                device = new(ArchitectureType.ESP32, Name, $"{IP}:{Port}", "ota");
            else
                device = new(ArchitectureType.ESP32, Name, Address, "bus");

            ActionsViewModel.Instanz.AddAction(new FlashAction(device, selectedProduct));
        }
    }
}
