using Phyros.Logging.Models;

namespace Phyros.Logging.Tests
{
    public class LogEntryTests
    {
        [Fact]
        public void Constructor_SetsRequiredProperties()
        {
            // Arrange
            var severity = LogSeverityKind.Information;
            var messageTemplate = "Test message {Param}";
            var param = "value";

            // Act
            var entry = new LogEntry(severity, messageTemplate, param);

            // Assert
            Assert.Equal(severity, entry.Severity);
            Assert.Equal(messageTemplate, entry.MessageTemplate);
            Assert.Single(entry.OrderedMessageProperties);
            Assert.Equal(param, entry.OrderedMessageProperties[0]);
            Assert.NotEqual(Guid.Empty, entry.CorrelationId);
        }

        [Fact]
        public void StaticFactoryMethods_CreateEntriesWithCorrectSeverity()
        {
            // Arrange & Act
            var debugEntry = LogEntry.Debug("Debug message");
            var infoEntry = LogEntry.Information("Info message");
            var warningEntry = LogEntry.Warning("Warning message");
            var exception = new InvalidOperationException("Test exception");
            var errorEntry = LogEntry.Error("Error message", exception);
            var fatalEntry = LogEntry.Fatal("Fatal message", exception);

            // Assert
            Assert.Equal(LogSeverityKind.Debug, debugEntry.Severity);
            Assert.Equal(LogSeverityKind.Information, infoEntry.Severity);
            Assert.Equal(LogSeverityKind.Warning, warningEntry.Severity);
            Assert.Equal(LogSeverityKind.Error, errorEntry.Severity);
            Assert.Equal(LogSeverityKind.Fatal, fatalEntry.Severity);
            
            Assert.Null(debugEntry.Exception);
            Assert.Null(infoEntry.Exception);
            Assert.Null(warningEntry.Exception);
            Assert.Same(exception, errorEntry.Exception);
            Assert.Same(exception, fatalEntry.Exception);
        }

        [Fact]
        public void AddProperty_AddsPropertyToInternalCollection()
        {
            // Arrange
            var entry = new LogEntry(LogSeverityKind.Information, "Test");
            var key = "TestKey";
            var value = "TestValue";

            // Act
            var result = entry.AddProperty(key, value);

            // Assert
            Assert.Same(entry, result); // Fluent API returns the same instance
            var property = Assert.Single(entry.Properties);
            Assert.Equal(key, property.Key);
            Assert.Equal(value, property.Value);
        }

        [Fact]
        public void AddProperty_IgnoresDuplicateKeys()
        {
            // Arrange
            var entry = new LogEntry(LogSeverityKind.Information, "Test");
            var key = "TestKey";
            var value1 = "TestValue1";
            var value2 = "TestValue2";
            
            // Act
            entry.AddProperty(key, value1);
            entry.AddProperty(key, value2); // Try to add with same key
            
            // Assert
            var property = Assert.Single(entry.Properties);
            Assert.Equal(key, property.Key);
            Assert.Equal(value1, property.Value); // Should keep the first value
        }

        [Fact]
        public void AddProperties_Dictionary_AddsMultiplePropertiesToInternalCollection()
        {
            // Arrange
            var entry = new LogEntry(LogSeverityKind.Information, "Test");
            var properties = new Dictionary<string, object>
            {
                { "Key1", "Value1" },
                { "Key2", 123 }
            };

            // Act
            var result = entry.AddProperties(properties);

            // Assert
            Assert.Same(entry, result); // Fluent API returns the same instance
            Assert.Equal(2, entry.Properties.Count());
            Assert.Contains(entry.Properties, p => p.Key == "Key1" && p.Value.Equals("Value1"));
            Assert.Contains(entry.Properties, p => p.Key == "Key2" && p.Value.Equals(123));
        }

