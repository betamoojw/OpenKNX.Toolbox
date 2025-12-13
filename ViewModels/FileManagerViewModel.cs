using CommunityToolkit.Mvvm.Input;
using Kaenx.Konnect;
using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Classes;
using Kaenx.Konnect.Connections;
using Kaenx.Konnect.Telegram.Contents;
using Kaenx.Konnect.Telegram.IP;
using Kaenx.Konnect.Telegram.IP.DIB;
using KnxFileTransferClient.Lib;
using Octokit;
using OpenKNX.Toolbox.Classes;
using OpenKNX.Toolbox.Classes.Actions;
using OpenKNX.Toolbox.Dialogs;
using OpenKNX.Toolbox.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Management.Automation.Language;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenKNX.Toolbox.ViewModels
{
    public partial class FileManagerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string RemoteAddress { get; set; } = string.Empty;
        public UnicastAddress RemoteAddressUni = UnicastAddress.FromString("0.0.0");

        private bool _isDragging = false;
        public bool IsDragging
        {
            get { return _isDragging; }
            set
            {
                _isDragging = value;
                Changed(nameof(IsDragging));
            }
        }

        public ObservableCollection<FileModel> Items { get; set; } = new ObservableCollection<FileModel>();

        private DeviceConnectionModel? _connection;

        public DeviceConnectionModel? GetConnectionModel()
        {
            return _connection;
        }


        [RelayCommand]
        public void Delete(FileModel file)
        {
            if (_connection == null)
            {
                MainViewModel.Instanz.ShowError("FileManager Error", "Es wurde keine Verbindung zu einem Gateway hergestellt.");
                return;
            }
            ActionsViewModel.Instanz.AddAction(new FileAction(RemoteAddressUni, _connection, file.FullPath, string.Empty, file.IsFile, FileActionTypes.Delete));
        }

        [RelayCommand]
        public void Download(FileModel file)
        {
            if(!file.IsFile)
            {
                MainViewModel.Instanz.ShowError("FileManager Error", "Das ausgewählte Element ist kein Datei.");
                return;
            }
            if (_connection == null)
            {
                MainViewModel.Instanz.ShowError("FileManager Error", "Es wurde keine Verbindung zu einem Gateway hergestellt.");
                return;
            }
            ActionsViewModel.Instanz.AddAction(new FileAction(RemoteAddressUni, _connection, file.FullPath, @"C:\Users\Mike\Desktop\sources\test.txt", true, FileActionTypes.Download));
        }

        [RelayCommand]
        public async Task ReloadFiles()
        {
            try
            {
                RemoteAddressUni = UnicastAddress.FromString(RemoteAddress);
            } catch
            {
                MainViewModel.Instanz.ShowError("FileManager Error", "Die angegeben Adresse ist ungültig.");
            }

            int counter = 0;
            object lockObject = new object();
            HashSet<string> uniquePhysicalAddresses = new HashSet<string>();
            List<DeviceConnectionModel> gateways = new List<DeviceConnectionModel>();
            IpKnxConnection _conn = KnxFactory.CreateTunnelingUdp(new(IPAddress.Parse("224.0.23.12"), 3671));

            _conn.OnReceivedService += (IpTelegram message) =>
            {
                lock (lockObject)
                {
                    if (message.ServiceIdentifier != Kaenx.Konnect.Enums.ServiceIdentifiers.SearchResponse)
                        return;
                    SearchResponse? response = message as SearchResponse;
                    if (response == null)
                        return;

                    HpaiContent? hpai = response.GetEndpoint();
                    DeviceInfo? deviceInfo = response.GetDeviceInfo();
                    SupportedServiceFamilies? svcFamilies = response.GetSupportedServiceFamilies();
                    if (hpai == null || deviceInfo == null || svcFamilies == null)
                        return;

                    if (deviceInfo.Medium != Kaenx.Konnect.Enums.KnxMediums.TP1)
                        return;

                    if (uniquePhysicalAddresses.Add(deviceInfo.UnicastAddress.ToString()))
                    {
                        if (svcFamilies.GetServiceFamilyVersion(Kaenx.Konnect.Enums.ServiceFamilies.Tunneling) > 0)
                        {
                            int tunnelingVersion = svcFamilies.GetServiceFamilyVersion(Kaenx.Konnect.Enums.ServiceFamilies.Tunneling);
                            Console.WriteLine($"{counter,2} Tunneling v{tunnelingVersion} -> {hpai.Endpoint,-20} ({deviceInfo.UnicastAddress,-9}) [{deviceInfo.FriendlyName}]");
                            DeviceConnectionModel conn = new(hpai.Endpoint, deviceInfo.UnicastAddress, tunnelingVersion, deviceInfo.FriendlyName);
                            gateways.Add(conn);
                            counter++;
                        }

                        if (svcFamilies.GetServiceFamilyVersion(Kaenx.Konnect.Enums.ServiceFamilies.Routing) > 0)
                        {
                            int routingVersion = svcFamilies.GetServiceFamilyVersion(Kaenx.Konnect.Enums.ServiceFamilies.Routing);
                            Console.WriteLine($"{counter,2} Routing   v{routingVersion} -> {hpai.Endpoint,-20} ({deviceInfo.UnicastAddress,-9}) [{deviceInfo.FriendlyName}]");
                            DeviceConnectionModel conn = new(hpai.Endpoint, deviceInfo.UnicastAddress, routingVersion, deviceInfo.FriendlyName, true);
                            gateways.Add(conn);
                            counter++;
                        }
                    }
                }
            };

            SearchRequest req = new SearchRequest(_conn.GetLocalEndpoint());
            await _conn.SendAsync(req);

            await Task.Delay(500);
            _conn.Dispose();

            ObjectSelectDialog selectDialog = new ObjectSelectDialog("Gateway Auswahl", gateways.Cast<object>().ToList());

            CancellationTokenSource token = new CancellationTokenSource();
            await MainViewModel.Instanz.ContentDialogService.ShowAsync(
                selectDialog,
                token.Token
            );

            DeviceConnectionModel? connection = selectDialog.GetSelectedItem() as DeviceConnectionModel;
            if (connection == null)
            {
                return;
            }

            _connection = connection;

            Items.Clear();

            _conn = KnxFactory.CreateTunnelingUdp(connection.RemoteEndpoint);
            await _conn.Connect();

            BusDevice busDevice = new BusDevice(RemoteAddressUni.ToString(), _conn);
            await busDevice.ConnectIndividual();

            FileTransferClient client = new FileTransferClient(busDevice);

            await GetItemsForFolder(client, "/", Items);

            await busDevice.Disconnect();
            await _conn.Disconnect();
        }

        public async Task GetItemsForFolder(FileTransferClient client, string path, ObservableCollection<FileModel> subItems)
        {
            List<FileTransferPath> items = await client.List(path, false);

            foreach(var item in items)
            {
                FileModel model = new FileModel()
                {
                    Name = item.Name,
                    IsFile = item.IsFile,
                    FullPath = path.EndsWith("/") ? path + item.Name : path + "/" + item.Name
                };
                subItems.Add(model);

                if (!model.IsFile)
                    await GetItemsForFolder(client, model.FullPath, model.Items);

                // Fix für CS0411: Typargumente explizit angeben
                subItems.Sort();
            }
        }

        private void Changed(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
