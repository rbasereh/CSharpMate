using System;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;

namespace CSharpMate.CommandLine
{
    [Command("csmate", 
        FullName = "CSharpMate", 
        OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    public class CSharpMateCommand : CommandBase
    {
        [Option("-log", Description = "LogEnabled")]
        public bool LogEnabled { get; set; }

        protected override void OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
        }
        private static string GetVersion()
            => typeof(CSharpMateCommand).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    }
}
