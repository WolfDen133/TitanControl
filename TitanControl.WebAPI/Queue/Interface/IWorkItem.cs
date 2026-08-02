using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitanControl.WebAPI.Queue.Interface
{
    public interface IWorkItem
    {
        string OperationName { get; }

        Task ExecuteAsync(CancellationToken queueCancellationToken);

        void Cancel(CancellationToken cancellationToken);

        void Fail(Exception exception);
    }
}
