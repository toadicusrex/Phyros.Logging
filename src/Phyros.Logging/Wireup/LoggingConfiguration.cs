namespace Phyros.Logging.Wireup;

public class LoggingConfiguration
{
	public string ApplicationName { get; set; } = null!;
	public string HostName { get; set; } = Environment.MachineName;
}