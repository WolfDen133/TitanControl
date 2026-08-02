using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitanControl.Session
{
    public enum SessionConnectionState
    {
        Available,
        Inactive,
        Connected,
        Connecting,
        Disconnected,
        Error,
        Unreachable
    }
}
