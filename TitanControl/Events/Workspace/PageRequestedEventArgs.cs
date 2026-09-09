using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.ViewModels.Page;

namespace TitanControl.Events.Workspace
{
    public class PageRequestedEventArgs
    {
        public required PageId Page;
        public bool Opening = true;
    }
}
