using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Phyros.Logging.LogWriter;
using Phyros.Logging.Serilog;
using Phyros.Logging.Wireup;
using Serilog;

namespace Phyros.Logging.Wireup
{
	public static class PhyrosLoggingOptionsExtensions
	{
		/// <summary>
		/// Adds Serilog as the logging provider for Phyros.
		/// </summary>
		/// <param name="options">The Phyros logging options.</param>
		/// <returns>The updated Phyros logging options.</returns>
		public static PhyrosLoggingOptions UseSerilog(this PhyrosLoggingOptions options, ILogger logger)
		{
			if (options == null) throw new ArgumentNullException(nameof(options));
			options.LogProvider = new SerilogLogProvider(logger, disposeLogger: true);
			return options;
		}

		public static PhyrosLoggingOptions UseSerilog(this PhyrosLoggingOptions options, Func<ILogger> loggerFactory)
		{
			if (options == null) throw new ArgumentNullException(nameof(options));
			if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
			var logger = loggerFactory();
			options.LogProvider = new SerilogLogProvider(logger, disposeLogger: true);
			return options;
		}

		public static PhyrosLoggingOptions UseRegisteredSerilog(this PhyrosLoggingOptions options, IServiceCollection collection)
		{
			if (options == null) throw new ArgumentNullException(nameof(options));
			var serviceProvider = collection.BuildServiceProvider();
			var logger = serviceProvider.GetService<ILogger>();
			if (logger == null)
			{
				throw new InvalidOperationException("No Serilog logger registered in the service collection.");
			}
			options.LogProvider = new SerilogLogProvider(logger, disposeLogger: true);
			return options;
		}
	}
}
