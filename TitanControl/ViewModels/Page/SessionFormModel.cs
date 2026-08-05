using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TitanControl.ViewModels.Page
{
    public class SessionFormModel : BaseViewModel
    {
        public string SessionName { get; set; } = string.Empty;
        public string IpAddress { get; set; } = null!;

        public static ValidationResult? ValidateIp(string? value, ValidationContext context)
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
    }
}
