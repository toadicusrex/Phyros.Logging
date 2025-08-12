using System.Text;
using Phyros.Logging.Models;

namespace Phyros.Logging.LogWriter
{
    /// <summary>
    /// A simple console-based log provider
    /// </summary>
    public class ConsoleLogProvider : ILogProvider
    {
        private readonly bool _includeTimestamps;
        private readonly LogSeverityKind _minimumLevel;
        private readonly bool _outputToDebugConsole;
        private bool _disposed = false;

        /// <summary>
        /// Creates a new instance of ConsoleLogProvider
        /// </summary>
        /// <param name="minimumLevel">Minimum log level to output</param>
        /// <param name="includeTimestamps">Whether to include timestamps in log output</param>
        /// <param name="outputToDebugConsole">Whether to also output to Debug.WriteLine</param>
        public ConsoleLogProvider(
            LogSeverityKind minimumLevel = LogSeverityKind.Information,
            bool includeTimestamps = true,
            bool outputToDebugConsole = false)
        {
            _minimumLevel = minimumLevel;
            _includeTimestamps = includeTimestamps;
            _outputToDebugConsole = outputToDebugConsole;
        }

        /// <inheritdoc />
        public void Write(LogEntry entry)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ConsoleLogProvider));

            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            // Skip if below minimum level
            if (entry.Severity < _minimumLevel)
                return;

            var output = FormatLogEntry(entry);
            
            // Set console color based on severity
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = GetColorForSeverity(entry.Severity);
            
            // Write to console
            Console.WriteLine(output);
            
            // Reset color
            Console.ForegroundColor = originalColor;
            
            // Optionally write to debug console
            if (_outputToDebugConsole)
            {
                System.Diagnostics.Debug.WriteLine(output);
            }
        }

        private string FormatLogEntry(LogEntry entry)
        {
            var sb = new StringBuilder();
            
            // Add timestamp if enabled
            if (_includeTimestamps)
            {
                sb.Append($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] ");
            }
            
            // Add severity
            sb.Append($"[{entry.Severity}] ");
            
            // Add correlation ID
            sb.Append($"[{entry.CorrelationId}] ");
            
            // Add message template with parameters
            string formattedMessage;
            try
            {
                formattedMessage = string.Format(entry.MessageTemplate, entry.OrderedMessageProperties);
            }
            catch
            {
                // If the format fails, just use the template
                formattedMessage = entry.MessageTemplate;
            }
            sb.Append(formattedMessage);
            
            // Add exception if present
            if (entry.Exception != null)
            {
                sb.Append($" | Exception: {entry.Exception.GetType().Name}: {entry.Exception.Message}");
            }
            
            // Add properties
            if (entry.Properties.Any())
            {
                sb.Append(" | Properties: {");
                sb.Append(string.Join(", ", entry.Properties.Select(p => $"{p.Key}={FormatPropertyValue(p.Value)}")));
                sb.Append("}");
            }
            
            return sb.ToString();
        }

        private static string FormatPropertyValue(object? value)
        {
            if (value == null)
                return "null";
            
            // Handle simple types directly
            if (value is string or ValueType)
                return value.ToString() ?? "null";
            
            // For complex objects, just output the type name
            return $"[{value.GetType().Name}]";
        }

        private static ConsoleColor GetColorForSeverity(LogSeverityKind severity)
        {
            return severity switch
            {
                LogSeverityKind.Debug => ConsoleColor.Gray,
                LogSeverityKind.Information => ConsoleColor.White,
                LogSeverityKind.Warning => ConsoleColor.Yellow,
                LogSeverityKind.Error => ConsoleColor.Red,
                LogSeverityKind.Fatal => ConsoleColor.DarkRed,
                _ => ConsoleColor.White
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
            _disposed = true;
        }

        /// <summary>
        /// Finalizer to ensure resources are released
        /// </summary>
        ~ConsoleLogProvider()
        {
            Dispose(false);
        }
    }
}