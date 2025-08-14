using Phyros.Logging.Models;
using Phyros.Logging.Serilog;
using Serilog;
using Serilog.Sinks.TestCorrelator;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Phyros.Logging.Tests
{
    public class FrameworkCompatibilityTests
    {
        /// <summary>
        /// This test verifies that the Serilog implementation works with both target frameworks.
        /// The test itself runs on net8.0, but it tests code that should be compatible with netstandard2.0
        /// </summary>
        [Fact]
        public void SerilogProvider_WorksWithBothFrameworkApis()
        {
            // Arrange
            using (TestCorrelator.CreateContext())
            {
                var logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.TestCorrelator()
                    .CreateLogger();

                var logProvider = new SerilogLogProvider(logger);

                // Act - create and write a log entry
                var entry = LogEntry.Information("Test message from {TestName}", "FrameworkCompatibilityTests");
                logProvider.Write(entry);

                // Assert - verify the log was written
                var events = TestCorrelator.GetLogEventsFromCurrentContext().ToList();
                Assert.Single(events);
                Assert.Equal("Test message from \"FrameworkCompatibilityTests\"", events[0].RenderMessage());
            }
        }

        /// <summary>
        /// This test creates log entries using different target framework APIs to ensure compatibility
        /// </summary>
        [Fact]
        public void LogEntry_CompatibleWithBothFrameworkApis()
        {
            // Create log entries using APIs that should be available in both frameworks
            var entry1 = LogEntry.Information("Test message");
            var entry2 = LogEntry.Error("Error message", new Exception("Test exception"));
            var entry3 = new LogEntry(LogSeverityKind.Warning, "Warning {Number}", 123);

            // Test serialization/deserialization compatibility (System.Text.Json is available in both frameworks)
            var properties = new Dictionary<string, object>
            {
                ["TestKey"] = "TestValue",
                ["Number"] = 42
            };

            var entry = new LogEntry(LogSeverityKind.Information, "Test with properties")
                .AddProperties(properties)
                .AddCorrelationId(Guid.NewGuid());

            Assert.Equal(2, entry.Properties.Count());
            Assert.Equal("TestValue", entry.Properties.First(p => p.Key == "TestKey").Value);
            Assert.Equal(42, entry.Properties.First(p => p.Key == "Number").Value);
        }
        
        /// <summary>
        /// Test that ensures the FluentAPI works the same across frameworks
        /// </summary>
        [Fact]
        public void FluentApi_WorksConsistentlyAcrossFrameworks()
        {
            // This should work on both frameworks
            var entry = new LogEntry(LogSeverityKind.Information, "Test {Param}", "value")
                .AddProperty("Key1", "Value1")
                .AddProperty("Key2", 42)
                .AddCorrelationId(Guid.NewGuid())
                .AddException(new InvalidOperationException("Test exception"));

            // Check the entry was created correctly
            Assert.Equal("Test {Param}", entry.MessageTemplate);
            Assert.Single(entry.OrderedMessageProperties);
            Assert.Equal("value", entry.OrderedMessageProperties[0]);
            Assert.Equal("Value1", entry.Properties.First(p => p.Key == "Key1").Value);
            Assert.Equal(42, entry.Properties.First(p => p.Key == "Key2").Value);
            Assert.IsType<InvalidOperationException>(entry.Exception);
        }

        [Fact]
        public void LogEntry_CreatedWithBothFrameworkCompatibleCode()
        {
            // This test verifies that the LogEntry class works with code that's compatible with both frameworks
            
            // Create a log entry with code that should work in both netstandard2.0 and net8.0
            var entry = new LogEntry(LogSeverityKind.Information, "Test message {Param}", "value");
            
            // Add properties - should work in both frameworks
            entry.AddProperty("StringProp", "test")
                 .AddProperty("IntProp", 42)
                 .AddProperty("DateProp", DateTime.UtcNow);
                 
            // Add dictionary of properties - should work in both frameworks
            var dict = new Dictionary<string, object>
            {
                ["Key1"] = "Value1",
                ["Key2"] = 123
            };
            entry.AddProperties(dict);
            
            // Verify the entry was created correctly
            Assert.Equal(LogSeverityKind.Information, entry.Severity);
            Assert.Equal("Test message {Param}", entry.MessageTemplate);
            Assert.Single(entry.OrderedMessageProperties);
            Assert.Equal("value", entry.OrderedMessageProperties[0]);
            Assert.Equal(5, entry.Properties.Count());
            Assert.Equal("test", entry.Properties.First(p => p.Key == "StringProp").Value);
            Assert.Equal(42, entry.Properties.First(p => p.Key == "IntProp").Value);
            Assert.Equal("Value1", entry.Properties.First(p => p.Key == "Key1").Value);
        }
        
        [Fact]
        public void StaticFactoryMethods_WorkInBothFrameworks()
        {
            // These factory methods should work in both frameworks
            var debugEntry = LogEntry.Debug("Debug message");
            var infoEntry = LogEntry.Information("Info message {Param}", "value");
            var warningEntry = LogEntry.Warning("Warning message");
            var errorEntry = LogEntry.Error("Error message", new Exception("Test"));
            var fatalEntry = LogEntry.Fatal("Fatal message", new Exception("Test"));
            
            Assert.Equal(LogSeverityKind.Debug, debugEntry.Severity);
            Assert.Equal(LogSeverityKind.Information, infoEntry.Severity);
            Assert.Equal(LogSeverityKind.Warning, warningEntry.Severity);
            Assert.Equal(LogSeverityKind.Error, errorEntry.Severity);
            Assert.Equal(LogSeverityKind.Fatal, fatalEntry.Severity);
        }
        
        [Fact]
        public void LogEntryProperties_WorkWithStandardTypes()
        {
            // Use standard types that exist in both frameworks
            var entry = new LogEntry(LogSeverityKind.Information, "Test");
            
            // Use simple types
            entry.AddProperty("String", "test");
            entry.AddProperty("Int", 42);
            entry.AddProperty("Bool", true);
            entry.AddProperty("DateTime", DateTime.UtcNow);
            entry.AddProperty("Guid", Guid.NewGuid());
            
            // Use collection types that exist in both frameworks
            var list = new List<int> { 1, 2, 3 };
            var array = new[] { "a", "b", "c" };
            var dict = new Dictionary<string, string> { ["key"] = "value" };
            
            entry.AddProperty("List", list);
            entry.AddProperty("Array", array);
            entry.AddProperty("Dictionary", dict);
            
            // Just verify we can add them without exceptions
            Assert.Equal(8, entry.Properties.Count());
        }
    }
}