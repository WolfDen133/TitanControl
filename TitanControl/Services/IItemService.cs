using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitanControl.Services
{
    public interface IItemService<T, TKey> : INotifyPropertyChanged, IDisposable
    {
        Task<T> Create(string id);
        Task Delete(TKey id);
        Task<T> Get(TKey id);

        Task SaveAsync();
        Task LoadAsync();
    }
}
