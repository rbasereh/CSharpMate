using Generator;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using CSharpMate.Core.Commands;

namespace CSharpMate.Core.ProtoToPoco
{
    public static class ProtoToPocoHelper
    {
        public static string Path { get; set; }
    }

    internal class ProtoToPocoCommandHandler : CommandHandlerBase<ProtoToPocoCommand>
    {
        public override async Task RunAsync(ProtoToPocoCommand command, CancellationToken cancellationToken)
        {
            ProtoToPocoHelper.Path = Path.GetFullPath(Path.Combine(CliHelper.ProjectPath, "..", "GeneratedFiles"));
            var docs = await CSMateHelper.GetDocuments(CliHelper.ProjectPath);
            await Walker<ProtoToPocoWalker>(docs, cancellationToken);
        }
    }

}

