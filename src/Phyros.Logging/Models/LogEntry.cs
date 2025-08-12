using System.Diagnostics;

namespace Phyros.Logging.Models
{
	public record LogEntry
	{
		// required properties
		public LogSeverityKind Severity { get; init; }
		public string MessageTemplate { get; init; }
		public object[] OrderedMessageProperties { get; init; }

		// optional parameters
		public Exception? Exception { get; internal set; }
		internal readonly Dictionary<string, object> InternalProperties = new();
		public IEnumerable<KeyValuePair<string, object>> Properties => InternalProperties.ToArray();
		private Guid _correlationId = Guid.NewGuid();

		public Guid CorrelationId
		{
			get => _correlationId;
			internal set
			{
				if (value != Guid.Empty)
				{
					_correlationId = value;
				}
			}
		}

		public LogEntry(LogSeverityKind severity, string messageTemplate, params object[] orderedMessageProperties)
		{
			System.Diagnostics.Debug.Assert(!String.IsNullOrWhiteSpace(messageTemplate));

			Severity = severity;
			MessageTemplate = messageTemplate;
			OrderedMessageProperties = orderedMessageProperties;
		}

		public static LogEntry Debug(string messageTemplate, params object[] orderedMessageProperties)
		{
			return new LogEntry(LogSeverityKind.Debug, messageTemplate, orderedMessageProperties)
			{
				MessageTemplate = messageTemplate,
				OrderedMessageProperties = orderedMessageProperties
			};
		}

		public static LogEntry Information(string messageTemplate, params object[] orderedMessageProperties)
		{
			return new LogEntry(LogSeverityKind.Information, messageTemplate, orderedMessageProperties)
			{
				MessageTemplate = messageTemplate,
				OrderedMessageProperties = orderedMessageProperties
			};
		}

		public static LogEntry Warning(string messageTemplate, params object[] orderedMessageProperties)
		{
			return new LogEntry(LogSeverityKind.Warning, messageTemplate, orderedMessageProperties)
			{
				MessageTemplate = messageTemplate,
				OrderedMessageProperties = orderedMessageProperties
			};
		}

		public static LogEntry Error(string messageTemplate, Exception ex, params object[] orderedMessageProperties)
		{
			return new LogEntry(LogSeverityKind.Error, messageTemplate, orderedMessageProperties)
			{
				MessageTemplate = messageTemplate,
				OrderedMessageProperties = orderedMessageProperties,
				Exception = ex
			};
		}

		public static LogEntry Fatal(string messageTemplate, Exception ex, params object[] orderedMessageProperties)
		{
			return new LogEntry(LogSeverityKind.Fatal, messageTemplate, orderedMessageProperties)
			{
				MessageTemplate = messageTemplate,
				OrderedMessageProperties = orderedMessageProperties,
				Exception = ex
			};
		}
	}
}
