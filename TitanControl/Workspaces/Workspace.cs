using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Controls.Handle;
using TitanControl.Disk.Interface;
using TitanControl.Disk.Model.Workspace;

namespace TitanControl.Workspaces
{
    public class Workspace : INotifyPropertyChanged, ISaveable
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

        public required WorkspaceOptions Options { get; set; }
        public List<BaseHandleControl> Controls { get; set; } = null!;
        public DateTime LastModified { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged(string type)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(type));
        }

        public ISaveModel ToModel()
        {
            return new WorkspaceModel
            {
                Id = Id,
                Name = Name,
                Options = (WorkspaceOptionsModel)Options.ToModel(),
                Controls = Controls.Select(c => (WorkspaceControlModel)c.ToModel()).ToList(),
                LastModified = LastModified
            };
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}
