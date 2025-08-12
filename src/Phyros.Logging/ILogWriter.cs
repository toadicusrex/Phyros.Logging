using Phyros.Logging.Models;

namespace Phyros.Logging
{
	/// <summary>
	/// Defines a method that logs a given message with type and optional source and exception to the logging
	/// store.
	/// </summary>
	/// <remarks>
	/// The <see cref="ILogWriterExtensions"/> class defines convenient extension methods for the 
	/// <see cref="ILogWriter"/> interface.
	/// </remarks>
	public interface ILogWriter
	{
		/// <summary>Logs the specified entry.</summary>
		/// <param name="entry">The entry to log.</param>
		/// <returns>The id of the saved log (or null when an id is not appropriate for this type of logger.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the given <paramref name="entry" /> is a null reference.</exception>
		/// <remarks>Implementations of this method must guarantee to be thread safe.</remarks>
		void Log(LogEntry entry);
	}
}
