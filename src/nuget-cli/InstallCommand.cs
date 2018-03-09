using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using NuGet.Commands;
using NuGet.Configuration;
using NuGet.Versioning;

namespace NuGet.Cli
{
    class InstallCommand : CommandBase
    {
        [Argument(0, Description = "The package id of the tool you want to install.")]
        [Required]
        public string PackageId { get; }

        [Option(Description = "The version of the package to install.\nIf not specified, the latest version is installed.")]
        public string Version { get; }

        [Option("--verbose", Description = "Show verbose output")]
        public bool Verbose { get; }

        [Option(Description = "If version is not specified, install the latest prerelease versions.")]
        public bool Prerelease { get; }

        [Option(LongName = "output", Description = "The output directory where the package is installed. Defaults to 'packages'")]
        [LegalFilePath]
        public string OutputDirectory { get; }

        [Option("--source <SOURCE>", Description = "Additional feeds to use to install.")]
        public string[] Sources { get; } = Array.Empty<string>();

        private async Task<int> OnExecuteAsync()
        {
            var reporter = new ConsoleReporter(PhysicalConsole.Singleton)
            {
                IsVerbose = Verbose
            };

            var installDir = string.IsNullOrEmpty(OutputDirectory)
                ? Path.Combine(Directory.GetCurrentDirectory(), "packages")
                : Path.GetFullPath(OutputDirectory);

            var tempFilePath = Path.Combine(AppContext.BaseDirectory, "projectThatNeverExists.csproj");
            ISettings settings = Settings.LoadDefaultSettings(tempFilePath);

            VersionRange versionRange;
            if (!string.IsNullOrEmpty(Version))
            {
                if (!VersionRange.TryParse(Version, out versionRange))
                {
                    reporter.Error($"Invalid nuget version '{Version}'");
                    return 1;
                }
            }
            else
            {
                versionRange = Prerelease
                    ? VersionRange.AllFloating
                    : VersionRange.AllStableFloating;
            }

            var logger = new ConsoleNuGetLogger(reporter);

            var results = await RestoreRunnerEx.RunWithoutCommit(tempFilePath,
                installDir,
                PackageId,
                versionRange,
                settings,
                Sources,
                logger);

            var success = false;
            foreach (var result in results)
            {
                if (result.Result.Success)
                {
                    var installedVersion = result.Result.LockFile.Libraries.FirstOrDefault(l => string.Equals(PackageId, l.Name, StringComparison.OrdinalIgnoreCase));
                    if (installedVersion != null)
                    {
                        var path = installedVersion.Path;
                        reporter.Output($"Installed {installedVersion.Name} {installedVersion.Version}");
                        foreach (var file in installedVersion.Files)
                        {
                            reporter.Verbose("Package file: " + file);
                        }
                        success = true;
                        break;
                    }
                }
                else
                {
                    foreach (var unresolved in result.Result.GetAllUnresolved())
                    {
                        reporter.Warn($"Could not find a package {unresolved.Name} in the version range {unresolved.VersionRange}");
                    }
                }
            }

            if (success)
            {
                reporter.Output("Installation succeeded");
                return 0;
            }

            reporter.Error("Installation failed");

            return success ? 1 : 0;
        }
    }
}
