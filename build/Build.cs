using System;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.PathConstruction;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Tools.DotNet;

class Build : NukeBuild
{
	/// Support plugins are available for:
	///   - JetBrains ReSharper        https://nuke.build/resharper
	///   - JetBrains Rider            https://nuke.build/rider
	///   - Microsoft VisualStudio     https://nuke.build/visualstudio
	///   - Microsoft VSCode           https://nuke.build/vscode

	[Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
	readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

	// API key for GitHub Packages
	readonly string NuGetApiKey = Environment.GetEnvironmentVariable("PACKAGES_API_KEY");
	readonly string NuGetSource = "https://nuget.pkg.github.com/toadicusrex/index.json";

	// API key for NuGet.org
	readonly string NuGetOrgApiKey = Environment.GetEnvironmentVariable("NUGET_ORG_API_KEY");
	readonly string NuGetOrgSource = "https://api.nuget.org/v3/index.json";

	// Let Nuke automatically find the solution without hardcoding the path
	[Solution] readonly Solution Solution;
	[GitVersion] readonly GitVersion GitVersion;
	[Parameter("NuGet version from GitVersion")] readonly string GitVersionNuGetVersion;

	Target Clean => _ => _
			.Before(Restore)
			.Executes(() =>
			{
				// Clean bin/obj from all projects and artifacts directory
				var directoriesToClean = Solution.AllProjects.SelectMany(p => new[]
					{
								p.Directory / "bin",
								p.Directory / "obj"
					}).Concat(new[] { RootDirectory / "artifacts" });
				foreach (var dir in directoriesToClean)
				{
					if (Directory.Exists(dir))
						Directory.Delete(dir, true);
				}
			});

	Target Restore => _ => _
			.Executes(() =>
			{
				DotNetTasks.DotNetRestore(s => s
					.SetProjectFile(Solution));
			});

	Target Compile => _ => _
			.DependsOn(Restore)
			.Executes(() =>
			{
				DotNetTasks.DotNetBuild(s => s
							.SetProjectFile(Solution)
							.SetConfiguration(Configuration)
							.EnableNoRestore());
			});

	Target Pack => _ => _
		.DependsOn(Compile)
		.Executes(() =>
		{
			var version = GitVersion?.NuGetVersion ?? GitVersionNuGetVersion ?? "1.0.0";
			Console.WriteLine($"[Pack] Using version: {version}");
			
			// Create the artifacts directory if it doesn't exist
			var artifactsDirectory = RootDirectory / "artifacts";
			Directory.CreateDirectory(artifactsDirectory);
			
			// Get all projects that are class libraries (not console apps or test projects)
			var projectsToPack = Solution.AllProjects
				.Where(p => IsPackableProject(p))
				.ToList();
				
			Console.WriteLine($"[Pack] Found {projectsToPack.Count} projects to pack:");
			foreach (var project in projectsToPack)
			{
				Console.WriteLine($"[Pack] - {project.Name}");
				
				DotNetTasks.DotNetPack(s => s
					.SetProject(project)
					.SetConfiguration(Configuration)
					.SetVersion(version)
					.SetOutputDirectory(artifactsDirectory)
					.EnableIncludeSymbols()
					.SetSymbolPackageFormat(DotNetSymbolPackageFormat.snupkg));
			}
				
			// Log which packages were created
			var packageFiles = Directory.GetFiles(artifactsDirectory, "*.nupkg");
			Console.WriteLine($"[Pack] Created {packageFiles.Length} packages:");
			foreach (var package in packageFiles)
			{
				Console.WriteLine($"[Pack] - {Path.GetFileName(package)}");
			}
		});

	/// <summary>
	/// Determines if a project should be packaged as a NuGet package
	/// </summary>
	private bool IsPackableProject(Project project)
	{
		// Skip projects with "Test" in the name
		if (project.Name.Contains("Test"))
			return false;
			
		// Skip projects with "ConsoleTester" or "Console" in the name
		if (project.Name.Contains("Console") || project.Name.EndsWith(".ConsoleTester"))
			return false;

		// Skip build project
		if (project.Name == "_build")
			return false;

		try
		{
			// Check if this is a console application by looking at the output type
			var projectFile = File.ReadAllText(project.Path);
			if (projectFile.Contains("<OutputType>Exe</OutputType>"))
				return false;
		}
		catch
		{
			// If we can't read the file, assume it's not packable
			return false;
		}
		
		return true;
	}

	/// <summary>
	/// Publishes packages to GitHub Packages repository
	/// Requires PACKAGES_API_KEY environment variable to be set
	/// </summary>
	Target Publish => _ => _
		.DependsOn(Pack)
		.OnlyWhenStatic(() => IsServerBuild)
		.Executes(() =>
		{
			var artifactsDir = Path.Combine(RootDirectory, "artifacts");
			if (!Directory.Exists(artifactsDir))
			{
				Console.WriteLine($"[Publish] Artifacts directory '{artifactsDir}' does not exist. Skipping publish.");
				return;
			}
			var packageFiles = Directory.GetFiles(artifactsDir, "*.nupkg");
			foreach (var packageFile in packageFiles)
			{
				DotNetTasks.DotNetNuGetPush(s => s
					.SetTargetPath(packageFile)
					.SetSource(NuGetSource)
					.SetApiKey(NuGetApiKey)
					.SetProcessAdditionalArguments(new[] { "--skip-duplicate" }));
			}
		});

	/// <summary>
	/// Publishes packages to NuGet.org
	/// Requires NUGET_ORG_API_KEY environment variable to be set
	/// </summary>
	Target PublishToNugetDotOrg => _ => _
			.DependsOn(Publish)  // Changed from Publish to Pack to make it independent
			.OnlyWhenStatic(() => IsServerBuild)
			.Executes(() =>
			{
				var artifactsDir = Path.Combine(RootDirectory, "artifacts");
				if (!Directory.Exists(artifactsDir))
				{
					Console.WriteLine($"[PublishToNugetDotOrg] Artifacts directory '{artifactsDir}' does not exist. Skipping publish.");
					return;
				}

				if (string.IsNullOrEmpty(NuGetOrgApiKey))
				{
					Console.WriteLine("[PublishToNugetDotOrg] NuGet.org API key (NUGET_ORG_API_KEY) not provided. Skipping publish to NuGet.org.");
					return;
				}

				var packageFiles = Directory.GetFiles(artifactsDir, "*.nupkg");
				foreach (var packageFile in packageFiles)
				{
					Console.WriteLine($"[PublishToNugetDotOrg] Publishing {Path.GetFileName(packageFile)} to NuGet.org");
					DotNetTasks.DotNetNuGetPush(s => s
								.SetTargetPath(packageFile)
								.SetSource(NuGetOrgSource)
								.SetApiKey(NuGetOrgApiKey)
								.SetProcessAdditionalArguments(new[] { "--skip-duplicate" }));
				}
			});

	public static int Main() => Execute<Build>(x => IsServerBuild ? x.PublishToNugetDotOrg : x.Pack);
}
