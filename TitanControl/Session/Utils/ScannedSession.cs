using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Session.Interface;
using TitanControl.WebAPI;

namespace TitanControl.Session.Utils
{
    public class ScannedSession
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "Titan Session";
        public string ComputerName { get; set; } = "DESKTOP-NAME";
        public IPAddress Address { get; set; } = new IPAddress(new byte[] { 127, 0, 0, 1 });
        public int Port { get; set; } = 4430;
        public int? PortInteractive { get; set; } = null;
        public SessionConnectionState State { get; set; } = SessionConnectionState.Disconnected;
        public bool IsSelected { get; set; } = false;
    }
}
