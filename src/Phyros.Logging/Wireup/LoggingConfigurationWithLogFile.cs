namespace Phyros.Logging.Wireup;

public class LoggingConfigurationWithLogFile : LoggingConfiguration
{
	public string BasePath { get; set; } = null!;
	public string? ClientName { get; set; }
}