using Phyros.Logging.Models;

namespace Phyros.Logging.LogWriter
{
    /// <summary>
    /// Implementation of ILogWriter that uses an ILogProvider
    /// </summary>
    public class PhyrosLogWriter : ILogWriter, IDisposable
    {
        private readonly ILogProvider _provider;
        private bool _disposed = false;

        /// <summary>
        /// Creates a new instance of PhyrosLogWriter
        /// </summary>
        /// <param name="provider">The log provider implementation</param>
        public PhyrosLogWriter(ILogProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <inheritdoc />
        public void Log(LogEntry entry)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(PhyrosLogWriter), "Cannot log to a disposed logger");
            }

            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            _provider.Write(entry);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases resources held by this instance
        /// </summary>
        /// <param name="disposing">Whether this is being called from Dispose or a finalizer</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _provider?.Dispose();
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Finalizer to ensure resources are released
        /// </summary>
        ~PhyrosLogWriter()
        {
            Dispose(false);
        }
    }
}