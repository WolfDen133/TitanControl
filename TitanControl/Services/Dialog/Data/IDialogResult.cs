using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitanControl.Services.Dialog.Data
{
    public interface IDialogResult<T>
    {
        public T Value { get; protected set; }
    }
}