        [Fact]
        public void AddProperties_KeyValuePairs_AddsMultiplePropertiesToInternalCollection()
        {
            // Arrange
            var entry = new LogEntry(LogSeverityKind.Information, "Test");
            var property1 = new KeyValuePair<string, object>("Key1", "Value1");
            var property2 = new KeyValuePair<string, object>("Key2", 123);

            // Act
            var result = entry.AddProperties(property1, property2);

            // Assert
            Assert.Same(entry, result); // Fluent API returns the same instance
            Assert.Equal(2, entry.Properties.Count());
            Assert.Contains(entry.Properties, p => p.Key == "Key1" && p.Value.Equals("Value1"));
            Assert.Contains(entry.Properties, p => p.Key == "Key2" && p.Value.Equals(123));
        }

        [Fact]
        public void AddProperties_WithDuplicateKeys_KeepsFirstValue()
        {
            // Arrange
            var entry = new LogEntry(LogSeverityKind.Information, "Test");
            entry.AddProperty("ExistingKey", "ExistingValue");
            
            var properties = new Dictionary<string, object>
            {
                { "ExistingKey", "NewValue" }, // This should be ignored
                { "NewKey", "Value" }
            };

            // Act
            entry.AddProperties(properties);

            // Assert
            Assert.Equal(2, entry.Properties.Count());
            Assert.Contains(entry.Properties, p => p.Key == "ExistingKey" && p.Value.Equals("ExistingValue")); // Original value preserved
            Assert.Contains(entry.Properties, p => p.Key == "NewKey" && p.Value.Equals("Value")); // New key added
        }

        [Fact]
        public void AddCorrelationId_SetsCorrelationIdWhenNotEmpty()
        {
            // Arrange
            var entry = new LogEntry(LogSeverityKind.Information, "Test");
            var originalId = entry.CorrelationId;
            var newId = Guid.NewGuid();

            // Act
            var result = entry.AddCorrelationId(newId);

            // Assert
            Assert.Same(entry, result); // Fluent API returns the same instance
            Assert.Equal(newId, entry.CorrelationId);
            Assert.NotEqual(originalId, entry.CorrelationId);
        }

        [Fact]
        public void AddCorrelationId_DoesNotSetEmptyCorrelationId()
        {
            // Arrange
            var entry = new LogEntry(LogSeverityKind.Information, "Test");
            var originalId = entry.CorrelationId;

            // Act
            var result = entry.AddCorrelationId(Guid.Empty);

            // Assert
            Assert.Same(entry, result);
            Assert.Equal(originalId, entry.CorrelationId); // ID should not have changed
        }

        [Fact]
        public void AddException_SetsExceptionProperty()
        {
            // Arrange
            var entry = new LogEntry(LogSeverityKind.Information, "Test");
            var exception = new InvalidOperationException("Test exception");

            // Act
            var result = entry.AddException(exception);

            // Assert
            Assert.Same(entry, result); // Fluent API returns the same instance
            Assert.Same(exception, entry.Exception);
        }

        [Fact] 
        public void AddException_CanOverwriteExistingException()
        {
            // Arrange
            var exception1 = new InvalidOperationException("Test exception 1");
            var exception2 = new ArgumentException("Test exception 2");
            var entry = new LogEntry(LogSeverityKind.Error, "Test");
            entry.AddException(exception1);

            // Act
            entry.AddException(exception2);

            // Assert
            Assert.Same(exception2, entry.Exception); // Should be replaced
        }

        [Fact]
        public void FluentApi_CanChainMultipleCalls()
        {
            // Arrange
            var exception = new InvalidOperationException("Test exception");
            var correlationId = Guid.NewGuid();

            // Act
            var entry = new LogEntry(LogSeverityKind.Information, "Test")
                .AddProperty("Key1", "Value1")
                .AddProperty("Key2", 123)
                .AddCorrelationId(correlationId)
                .AddException(exception);

            // Assert
            Assert.Equal(2, entry.Properties.Count());
            Assert.Equal(correlationId, entry.CorrelationId);
            Assert.Same(exception, entry.Exception);
        }
    }
}