namespace TitanControl.Services.Session
{
    public enum SessionConnectionState
    {
        Available, // Is descovered
        Enabled, // Is descovered and enabled
        Disabled, // Is descovered and disabled
        Connected, // Is discovered, enabled and connected
        Connecting, // Is descovered, enabled and connecting
        Unreachable, // Is descovered enabled and disconnected
    }
}
