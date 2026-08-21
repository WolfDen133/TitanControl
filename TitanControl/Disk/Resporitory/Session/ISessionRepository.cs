using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Models.Session;

namespace TitanControl.Disk.Resporitory.Session
{
    public interface ISessionRepository : IRepository<SessionRecordModel>
    {
        Task<SessionRecordModel> LoadAsync();
    }
}
