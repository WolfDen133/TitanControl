using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitanControl.Session
{
    public enum SessionConnectionState
    {
        Available, // Is descovered
        Enabled, // Is descovered and enabled
        Disabled, // Is descovered and disabled
        Connected, // Is discovered, enabled and connected
        Connecting, // Is descovered, enabled and connecting
        Disconnected, // Is descovered enabled and disconnected
        Unreachable // Is not discovered
    }
}
