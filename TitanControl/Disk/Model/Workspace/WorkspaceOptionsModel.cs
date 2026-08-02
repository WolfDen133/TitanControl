using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Disk.Model.Session;
using TitanControl.Session.Interface;
using TitanControl.WebAPI;

namespace TitanControl.Disk.Model.Workspace
{
    public class WorkspaceOptionsModel
    {
        public Guid? Session { get; set; } = null;
        public Size GridSize = new Size(18, 18);
    }
}
