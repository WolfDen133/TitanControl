using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Controls.Handle;

namespace TitanControl.Disk.Model.Workspace
{
    public class WorkspaceModel
    {
        public required Guid Id { get; set; }
        public required string Name { get; set; }
        public int Version { get; } = 1;
        public required WorkspaceOptionsModel Options { get; set; }
        public List<BaseHandleControl> Controls { get; set; } = null!;
        public DateTime LastModified { get; set; }
    }
}
