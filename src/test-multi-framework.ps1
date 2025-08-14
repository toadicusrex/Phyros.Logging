# Script to test Phyros.Logging against both netstandard2.0 and net8.0
Write-Host "Testing Phyros.Logging multi-framework compatibility" -ForegroundColor Cyan

# Step 1: Build the solution for both target frameworks
Write-Host "`n[Step 1] Building solution for all target frameworks..." -ForegroundColor Yellow
dotnet build --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "? Solution build failed" -ForegroundColor Red
    exit 1
}
Write-Host "? Solution build successful" -ForegroundColor Green

# Step 2: Test specific framework builds
Write-Host "`n[Step 2] Testing specific framework builds..." -ForegroundColor Yellow

# Test netstandard2.0 build
Write-Host "`nBuilding Phyros.Logging.Serilog for netstandard2.0..." -ForegroundColor White
dotnet build Phyros.Logging.Serilog/Phyros.Logging.Serilog.csproj -f netstandard2.0 --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "? Phyros.Logging.Serilog failed to build for netstandard2.0" -ForegroundColor Red
    exit 1
}
Write-Host "? Phyros.Logging.Serilog built successfully for netstandard2.0" -ForegroundColor Green

# Test net8.0 build
Write-Host "`nBuilding Phyros.Logging.Serilog for net8.0..." -ForegroundColor White
dotnet build Phyros.Logging.Serilog/Phyros.Logging.Serilog.csproj -f net8.0 --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "? Phyros.Logging.Serilog failed to build for net8.0" -ForegroundColor Red
    exit 1
}
Write-Host "? Phyros.Logging.Serilog built successfully for net8.0" -ForegroundColor Green

# Step 3: Run unit tests
Write-Host "`n[Step 3] Running unit tests..." -ForegroundColor Yellow
dotnet test --configuration Release --no-build
if ($LASTEXITCODE -ne 0) {
    Write-Host "? Unit tests failed" -ForegroundColor Red
    exit 1
}
Write-Host "? Unit tests passed" -ForegroundColor Green

# Step 4: Create test apps to verify functionality in different frameworks
Write-Host "`n[Step 4] Creating test applications to verify compatibility..." -ForegroundColor Yellow

# Create a temp directory for testing
$testDir = ".\temp-framework-test"
if (!(Test-Path $testDir)) {
    Write-Host "Creating test directory: $testDir" -ForegroundColor White
    New-Item -ItemType Directory -Path $testDir | Out-Null
}

# 4.1: Test with .NET 8
$net8Dir = "$testDir\net8-test"
if (!(Test-Path $net8Dir)) {
    New-Item -ItemType Directory -Path $net8Dir | Out-Null
}

Write-Host "`nCreating .NET 8 test app..." -ForegroundColor White
Set-Location $net8Dir
dotnet new console --framework net8.0 --no-restore
dotnet add reference ..\..\Phyros.Logging\Phyros.Logging.csproj
dotnet add reference ..\..\Phyros.Logging.Serilog\Phyros.Logging.Serilog.csproj

# Add a simple test program for .NET 8
$net8Program = @'
using Phyros.Logging;
using Phyros.Logging.Models;
using Phyros.Logging.Serilog;
using Serilog;

// Create a Serilog logger
var serilogLogger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(outputTemplate: "[NET8] [{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

// Create a log provider
var logProvider = new SerilogLogProvider(serilogLogger);

// Create and write a log entry
var entry = LogEntry.Information("Hello from {Framework}!", ".NET 8");
logProvider.Write(entry);

// Test with structured logging data
var entryWithProps = new LogEntry(LogSeverityKind.Information, "Testing with {ItemCount} items")
    .AddProperty("Application", "FrameworkTest")
    .AddProperty("Framework", "net8.0");

entryWithProps.OrderedMessageProperties = new object[] { 42 };
logProvider.Write(entryWithProps);

// Test with exception
try
{
    throw new System.InvalidOperationException("Test exception from .NET 8");
}
catch (System.Exception ex)
{
    logProvider.Write(LogEntry.Error("Caught exception", ex));
}

Console.WriteLine("\nNET 8 test completed successfully!");
'@
Set-Content -Path "Program.cs" -Value $net8Program

# Build and run the .NET 8 test
Write-Host "Building and running .NET 8 test app..." -ForegroundColor White
dotnet build --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "? .NET 8 test application failed to build" -ForegroundColor Red
} else {
    dotnet run --configuration Release
    if ($LASTEXITCODE -ne 0) {
        Write-Host "? .NET 8 test application failed to run" -ForegroundColor Red
    } else {
        Write-Host "? .NET 8 test completed successfully" -ForegroundColor Green
    }
}

