using System.Threading.Tasks;
using TitanControl.Models.Session;

namespace TitanControl.Disk.Resporitory.Session
{
    public class SessionRepository : ISessionRepository
    {
        public FileHandler _handler;

        public SessionRepository(FileHandler handler)
        {
            _handler = handler;
        }

        public async Task<SessionRecordModel> LoadAsync()
        {
            return await _handler.LoadSessions();
        }

        public async Task SaveAsync(SessionRecordModel item)
        {
            await _handler.SaveSessions(item);
        }

        public void Dispose()
        { }
    }
}
