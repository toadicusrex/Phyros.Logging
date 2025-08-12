using Microsoft.Extensions.DependencyInjection;
using Phyros.Logging.Models;
using Phyros.Logging.Wireup; 
using Serilog;

namespace Phyros.Logging.ConsoleTester
{
	internal class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Phyros.Logging ConsoleTester");
			Console.WriteLine("============================");
			Console.WriteLine();

			// Example 1: Using existing Serilog logger
			Console.WriteLine("\nExample 1: Using existing Serilog logger");
			TestWithExistingSerilogLogger();
			
			// Example 2: Using registered Serilog logger in DI
			Console.WriteLine("\nExample 2: Using registered Serilog logger in DI");
			TestWithRegisteredSerilogLogger();
			
			// Example 3: Using logger factory
			Console.WriteLine("\nExample 3: Using logger factory");
			TestWithLoggerFactory();

			Console.WriteLine("\nPress any key to exit...");
			Console.ReadKey();
		}
		
		static void TestWithExistingSerilogLogger()
		{
			try
			{
				// Create a service collection
				var services = new ServiceCollection();
				
				// Create a Serilog logger
				var existingLogger = new LoggerConfiguration()
					.MinimumLevel.Debug()
					.WriteTo.Console(outputTemplate: "[Existing] [{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
					.CreateLogger();
					
				// Register using the new approach
				var logWriter = services.AddPhyrosLogger(builder => 
					builder.UseSerilog(existingLogger));
				
				// Log some test messages
				logWriter.Log(LogEntry.Information("This is an information message from existing Serilog logger"));
				logWriter.Log(LogEntry.Warning("This is a warning message"));
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in TestWithExistingSerilogLogger: {ex}");
			}
		}
		
		static void TestWithRegisteredSerilogLogger()
		{
			try
			{
				// Create a service collection
				var services = new ServiceCollection();
				
				// Register Serilog with DI
				var logger = new LoggerConfiguration()
					.MinimumLevel.Debug()
					.WriteTo.Console(outputTemplate: "[Registered] [{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
					.CreateLogger();
					
				services.AddSingleton<ILogger>(logger);
				
				// Register the Phyros logger with the registered Serilog logger
				var logWriter = services.AddPhyrosLogger(builder => 
					builder.UseSerilog(services));
				
				// Log some test messages
				logWriter.Log(LogEntry.Information("This is an information message from DI-registered Serilog logger"));
				
				// Test with correlation ID
				var correlationId = Guid.NewGuid();
				var entry = LogEntry.Information("Message with explicit correlation ID").AddCorrelationId(correlationId);
				logWriter.Log(entry);
				
				// Log entry with the same correlation ID to show grouping
				var followupEntry = LogEntry.Information("Follow-up message with same correlation ID").AddCorrelationId(correlationId);
				logWriter.Log(followupEntry);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in TestWithRegisteredSerilogLogger: {ex}");
			}
		}
		
		static void TestWithLoggerFactory()
		{
			try
			{
				// Create a service collection
				var services = new ServiceCollection();
				
				// Create a logger factory
				Func<ILogger> loggerFactory = () => new LoggerConfiguration()
					.MinimumLevel.Debug()
					.WriteTo.Console(outputTemplate: "[Factory] [{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
					.CreateLogger();
				
				// Register the Phyros logger with the factory
				var logWriter = services.AddPhyrosLogger(builder => 
					builder.UseSerilog(loggerFactory));
				
				// Log some test messages
				logWriter.Log(LogEntry.Information("This is an information message from factory-created Serilog logger"));
				logWriter.Log(LogEntry.Warning("This is a warning message from factory"));
				
				// Test structured logging
				logWriter.Log(LogEntry.Information("Testing structured logging with {Count} items", 42));
				
				// Test with exception
				try
				{
					throw new InvalidOperationException("Test exception for factory logger");
				}
				catch (Exception ex)
				{
					logWriter.Log(LogEntry.Error("Caught exception in factory test", ex));
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in TestWithLoggerFactory: {ex}");
			}
		}
	}
}
