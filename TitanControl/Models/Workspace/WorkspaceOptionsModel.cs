using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TitanControl.Disk.Converter;
using TitanControl.Session.Interface;
using TitanControl.WebAPI;

namespace TitanControl.Models.Workspace
{
    public class WorkspaceOptionsModel : INotifyPropertyChanged, ISaveModel
    {
        private Guid _session;
        private Size _gridSize = new Size(18, 18);

        [JsonPropertyName("session")]
        public Guid Session
        {
            get => _session;
            set
            {
                _session = value;
                OnPropertyChanged(nameof(Session));
            }
        }

        [JsonPropertyName("gridSize")]
        public Size GridSize
        {
            get => _gridSize;
            set
            {
                _gridSize = value;
                OnPropertyChanged(nameof(GridSize));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged(string type)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(type));
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}
