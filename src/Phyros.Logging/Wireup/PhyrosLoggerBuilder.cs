using Phyros.Logging.LogWriter;
using Microsoft.Extensions.DependencyInjection;

namespace Phyros.Logging.Wireup
{
    public class PhyrosLoggerBuilder
    {
        private ILogProvider? _logProvider;
        private Func<ILogProvider>? _logProviderFactory;
        private IServiceCollection? _services;

        /// <summary>
        /// Sets a specific log provider instance to use
        /// </summary>
        /// <param name="logProvider">The log provider to use</param>
        /// <returns>The builder for method chaining</returns>
        public PhyrosLoggerBuilder UseProvider(ILogProvider logProvider)
        {
            _logProvider = logProvider ?? throw new ArgumentNullException(nameof(logProvider));
            _logProviderFactory = null;
            return this;
        }

        /// <summary>
        /// Sets a factory function that will create the log provider
        /// </summary>
        /// <param name="factory">Factory function to create the log provider</param>
        /// <returns>The builder for method chaining</returns>
        public PhyrosLoggerBuilder UseProviderFactory(Func<ILogProvider> factory)
        {
            _logProviderFactory = factory ?? throw new ArgumentNullException(nameof(factory));
            _logProvider = null;
            return this;
        }

        /// <summary>
        /// Registers the services collection for potential provider resolution
        /// </summary>
        internal PhyrosLoggerBuilder WithServices(IServiceCollection services)
        {
            _services = services;
            return this;
        }

        /// <summary>
        /// Builds the log provider based on the configuration
        /// </summary>
        /// <returns>The configured log provider</returns>
        public ILogProvider Build()
        {
            // Direct provider has highest priority
            if (_logProvider != null)
            {
                return _logProvider;
            }

            // Factory has second priority
            if (_logProviderFactory != null)
            {
                return _logProviderFactory();
            }

            // No provider configured
            throw new InvalidOperationException(
                "No log provider has been configured. Use UseProvider or UseProviderFactory to configure a provider.");
        }
    }
}