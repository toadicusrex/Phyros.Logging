using Phyros.Logging.Models;

namespace Phyros.Logging.LogWriter
{
    /// <summary>
    /// Abstracts the underlying logging implementation
    /// </summary>
    public interface ILogProvider : IDisposable
    {
        /// <summary>
        /// Writes a log entry to the underlying logging system
        /// </summary>
        /// <param name="entry">The log entry to write</param>
        void Write(LogEntry entry);
    }
}