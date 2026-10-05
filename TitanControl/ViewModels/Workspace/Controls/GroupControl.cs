using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Models;
using TitanControl.Models.Control;
using TitanControl.Models.Control.Handle;
using TitanControl.Models.Control.Workspace;
using TitanControl.ViewModels.Workspace.Controls.Handle;

namespace TitanControl.ViewModels.Workspace.Controls
{
    public class GroupControl : 
        ObservableObject, IWorkspaceControl, ISaveable
    {
        private bool _isSelected;
        private bool _isMoving;
        private Rectangle _location;
        private string _name = string.Empty;
        public ControlId ControlId => ControlId.Group;

        public ObservableCollection<IHandleControl> Controls 
        { 
            get; 
            set; 
        } = [];

        public Rectangle Location
        {
            get => _location;
            set => SetProperty(ref _location, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public bool IsMoving
        {
            get => _isMoving;
            set => SetProperty(ref _isMoving, value);
        }
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public IWorkspaceControl Copy()
        {
            return new GroupControl
            {
                Location = this.Location,
                IsMoving = this.IsMoving,
                IsSelected = this.IsSelected
            };
        }

        public ISaveModel ToModel()
        {
            return new GroupControlModel
            {
                Location = this.Location,
                Controls = new List<IHandleModel>() // Assuming you want to initialize an empty list for controls
            };
        }
    }
}
