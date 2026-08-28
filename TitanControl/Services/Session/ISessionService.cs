using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace TitanControl.Services.Session
{
    public interface ISessionService : IItemService<TitanSession, Guid>
    {
        ObservableCollection<TitanSession> Sessions { get; }

        ISession? CurrentSession { get; }

        ReadOnlyObservableCollection<ISession> ScanResults { get; }
        Dictionary<string, IPAddress> Nics { get; }

        bool IsScannerRunning { get; }

        IPAddress? ScannerInterfaceAddress { get; }

        event EventHandler<bool>? ScannerRunningChanged;

        Task ConfigureScannerAsync(
            IPAddress localInterfaceAddress,
            TimeSpan? scanDuration = null,
            TimeSpan? connectionTimeout = null,
            TimeSpan? delayBetweenScans = null,
            int maximumConcurrency = 64,
            bool useHttps = false);

        Task StartScannerAsync(
            CancellationToken cancellationToken = default);

        void StopScanner();

        Task ClearScanResultsAsync();

        void IDisposable.Dispose()
        {
            return;
        }
    }
}
