using System.Threading.Tasks;
using TitanControl.Models.Session;

namespace TitanControl.Disk.Resporitory.Session
{
    public interface ISessionRepository : IRepository<SessionRecordModel>
    {
        Task<SessionRecordModel> LoadAsync();
    }
}
