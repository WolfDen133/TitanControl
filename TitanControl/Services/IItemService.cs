using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace TitanControl.Services
{
    public interface IItemService<T, TKey> : IService
    {
        Task<T> Create(string id);
        Task Delete(TKey id);
        Task<T> Get(TKey id);

        Task SaveAsync();
        Task LoadAsync();

        Task Select(TKey id)
        {
            return null!;
        }
    }
}
