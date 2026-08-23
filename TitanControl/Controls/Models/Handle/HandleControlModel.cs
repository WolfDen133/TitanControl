using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Controls.Handle;
using TitanControl.Controls.Models.Handle.Command;
using TitanControl.Models;
using TitanControl.Models.Control;
using TitanControl.Services.Session;
using TitanControl.WebAPI.Data;
using HandleInformation = TitanControl.WebAPI.Data.Model.Handle;

namespace TitanControl.Controls.Models.Handle
{
    public abstract class HandleControlModel<TModel> 
        : ObservableObject, IHandleControl<TModel>, ISaveable
        where TModel : ControlModel
    {
        private bool _isSelected;
        private bool _isMoving;
        private HandleInformation? _handleInformation;

        protected ISessionService SessionService;

        protected HandleControlModel(TModel model, ISessionService service)
        {
            Model = model;
            SessionService = service;
        }

        public TModel Model { get; }

        protected ICommandMap<TModel> CommandMap { get; set; } = null!;

        public ControlId ControlId => Model.ControlId;

        protected HandleInformation? HandleInformation
        {
            get => _handleInformation;
            set
            {
                if (_handleInformation == value)
                    return;

                TitanId = value?.TitanId ?? -1;

                SetProperty(ref _handleInformation, value);
            }

        }
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool IsMoving
        {
            get => _isMoving;
            set => SetProperty(ref _isMoving, value);
        }

        public Rectangle Location
        {
            get => Model.Location;
            set
            {
                if (Model.Location == value)
                    return;

                Model.Location = value;
                OnPropertyChanged(nameof(Location));
            }
        }

        public int TitanId
        {
            get => Model.TitanId;
            private set
            {
                if (Model.TitanId == value)
                    return;

                Model.TitanId = value;
                OnPropertyChanged(nameof(TitanId));
            }
        }

        public HandleType HandleType
        {
            get => Model.HandleType;
            set
            {
                if (Model.HandleType == value)
                    return;

                Model.HandleType = value;
                OnPropertyChanged(nameof(HandleType));
            }
        }

        public KeyProfile KeyProfile
        {
            get => Model.KeyProfile;
            set
            {
                if (Model.KeyProfile == value)
                    return;

                Model.KeyProfile = value;
                OnPropertyChanged(nameof(KeyProfile));
            }
        }

        public Task ExecuteAsync()
        {
            return CommandMap.ExecuteAsync(
                KeyProfile,
                HandleType,
                Model);
        }

        public ISaveModel ToModel()
        {
            return Model;
        }
    }
}
