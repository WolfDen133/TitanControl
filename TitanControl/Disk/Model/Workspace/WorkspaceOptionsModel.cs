using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Disk.Model.Session;
using TitanControl.Session.Interface;
using TitanControl.WebAPI;

namespace TitanControl.Disk.Model.Workspace
{
    public class WorkspaceOptionsModel : INotifyPropertyChanged
    {
        private Guid? session;
        public Guid? Session
        {
            get => session;
            set
            {
                session = value;
                OnPropertyChanged(nameof(Session));
            }
        }

        public Size GridSize = new Size(18, 18);

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
