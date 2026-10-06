using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitanControl.Services
{
    public interface IService : INotifyPropertyChanged, IDisposable
    {
        Task InitializeAsync()
        {
            return null!;
        }
    }
}
