using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitanControl.Disk.Model.Session
{
    public class SessionRecordModel
    {
        public Guid LastSession { get; set; }
        public Dictionary<Guid, SessionModel> Sessions { get; set; } = null!;
    }
}
