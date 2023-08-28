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

namespace CSharpMate.Core.ExtractApi
{
    public static class ExtractApiHelper
    {
        public static string Path { get; set; }
    }
    public class ExtractApiResult
    {
        public string ControllerName { get; set; }
        public string MethodName { get; set; }
        public string HttpMethod { get; set; }
        public string RouteTemplate { get; set; }
        public string Authorize { get; internal set; }
        public string RouteMethodTemplate { get; internal set; }
    }

    internal class ExtractApiCommandHandler : CommandHandlerBase<ExtractApiCommand>
    {
        public static List<ExtractApiResult> Result { get; set; } = new();
        public override async Task RunAsync(ExtractApiCommand command, CancellationToken cancellationToken)
        {
            ExtractApiHelper.Path = Path.GetFullPath(Path.Combine(CliHelper.ProjectPath, "..", "GeneratedFiles"));
            var docs = await CSMateHelper.GetDocuments(CliHelper.ProjectPath);
            await Walker<ExtractApiWalker>(docs, cancellationToken);

            var str = string.Join(System.Environment.NewLine, Result.Select(e => $"{e.ControllerName}#{e.MethodName}#{e.HttpMethod}#{e.Authorize ?? "-"}#{e.RouteTemplate ?? "-"}#{e.RouteMethodTemplate ?? "-"}"));
            File.WriteAllText("D:\\SSO.csv", str);
            var x = 1;
        }
    }

}

