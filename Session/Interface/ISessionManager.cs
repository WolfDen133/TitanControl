using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TitanWebAPI;

namespace TitanControl.Session.Interface
{
    public interface ISessionManager<TApi> : IDisposable
    where TApi : Titan
    {
        ISession<TApi> Create(string name, IPAddress? ipAddress = null, int port = 4430, bool useHttps = false);

        bool TryGet(
            UUID sessionId,
            out ISession<TApi>? session);

        bool TryGet(
            string name,
            out ISession<TApi>? session);

        bool Remove(UUID sessionId);

        IReadOnlyCollection<ISession<TApi>> GetAll();
    }
}
