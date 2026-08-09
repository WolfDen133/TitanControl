using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Services.Data;

namespace TitanControl.Services.Interface
{
    public interface IDialogService
    {
       
        Task<bool> ShowConfirmationAsync(
            string title, string message);
        
    }
}
