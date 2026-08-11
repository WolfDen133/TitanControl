using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitanControl.Disk.Interface
{
    public interface ISaveable
    {
        ISaveModel ToModel();
    }
}
