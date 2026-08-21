using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TitanControl.Controls.Handle;
using TitanControl.Models.Control;

namespace TitanControl.Models.Workspace
{
    public class WorkspaceModel : INotifyPropertyChanged, ISaveModel
    {
        private string _name = string.Empty;
        private WorkspaceOptionsModel _options = null!;
        private List<ControlModel> _controls = null!;
        private DateTime _lastModified;

        [JsonPropertyName("version")]
        public int WorkspaceVersion { get; } = 1;


        [JsonPropertyName("workspaceId")]
        public required Guid Id
        { 
            get; 
            init; 
        }

        [JsonPropertyName("name")]
        public required string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        [JsonPropertyName("options")]
        public WorkspaceOptionsModel Options
        {
            get => _options;
            set
            {
                _options = value;
                OnPropertyChanged(nameof(Options));
            } 
        }

        [JsonPropertyName("controls")]
        public List<ControlModel> Controls 
        { 
            get => _controls;
            set
            {
                _controls = value;
                OnPropertyChanged(nameof(Controls));
            }
        }

        public DateTime LastModified 
        { 
            get => _lastModified; 
            set
            {
                _lastModified = value;
                OnPropertyChanged(nameof(LastModified));
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
