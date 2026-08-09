using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Disk.Model.Session;
using TitanControl.Logging;

namespace TitanControl.ViewModels.Page
{
    public class SessionFormModel : BaseViewModel
    {
        public required Guid Id;
        public string SessionName { get; set; } = string.Empty;
        public string IpAddress { get; set; } = null!;

        public static ValidationResult? ValidateIp(string? value)
        {
            return IPAddress.TryParse(value, out _)
                ? ValidationResult.Success
                : new ValidationResult("Enter a valid IPv4 or IPv6 address.");
        }

        public int Port { get; set; }
        public int? PortInteractive { get; set; } 

        public bool AutoTimeout { get; set; }
        public bool Reconnect { get; set; }
        public int? AutoTimeoutMinuates { get; set; }
        public int KeepAliveSeconds { get; set; }
        public int? ReconnectAttempts { get; set; }
        public bool UseHttps { get; set; }

        public static SessionFormModel FromModel(SessionModel model)
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
                AutoTimeoutMinuates = model.AutoTimeout,
                ReconnectAttempts = model.ReconnectIterations,
                KeepAliveSeconds = model.KeepAlive,
                UseHttps = model.UseHttps
            };
        }

        public SessionModel ToModel()
        {
            if (ValidateIp(IpAddress) is ValidationResult result)
            {
                var ex = new InvalidDataException("IPAddress is invalid.");
                Log.Error(ex, $"SessionFormModel {Id}:{SessionName} has invalid IP Address: {IpAddress}");
                throw ex;
            }

            if (ReconnectAttempts is null)
            {
                var ex = new InvalidDataException("Reconnect attempts is null.");
                Log.Error(ex, $"SessionFormModel {Id}:{SessionName} has invalid ReconnectionAttempts of null");
                throw ex;
            }

            return new SessionModel
            {
                ID = Id,
                Name = SessionName,
                IPAddress = IPAddress.Parse(IpAddress),
                Port = Port,
                PortInteractive = PortInteractive,
                AutoTimeout = AutoTimeout ? AutoTimeoutMinuates : null,
                KeepAlive = KeepAliveSeconds,
                ReconnectIterations = Reconnect ? (int)ReconnectAttempts : 0,
                UseHttps = UseHttps,
            };
        }
    }
}
