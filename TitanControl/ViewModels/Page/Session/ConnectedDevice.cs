using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitanControl.ViewModels.Page.Session
{
    public partial class ConnectedDevice : ObservableObject
    {
        [ObservableProperty]
        private string? computerName;

        [ObservableProperty]
        private string? legend;

        [ObservableProperty]
        private string? softwareVersion;

        [ObservableProperty]
        private string? id;

        [ObservableProperty]
        private string? connectedTo;

        [ObservableProperty]
        private string? notes;

        public void Clear()
        {
            ComputerName = null;
            Legend = null;
            SoftwareVersion = null;
            Id = null;
            ConnectedTo = null;
            Notes = null;
        }
    }
}
