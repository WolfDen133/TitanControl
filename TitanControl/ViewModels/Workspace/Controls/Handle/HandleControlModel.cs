using CommunityToolkit.Mvvm.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using TitanControl.Models;
using TitanControl.Models.Control;
using TitanControl.Models.Control.Handle;
using TitanControl.Services.Session;
using TitanControl.ViewModels.Workspace.Controls.Handle.Command;
using TitanControl.WebAPI.Data;
using HandleInformation = TitanControl.WebAPI.Data.Model.Handle;

namespace TitanControl.ViewModels.Workspace.Controls.Handle
{
    public abstract class HandleControlModel
    : ObservableObject, IHandleControl, ISaveable
    {
        private bool _isSelected;
        private bool _isMoving;
        private HandleInformation? _handleInformation;

        protected HandleControlModel(
            HandleModel model,
            ISessionService sessionService)
        {
            Model = model;
            SessionService = sessionService;
        }

        protected ISessionService SessionService { get; }

        public HandleModel Model { get; }

        IControlModel IHandleControl.Model => Model;

        public ControlId ControlId => Model.ControlId;

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

                OnPropertyChanged();
                OnPropertyChanged(nameof(TitanId));
                OnPropertyChanged(nameof(Halo));
                OnPropertyChanged(nameof(Legend));
            }
        }

        public string? Halo => HandleInformation?.Halo;
        public string? Legend => HandleInformation?.Legend;
        public int? TitanId => HandleInformation?.TitanId;

        public ISaveModel ToModel() => Model;

        public abstract Task ExecuteAsync();

        public abstract IWorkspaceControl Copy();
    }

    public abstract class HandleControlModel<TModel>
        : HandleControlModel, IHandleControl<TModel>
        where TModel : HandleModel
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
