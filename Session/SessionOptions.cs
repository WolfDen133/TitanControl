using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitanControl.Session
{
    public sealed class SessionOptions
    {
        public TimeSpan KeepAliveInterval { get; init; } =
            TimeSpan.FromSeconds(5);

        public int FailuresBeforeDisconnected { get; init; } = 3;
    }
}
