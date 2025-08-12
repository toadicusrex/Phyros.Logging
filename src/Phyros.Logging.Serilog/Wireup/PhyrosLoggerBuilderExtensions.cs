using Microsoft.Extensions.DependencyInjection;
using Phyros.Logging.Serilog;
using Serilog;

namespace Phyros.Logging.Wireup
{
    public static class PhyrosLoggerBuilderExtensions
    {
        /// <summary>
        /// Configures the builder to use a specific Serilog logger instance
        /// </summary>
        /// <param name="builder">The Phyros logger builder</param>
        /// <param name="logger">The Serilog logger to use</param>
        /// <param name="disposeLogger">Whether to dispose the logger when the provider is disposed</param>
        /// <returns>The builder for method chaining</returns>
        public static PhyrosLoggerBuilder UseSerilog(this PhyrosLoggerBuilder builder, ILogger logger, bool disposeLogger = false)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            
            return builder.UseProvider(new SerilogLogProvider(logger, disposeLogger));
        }

        /// <summary>
        /// Configures the builder to use a Serilog logger created by a factory
        /// </summary>
        /// <param name="builder">The Phyros logger builder</param>
        /// <param name="loggerFactory">Factory function to create the Serilog logger</param>
        /// <param name="disposeLogger">Whether to dispose the logger when the provider is disposed</param>
        /// <returns>The builder for method chaining</returns>
        public static PhyrosLoggerBuilder UseSerilog(
            this PhyrosLoggerBuilder builder,
            Func<ILogger> loggerFactory,
            bool disposeLogger = false)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            
            return builder.UseProviderFactory(() => new SerilogLogProvider(loggerFactory(), disposeLogger));
        }

        /// <summary>
        /// Configures the builder to use a Serilog logger from the service collection
        /// </summary>
        /// <param name="builder">The Phyros logger builder</param>
        /// <param name="services">The service collection that has a registered Serilog logger</param>
        /// <param name="disposeLogger">Whether to dispose the logger when the provider is disposed</param>
        /// <returns>The builder for method chaining</returns>
        public static PhyrosLoggerBuilder UseSerilog(
            this PhyrosLoggerBuilder builder,
            IServiceCollection services,
            bool disposeLogger = false)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (services == null) throw new ArgumentNullException(nameof(services));
            
            return builder.UseProviderFactory(() => 
            {
                // Use service provider to resolve ILogger
                var serviceProvider = services.BuildServiceProvider();
                var logger = serviceProvider.GetService<ILogger>();
                
                if (logger == null)
                {
                    throw new InvalidOperationException(
                        "No Serilog ILogger is registered in the service collection. " +
                        "Make sure to call services.AddSerilog() before calling this method, " +
                        "or use a different UseSerilog overload to provide your own logger instance.");
                }
                
                return new SerilogLogProvider(logger, disposeLogger);
            });
        }
        
        /// <summary>
        /// Configures the builder to use a Serilog logger from a service provider
        /// </summary>
        /// <param name="builder">The Phyros logger builder</param>
        /// <param name="serviceProvider">The service provider that has a registered Serilog logger</param>
        /// <param name="disposeLogger">Whether to dispose the logger when the provider is disposed</param>
        /// <returns>The builder for method chaining</returns>
        public static PhyrosLoggerBuilder UseSerilog(
            this PhyrosLoggerBuilder builder,
            IServiceProvider serviceProvider,
            bool disposeLogger = false)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
            
            return builder.UseProviderFactory(() => 
            {
                // Use service provider to resolve ILogger
                var logger = serviceProvider.GetService<ILogger>();
                
                if (logger == null)
                {
                    throw new InvalidOperationException(
                        "No Serilog ILogger is registered in the service provider.");
                }
                
                return new SerilogLogProvider(logger, disposeLogger);
            });
        }
    }
}