using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitanControl.Disk.Resporitory
{
    public interface IRepository<T> : IDisposable
    {
        Task SaveAsync(T item);
    }
}
