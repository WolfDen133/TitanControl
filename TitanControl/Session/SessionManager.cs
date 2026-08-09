using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Disk.Model.Session;
using TitanControl.Logging;
using TitanControl.Session.Interface;
using TitanControl.WebAPI;

namespace TitanControl.Session
{
    public sealed class SessionManager<TApi>
    : ISessionManager<TApi>
    where TApi : Titan
    {
        public const string LoggingCategory = "SessionManager";

        private readonly ConcurrentDictionary<
            Guid,
            ISession<TApi>> _sessions =
            new();

        private readonly SessionOptions _options;
        private bool _disposed;

        public SessionConnectionState ConnectionState { get; private set; }

        public SessionManager(SessionOptions options)
        {
            _options = options;
        }

        public ISession<TApi> Create(string name, IPAddress? ipAddress = null, int port = 4430, int portInteractive = -1, bool useHttps = false)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            var session = new ApiSession<TApi>(
                Guid.NewGuid(),
                (TApi)new Titan(ipAddress ?? IPAddress.Parse("127.0.0.1"), port, portInteractive, useHttps),
                _options);

            session.Name = name;

            if (!_sessions.TryAdd(session.ID, session))
            {
                session.Dispose();

                throw new InvalidOperationException(
                    $"Session '{session.ID}' already exists.");
            }

            Logging.Log.Information($"Created new session '{name}' ({ipAddress}:{port}).", LoggingCategory);

            return session;
        }

        public bool TryGet(
            Guid sessionId,
            out ISession<TApi>? session)
        {
            return _sessions.TryGetValue(
                sessionId,
                out session);
        }

        public bool TryGet(
            string name,
            out ISession<TApi>? session)
        {
            session = _sessions.Values.FirstOrDefault(s => s.Name == name);
            return session != null;
        }

        public IReadOnlyCollection<ISession<TApi>> GetAll()
        {
            return _sessions.Values.ToArray();
        }

        public bool Update(Guid sessionId, SessionModel newSession)
        {
            if (!_sessions.TryGetValue(sessionId, out var oldSession))
                return false;
            
            var session = new ApiSession<TApi>(
                oldSession.ID,
                (TApi)new Titan(newSession.IPAddress, newSession.Port, newSession.PortInteractive ?? -1, newSession.UseHttps),
                new SessionOptions
                {
                    KeepAliveInterval = TimeSpan.FromSeconds(newSession.KeepAlive),
                    FailuresBeforeDisconnected = newSession.ReconnectIterations,
                });

            session.Name = newSession.Name;

            _sessions[sessionId] = session;
            return true;
        }

        public bool Remove(Guid sessionId)
        {
            if (!_sessions.TryRemove(sessionId, out var session))
            {
                return false;
            }

            session.Dispose();

            Log.Information($"Removed session '{sessionId}'", LoggingCategory);
            return true;
        }

        public void Save()
        {

        }

        public void Load()
        {

        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            foreach (var session in _sessions.Values)
            {
                session.Dispose();
            }

            _sessions.Clear();

            GC.SuppressFinalize(this);
        }
    }
}
