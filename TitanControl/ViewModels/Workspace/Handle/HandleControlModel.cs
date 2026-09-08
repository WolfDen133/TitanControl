using CommunityToolkit.Mvvm.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using TitanControl.Models;
using TitanControl.Models.Control;
using TitanControl.Services.Session;
using TitanControl.ViewModels.Workspace.Handle.Command;
using TitanControl.Views.Controls.Handle;
using TitanControl.WebAPI.Data;
using HandleInformation = TitanControl.WebAPI.Data.Model.Handle;

namespace TitanControl.ViewModels.Workspace.Handle
{
    public abstract class HandleControlModel
    : ObservableObject, IHandleControl, ISaveable
    {
        private bool _isSelected;
        private bool _isMoving;
        private HandleInformation? _handleInformation;

        protected HandleControlModel(
            ControlModel model,
            ISessionService sessionService)
        {
            Model = model;
            SessionService = sessionService;
        }

        protected ISessionService SessionService { get; }

        public ControlModel Model { get; }

        IControlModel IHandleControl.Model => Model;

        public HandleControlId ControlId => Model.ControlId;

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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }

        public HandleKeyProfile KeyProfile
        {
            get => Model.KeyProfile;
            set
            {
                if (Model.KeyProfile == value)
                    return;

                Model.KeyProfile = value;
                OnPropertyChanged();
            }
        }

        protected HandleInformation? HandleInformation
        {
            get => _handleInformation;
            set
            {
                if (_handleInformation == value)
                    return;

                _handleInformation = value;
                TitanId = value?.TitanId ?? -1;

                OnPropertyChanged();
                OnPropertyChanged(nameof(Halo));
            }
        }

        public string? Halo => HandleInformation?.Halo;

        public ISaveModel ToModel() => Model;

        public abstract Task ExecuteAsync();

        public abstract IHandleControl Copy();
    }

    public abstract class HandleControlModel<TModel>
        : HandleControlModel, IHandleControl<TModel>
        where TModel : ControlModel
    {
        protected HandleControlModel(
            TModel model,
            ISessionService sessionService)
            : base(model, sessionService)
        { }

        public new TModel Model => (TModel)base.Model;

        TModel IHandleControl<TModel>.Model => Model;

        protected ICommandMap<TModel> CommandMap { get; set; } = null!;

        public override Task ExecuteAsync() =>
            CommandMap.ExecuteAsync(
                KeyProfile,
                HandleType,
                Model);
    }
}
