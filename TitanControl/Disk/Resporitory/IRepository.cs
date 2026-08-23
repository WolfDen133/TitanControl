using System;
using System.Threading.Tasks;

namespace TitanControl.Disk.Resporitory
{
    public interface IRepository<T> : IDisposable
    {
        Task SaveAsync(T item);
    }
}
