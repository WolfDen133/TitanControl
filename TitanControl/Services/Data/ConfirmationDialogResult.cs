using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Services.Interface;

namespace TitanControl.Services.Data
{
    public class ConfirmationDialogResult : IDialogResult<bool>
    {
        public bool Value { get; set; }
    }
}
