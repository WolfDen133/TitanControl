using System;
using System.ComponentModel;
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

        Task InitializeAsync()
        {
            return null!;
        }

        Task Select(TKey id)
        {
            return null!;
        }
    }
}
