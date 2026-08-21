using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Logging;
using TitanControl.Services.Session;
using TitanControl.Validation;

namespace TitanControl.ViewModels.Page
{
    public partial class SessionFormModel : ObservableValidator
    {
        public required Guid Id;

        public SessionFormModel()
        {
            ErrorsChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(IsValid));
            };
        }

        public bool IsValid => !HasErrors;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Session name is required.")]
        public string sessionName = string.Empty;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "IP Address is required.")]
        [IpAddress(ErrorMessage = "Enter a valid IP address.")]
        public string ipAddress = null!;

        [ObservableProperty]
        [NotifyDataErrorInfo]
        [Required(ErrorMessage = "Port is required.")]
        public int port;

        public int? PortInteractive { get; set; }

        [ObservableProperty]
        public bool autoTimeout;
        [ObservableProperty]
        public bool reconnect; 
        public int? AutoTimeoutMinuates { get; set; }
        public int KeepAliveSeconds { get; set; }
        public int? ReconnectAttempts { get; set; }
        public bool UseHttps { get; set; }

        public static SessionFormModel FromModel(ISession model)
        {
            return new SessionFormModel
            {
                Id = model.ID,
                SessionName = model.Name,
                IpAddress = model.IPAddress.ToString(),
                Port = model.Port,
                PortInteractive = model.PortInteractive,
                AutoTimeout = model.AutoTimeout != null,
                Reconnect = model.ReconnectIterations != 0,
                AutoTimeoutMinuates = model.AutoTimeout != null ? model.AutoTimeout : null,
                ReconnectAttempts = model.ReconnectIterations != 0 ? model.ReconnectIterations : null,
                KeepAliveSeconds = model.KeepAlive,
                UseHttps = model.UseHttps
            };
        }

        public static SessionFormModel Empty => new SessionFormModel() { Id = Guid.Empty };

    }
}
