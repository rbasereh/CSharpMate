using System;
using System.Reflection;
using CSharpMate.Core;
using CSharpMate.Core.Commands;
using CSharpMate.Core.ProtoToPoco;
using McMaster.Extensions.CommandLineUtils;

namespace CSharpMate.CommandLine
{
    [Command("csmate",
        FullName = "CSharpMate",
        OptionsComparison = StringComparison.InvariantCultureIgnoreCase)]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(
        typeof(ProtoToPocoCommand),
        typeof(ExtractApiCommand)
        )]
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
    public class ProtoToPocoCLICommand : CommandBase
    {
        protected override void OnExecute(CommandLineApplication app)
        {
            base.OnExecute(app);
            CSMateHelper.Execute(new ProtoToPocoCommand());
        }
    }
    public class ExtractApiCLICommand : CommandBase
    {
        protected override void OnExecute(CommandLineApplication app)
        {
            base.OnExecute(app);
            CSMateHelper.Execute(new ExtractApiCommand());
        }
    }
}
