using System;
using TitanControl.Services.Session;

namespace TitanControl.Events.Session
{
    public sealed class SessionStateChangedEventArgs : EventArgs
    {
        public SessionStateChangedEventArgs(
            SessionConnectionState previousState,
            SessionConnectionState currentState,
            Exception? exception = null)
        {
            PreviousState = previousState;
            CurrentState = currentState;
            Exception = exception;
        }

        public SessionConnectionState PreviousState { get; }

        public SessionConnectionState CurrentState { get; }

        public Exception? Exception { get; }
    }
}
