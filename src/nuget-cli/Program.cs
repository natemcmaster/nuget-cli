using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using NuGet.Commands;
using NuGet.Configuration;
using NuGet.Versioning;

namespace NuGet.Cli
{
    [Subcommand("install", typeof(InstallCommand))]
    [Command(
        FullName = "NuGet Command Line Utility",
        Name = "nuget-cli",
        Description = "A simple NuGet command line utility")]
    [VersionOptionFromMember(MemberName = nameof(GetVersion))]
    class Program : CommandBase
    {
        static void Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        public static string GetVersion() => typeof(Program)
            .Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            .InformationalVersion;

        public int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return 3;
        }
    }
}
