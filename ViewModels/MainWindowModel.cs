using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanControl.Session;

namespace TitanControl.ViewModels
{
    public class MainWindowModel : BaseViewModel
    {
        public SessionManager<TitanWebAPI.Titan> SessionManager;

        public MainWindowModel(SessionManager<TitanWebAPI.Titan> sessionManager)
        {
            SessionManager = sessionManager;
        }


    }
}
