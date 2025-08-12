using Phyros.Logging;
using Phyros.Logging.LogWriter;
using Phyros.Logging.Wireup;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
	// ReSharper disable once InconsistentNaming
	public static class IServiceCollectionExtensions
	{
		// Static field to track the global instance for idempotent registration
		private static ILogWriter? _globalLogWriter;
		private static readonly object _lock = new object();

		/// <summary>
		/// Adds Phyros logging to the service collection with the specified configuration
		/// </summary>
		/// <param name="services">The service collection</param>
		/// <param name="configureAction">Action to configure the logger builder</param>
		/// <returns>The registered log writer</returns>
		public static ILogWriter AddPhyrosLogger(this IServiceCollection services, Action<PhyrosLoggerBuilder> configureAction)
		{
			if (services == null) throw new ArgumentNullException(nameof(services));
			if (configureAction == null) throw new ArgumentNullException(nameof(configureAction));

			// Check for existing logger to make registration idempotent
			lock (_lock)
			{
				if (_globalLogWriter != null)
				{
					return _globalLogWriter;
				}

				var builder = new PhyrosLoggerBuilder().WithServices(services);
				
				// Apply the configuration
				configureAction(builder);
				
				// Build the provider
				var provider = builder.Build();
				
				// Create the log writer
				var logWriter = new PhyrosLogWriter(provider);
				
				// Register the services
				services.AddSingleton<ILogProvider>(provider);
				services.AddSingleton<ILogWriter>(logWriter);
				
				_globalLogWriter = logWriter;
				return _globalLogWriter;
			}
		}

		/// <summary>
		/// Adds Phyros logging with a specific provider to the service collection
		/// </summary>
		/// <param name="services">The service collection</param>
		/// <param name="provider">The log provider to use</param>
		/// <returns>The registered log writer</returns>
		public static ILogWriter AddPhyrosLogger(
			this IServiceCollection services,
			ILogProvider provider)
		{
			return services.AddPhyrosLogger(builder => builder.UseProvider(provider));
		}

		/// <summary>
		/// Registers a custom log provider with the service collection using the factory pattern
		/// </summary>
		/// <param name="services">The service collection</param>
		/// <param name="providerFactory">A factory function that creates a log provider</param>
		/// <returns>The registered ILogWriter</returns>
		public static ILogWriter AddPhyrosLogger(
			this IServiceCollection services,
			Func<ILogProvider> providerFactory)
		{
			return services.AddPhyrosLogger(builder => builder.UseProviderFactory(providerFactory));
		}
	}
}
