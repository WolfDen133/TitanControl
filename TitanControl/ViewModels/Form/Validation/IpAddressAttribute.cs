using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Sockets;

namespace TitanControl.ViewModels.Form.Validation
{
    public class IpAddressAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is null)
                return true;

            var text = value.ToString();

            if (string.IsNullOrWhiteSpace(text))
                return true;

            return IPAddress.TryParse(text, out var address) &&
            address.AddressFamily == AddressFamily.InterNetwork;
        }
    }
}
