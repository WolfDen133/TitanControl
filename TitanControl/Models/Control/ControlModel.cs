using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TitanControl.Controls.Handle;
using TitanControl.WebAPI.Data;

namespace TitanControl.Models.Control
{
    public abstract class ControlModel : INotifyPropertyChanged
    {
        private Rectangle _location;

        private int _titanId;
        private HandleType _handleType;
        private KeyProfile _keyProfile;

        public event PropertyChangedEventHandler? PropertyChanged;

        [JsonPropertyName("type")]
        public ControlId ControlId { get; init; } = ControlId.None;

        [JsonPropertyName("location")]
        public Rectangle Location 
        { 
            get => _location; 
            set
            {
                _location = value;
                OnPropertyChanged(nameof(Location));
            } 
        }

        [JsonPropertyName("titanId")]
        public int TitanId 
        {
            get => _titanId;
            set
            {
                _titanId = value;
                OnPropertyChanged(nameof(TitanId));
            }
        }

        [JsonPropertyName("handleType")]
        public HandleType HandleType 
        { 
            get => _handleType; 
            set
            {
                _handleType = value;
                OnPropertyChanged(nameof(HandleType));
            }
        }

        [JsonPropertyName("keyProfile")]
        public KeyProfile KeyProfile 
        { 
            get => _keyProfile;
            set 
            { 
                _keyProfile = value;
                OnPropertyChanged(nameof(KeyProfile));
            } 
        }

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
