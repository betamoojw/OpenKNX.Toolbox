using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKNX.Toolbox.ViewModels
{
    public class ActionsViewModel : INotifyPropertyChanged
    {
        public static ActionsViewModel Instanz { get; } = new ActionsViewModel();

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<string> Actions { get; set; } = new();

        public void Init()
        {
            Actions.Add("DOwnload");
        }

        private void Changed(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
