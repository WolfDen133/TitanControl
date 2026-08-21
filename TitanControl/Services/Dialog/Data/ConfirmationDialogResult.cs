using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitanControl.Services.Dialog.Data
{
    public class ConfirmationDialogResult : IDialogResult<bool>
    {
        public bool Value { get; set; }
    }
}
