using OpenKNX.Toolbox.Lib.Models;
using OpenKNX.Toolbox.Models;
using OpenKNX.Toolbox.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui.Controls;

namespace OpenKNX.Toolbox.ViewModels
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        private static MainViewModel? _instanz;
        public static MainViewModel Instanz
        {
            get
            {
                if (_instanz == null)
                    _instanz = new MainViewModel();
                return _instanz;
            }
        }



        public ActionsViewModel ActionsViewModel
        {
            get { return ActionsViewModel.Instanz; }
        }

        public DeviceManagerViewModel DeviceManagerViewModel
        {
            get { return DeviceManagerViewModel.Instanz; }
        }

        public string Title
        {
            get {
                string title = "OpenKNX Toolbox";
                var assembly = System.Reflection.Assembly.GetEntryAssembly();
                if (assembly == null) return title;
                var vers = assembly.GetName().Version;
                if (vers == null) return title;
                int showVersions = 3;
#if DEBUG
                showVersions = 4;
#endif
                title += " - v" + string.Join('.', vers.ToString().Split('.').Take(showVersions));
                return title;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public async Task Init()
        {
            ActionsViewModel.Init();
            await FirmwareManagerViewModel.Instanz.Init();
            DeviceManagerViewModel.Instanz.Init();
        }

        private DeviceModel? _selectedDevice;
        public DeviceModel? SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                _selectedDevice = value;
                Changed("SelectedDevice");
            }
        }

        private void Changed(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
