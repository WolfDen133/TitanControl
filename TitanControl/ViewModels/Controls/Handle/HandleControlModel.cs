using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using TitanControl.Logging;
using TitanControl.Models;
using TitanControl.Models.Control;
using TitanControl.Models.Control.Handle;
using TitanControl.Services.Command;
using TitanControl.Services.Command.Map;
using TitanControl.Services.Session;
using TitanControl.Views.Controls.Toolbar.Button;
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
        private Bitmap? _image;

        protected HandleControlModel(
            HandleModel model)
        {
            Model = model;
        }


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

        public Bitmap? Image
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }

        public HandleInformation? HandleInformation
        {
            get => _handleInformation;
            set
            {
                if (_handleInformation == value)
                    return;

                _handleInformation = value;

                Task.Run(TryLoadImage);

                OnPropertyChanged();
                OnPropertyChanged(nameof(TitanId));
                OnPropertyChanged(nameof(Halo));
                OnPropertyChanged(nameof(Legend));
            }
        }

        public string? Halo => HandleInformation?.Halo ?? "#555555";
        public string? Legend => HandleInformation?.Legend ?? "New Handle";
        public int? TitanId => HandleInformation?.TitanId;
        public string? UserNumber => HandleInformation?.UserNumber;

        public ICommandMap? CommandMap { get; set; }

        public ISaveModel ToModel() => Model;

        public abstract Task ExecuteAsync(CommandAction action);

        public abstract IWorkspaceControl Copy();

        private async Task TryLoadImage()
        {
            if (HandleInformation is null)
            {
                Image = null;
                return;
            }

            var imageUrl = HandleInformation.Icon;
            if (imageUrl is null)
            {
                Image = null;
                return;
            }

            var http = new HttpClient();
            byte[] imageBytes = await http.GetByteArrayAsync(imageUrl);
            using MemoryStream ms = new MemoryStream(imageBytes);
            Image = new Bitmap(ms);
        }
    }

    public abstract class HandleControlModel<TModel>
        : HandleControlModel, IHandleControl<TModel>
        where TModel : HandleModel
    {
        protected HandleControlModel(
            TModel model)
            : base(model)
        { }

        public new TModel Model => (TModel)base.Model;

        TModel IHandleControl<TModel>.Model => Model;

        public override async Task ExecuteAsync(CommandAction action)
        {
            if (HandleInformation is null || 
                CommandMap is null)
                return;

            await CommandMap.ExecuteAsync(
                KeyProfile,
                HandleInformation,  
                action);
        }
    }
}
