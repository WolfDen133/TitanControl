using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitanControl.Models
{
    public interface ISaveModel
    {
        ISaveable ToInstance()
        {
            return null!;
        }
    }
}
