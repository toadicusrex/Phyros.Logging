namespace Phyros.Logging.Models
{
	public enum LogSeverityKind
	{
		/// <summary>
		/// A debug event. This indicates a verbose event, useful during development.
		/// </summary>
		Debug = 0,

		/// <summary>
		/// An information event. This indicates a significant, successful operation.
		/// </summary>
		Information = 1,

		/// <summary>
		/// A warning event. This indicates a problem that is not immediately significant, but that may 
		/// signify conditions that could cause future problems.
		/// </summary>
		Warning = 2,

		/// <summary>
		/// An error event. This indicates a significant problem the user should know about; usually a loss of
		/// functionality or data.
		/// </summary>
		Error = 3,

		/// <summary>
		/// A fatal event. This indicates a fatal error or application crash.
		/// </summary>
		Fatal = 4,
	}

}
