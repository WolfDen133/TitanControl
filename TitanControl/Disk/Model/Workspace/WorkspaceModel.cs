using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Controls.Handle;

namespace TitanControl.Disk.Model.Workspace
{
    public class WorkspaceModel : INotifyPropertyChanged
    {
        private string name = string.Empty;

        public required Guid Id 
        { get; set; }

        public required string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public int Version { get; } = 1;
        
        public required WorkspaceOptionsModel Options { get; set; }
        public List<BaseHandleControl> Controls { get; set; } = null!;
        public DateTime LastModified { get; set; }

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
