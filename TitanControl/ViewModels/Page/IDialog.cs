using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Events;

namespace TitanControl.ViewModels.Page
{
    public interface IDialog
    {
        void OnAccept();
        void OnCancel();

        event EventHandler<DialogClosedEventArgs>? DialogClosed;
    }
}
