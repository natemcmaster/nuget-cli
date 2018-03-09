using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using NuGet.Common;

namespace NuGet.Cli
{
    class ConsoleNuGetLogger : LoggerBase
    {
        private IReporter _reporter;

        public ConsoleNuGetLogger(IReporter reporter)
        {
            this._reporter = reporter;
        }

        public override void Log(ILogMessage message)
        {
            LogAsync(message);
        }

        public override Task LogAsync(ILogMessage message)
        {
            if (message.Level == LogLevel.Warning)
            {
                _reporter.Warn(message.FormatWithCode());
            }
            else if (message.Level == LogLevel.Error)
            {
                _reporter.Error(message.FormatWithCode());
            }
            else if (message.Level > LogLevel.Information)
            {
                _reporter.Output(message.FormatWithCode());
            }
            else
            {
                _reporter.Verbose(message.FormatWithCode());
            }

            return Task.CompletedTask;
        }
    }
}
