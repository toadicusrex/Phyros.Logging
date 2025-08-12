using System.Collections;
using System.Text.Json;
using Phyros.Logging.LogWriter;
using Phyros.Logging.Models;
using Serilog;
using Serilog.Context;
using Serilog.Events;

namespace Phyros.Logging.Serilog
{
    /// <summary>
    /// Serilog implementation of ILogProvider
    /// </summary>
    public class SerilogLogProvider : ILogProvider
    {
        private readonly ILogger _logger;
        private readonly bool _disposeLogger;
        private readonly JsonSerializerOptions _jsonOptions;
        private bool _disposed = false;

        /// <summary>
        /// Creates a new instance of SerilogLogProvider
        /// </summary>
        /// <param name="logger">The Serilog logger instance</param>
        /// <param name="disposeLogger">Whether to dispose the logger when this instance is disposed</param>
        public SerilogLogProvider(ILogger logger, bool disposeLogger = true)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _disposeLogger = disposeLogger;
            _jsonOptions = new JsonSerializerOptions 
            { 
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        /// <inheritdoc />
        public void Write(LogEntry entry)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(SerilogLogProvider));
            }

            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            // Create a new scope with the correlation ID
            using (LogContext.PushProperty("correlationId", entry.CorrelationId, destructureObjects: true))
            {
                // Add all properties to the scope
                foreach (var item in entry.Properties)
                {
                    // Handle collection properties differently
                    if (item.Value is IEnumerable value && !(item.Value is string))
                    {
                        LogContext.PushProperty(item.Key, JsonSerializer.Serialize(item.Value, _jsonOptions));
                    }
                    else
                    {
                        LogContext.PushProperty(item.Key, item.Value, destructureObjects: true);
                    }
                }

                // Write the log entry
                _logger.Write(
                    ConvertToSerilogLevel(entry.Severity), 
                    entry.Exception, 
                    entry.MessageTemplate,
                    entry.OrderedMessageProperties);
            }
        }

        // Convert our severity levels to Serilog levels
        private static LogEventLevel ConvertToSerilogLevel(LogSeverityKind severity)
        {
            return severity switch
            {
                LogSeverityKind.Debug => LogEventLevel.Debug,
                LogSeverityKind.Information => LogEventLevel.Information,
                LogSeverityKind.Warning => LogEventLevel.Warning,
                LogSeverityKind.Error => LogEventLevel.Error,
                LogSeverityKind.Fatal => LogEventLevel.Fatal,
                _ => LogEventLevel.Information
            };
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
                if (disposing && _disposeLogger && _logger is IDisposable disposableLogger)
                {
                    disposableLogger.Dispose();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Finalizer to ensure resources are released
        /// </summary>
        ~SerilogLogProvider()
        {
            Dispose(false);
        }
    }
}