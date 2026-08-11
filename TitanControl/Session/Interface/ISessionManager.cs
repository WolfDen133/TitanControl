using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TitanControl.WebAPI;

namespace TitanControl.Session.Interface
{
    public interface ISessionManager : IDisposable
    {
        ObservableCollection<TitanSession> Sessions { get; }

        ReadOnlyObservableCollection<ISession> ScanResults { get; }

        bool IsScannerRunning { get; }

        IPAddress? ScannerInterfaceAddress { get; }

        event EventHandler<bool>? ScannerRunningChanged;

        ISession Create(
            string name,
            IPAddress? ipAddress = null,
            int port = 4430,
            int portInteractive = -1,
            bool useHttps = false);

        bool Remove(Guid sessionId);

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
        Task Save();
        Task Load();
    }
}
