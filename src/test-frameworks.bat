@echo off
echo Testing multi-framework compatibility...

echo.
echo Building Phyros.Logging.Serilog for netstandard2.0...
dotnet build Phyros.Logging.Serilog/Phyros.Logging.Serilog.csproj -f netstandard2.0 -c Release
if %ERRORLEVEL% neq 0 (
    echo ERROR: Build failed for netstandard2.0
    exit /b 1
)
echo SUCCESS: Build passed for netstandard2.0

echo.
echo Building Phyros.Logging.Serilog for net8.0...
dotnet build Phyros.Logging.Serilog/Phyros.Logging.Serilog.csproj -f net8.0 -c Release
if %ERRORLEVEL% neq 0 (
    echo ERROR: Build failed for net8.0
    exit /b 1
)
echo SUCCESS: Build passed for net8.0

echo.
echo Running tests...
dotnet test -c Release
if %ERRORLEVEL% neq 0 (
    echo ERROR: Tests failed
    exit /b 1
)
echo SUCCESS: All tests passed

echo.
echo Multi-framework compatibility tests completed successfully!