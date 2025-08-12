using Phyros.Logging.LogWriter;

namespace Phyros.Logging.Wireup
{
	public class PhyrosLoggingOptions
	{
		public ILogProvider LogProvider { get; set; } = null!;


		public ILogProvider Build()
		{
			return LogProvider;
		}
	}
}