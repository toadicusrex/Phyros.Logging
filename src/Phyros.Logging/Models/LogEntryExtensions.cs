namespace Phyros.Logging.Models
{
	public static class LogEntryExtensions
	{
		public static LogEntry AddProperty(this LogEntry entry, string key, object value)
		{
			// ReSharper disable once CanSimplifyDictionaryLookupWithTryAdd, TryAdd isn't available in netstandard
			if (!entry.InternalProperties.ContainsKey(key))
			{
				entry.InternalProperties.Add(key, value);
			}
			return entry;
		}
		public static LogEntry AddProperties(this LogEntry entry, Dictionary<string, object> properties)
		{
			foreach (var potentiallyNewProperty in properties)
			{
				// ReSharper disable once CanSimplifyDictionaryLookupWithTryAdd, TryAdd isn't available in netstandard
				if (!entry.InternalProperties.ContainsKey(potentiallyNewProperty.Key))
				{
					entry.InternalProperties.Add(potentiallyNewProperty.Key, potentiallyNewProperty.Value);
				}
			}
			return entry;
		}

		public static LogEntry AddProperties(this LogEntry entry,
				params KeyValuePair<string, object>[] properties)
		{
			foreach (var potentiallyNewProperty in properties)
			{
				// ReSharper disable once CanSimplifyDictionaryLookupWithTryAdd, TryAdd isn't available in netstandard
				if (!entry.InternalProperties.ContainsKey(potentiallyNewProperty.Key))
				{
					entry.InternalProperties.Add(potentiallyNewProperty.Key, potentiallyNewProperty.Value);
				}
			}
			return entry;
		}

		public static LogEntry AddCorrelationId(this LogEntry entry, Guid correlationId)
		{
			if (correlationId != Guid.Empty)
			{
				entry.CorrelationId = correlationId;
			}
			return entry;
		}

		public static LogEntry AddException(this LogEntry entry, Exception exception)
		{
			entry.Exception = exception;
			return entry;
		}
	}
}
