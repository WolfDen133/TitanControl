using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Disk;
using TitanControl.Disk.Interface;
using TitanControl.Disk.Model.Workspace;

namespace TitanControl.Workspaces
{
    public class WorkspaceOptions : INotifyPropertyChanged, ISaveable
    {
        private Guid? session;
        private Size gridSize = new Size(18, 18);

        public Guid? Session
        {
            get => session;
            set
            {
                session = value;
                OnPropertyChanged(nameof(Session));
            }
        }

        public Size GridSize
        {
            get => gridSize;
            set
            {
                gridSize = value;
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

        public ISaveModel ToModel()
        {
            return new WorkspaceOptionsModel
            {
                Session = session,
                GridSize = FileUtilities.ToArrayFromSize(GridSize)
            };
        }
    }
}
