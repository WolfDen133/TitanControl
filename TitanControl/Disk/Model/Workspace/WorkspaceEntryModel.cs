using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Disk.Interface;

namespace TitanControl.Disk.Model.Workspace
{
    public class WorkspaceEntryModel : ISaveModel
    {
        public string Path { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
