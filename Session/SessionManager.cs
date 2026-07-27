using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Session.Interface;
using TitanWebAPI;

namespace TitanControl.Session
{
    public sealed class SessionManager<TApi>
    : ISessionManager<TApi>
    where TApi : Titan
    {
        private readonly ConcurrentDictionary<
            UUID,
            ISession<TApi>> _sessions =
            new();

        private readonly SessionOptions _options;
        private bool _disposed;

        public SessionManager(SessionOptions options)
        {
            _options = options;
        }

        public ISession<TApi> Create(string name, IPAddress? ipAddress = null, int port = 4430, bool useHttps = false)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            var session = new ApiSession<TApi>(
                UUID.New(),
                (TApi)new Titan(ipAddress ?? IPAddress.Parse("127.0.0.1"), port, useHttps),
                _options);

            session.Name = name;

            if (!_sessions.TryAdd(session.ID, session))
            {
                session.Dispose();

                throw new InvalidOperationException(
                    $"Session '{session.ID}' already exists.");
            }

            session.Start();

            return session;
        }

        public bool TryGet(
            UUID sessionId,
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

        public bool Remove(UUID sessionId)
        {
            if (!_sessions.TryRemove(sessionId, out var session))
            {
                return false;
            }

            session.Dispose();
            return true;
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