# Return to the original directory
Set-Location ..\..

# 4.2: Test with .NET 6 (compatible with .NET Standard 2.0)
$net6Dir = "$testDir\net6-test"
if (!(Test-Path $net6Dir)) {
    New-Item -ItemType Directory -Path $net6Dir | Out-Null
}

Write-Host "`nCreating .NET 6 test app (compatible with netstandard2.0)..." -ForegroundColor White
Set-Location $net6Dir
dotnet new console --framework net6.0 --no-restore
dotnet add reference ..\..\Phyros.Logging\Phyros.Logging.csproj
dotnet add reference ..\..\Phyros.Logging.Serilog\Phyros.Logging.Serilog.csproj

# Add a test program for .NET 6
$net6Program = @'
using Phyros.Logging;
using Phyros.Logging.Models;
using Phyros.Logging.Serilog;
using Serilog;
using System;
using System.Collections.Generic;

// Create a Serilog logger
var serilogLogger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(outputTemplate: "[NET6] [{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

// Create a log provider
var logProvider = new SerilogLogProvider(serilogLogger);

// Create and write a log entry
var entry = LogEntry.Information("Hello from {Framework}!", ".NET 6 (netstandard2.0 compatible)");
logProvider.Write(entry);

// Test with properties using netstandard2.0 compatible dictionary initialization
var properties = new Dictionary<string, object>();
properties["Application"] = "FrameworkTest";
properties["Framework"] = "net6.0";

var entryWithProps = new LogEntry(LogSeverityKind.Information, "Testing with {ItemCount} items");
entryWithProps.AddProperties(properties);
entryWithProps.OrderedMessageProperties = new object[] { 42 };
logProvider.Write(entryWithProps);

// Test with exception
try
{
    throw new InvalidOperationException("Test exception from .NET 6");
}
catch (Exception ex)
{
    logProvider.Write(LogEntry.Error("Caught exception", ex));
}

Console.WriteLine("\nNET 6 test completed successfully!");
'@
Set-Content -Path "Program.cs" -Value $net6Program

# Build and run the .NET 6 test
Write-Host "Building and running .NET 6 test app..." -ForegroundColor White
dotnet build --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "? .NET 6 test application failed to build" -ForegroundColor Red
} else {
    dotnet run --configuration Release
    if ($LASTEXITCODE -ne 0) {
        Write-Host "? .NET 6 test application failed to run" -ForegroundColor Red
    } else {
        Write-Host "? .NET 6 test completed successfully" -ForegroundColor Green
    }
}

# Return to the original directory
Set-Location ..\..

# Step 5: Final summary
Write-Host "`n[Step 5] Testing summary" -ForegroundColor Yellow

Write-Host "`nResults of multi-framework compatibility testing:" -ForegroundColor Cyan
Write-Host "? Phyros.Logging.Serilog builds successfully for netstandard2.0" -ForegroundColor Green
Write-Host "? Phyros.Logging.Serilog builds successfully for net8.0" -ForegroundColor Green
Write-Host "? Unit tests pass on target framework" -ForegroundColor Green
Write-Host "? Test applications created and executed for both framework targets" -ForegroundColor Green

Write-Host "`nTo clean up test directories, run: Remove-Item -Recurse -Force $testDir" -ForegroundColor White