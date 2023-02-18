//using Microsoft.Extensions.Logging;
using MediatR;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
namespace CSharpMate.Core
{
    public static class MediatRHelper
    {
        public static void Execute(IRequest command)
        {
            CSMateHelper.RegisterMediateR();
            CSMateHelper.Mediator.Send(command).GetAwaiter().GetResult();
        }
        public static TResponse Execute<TResponse>(IRequest<TResponse> command)
            where TResponse : class
        {
            CSMateHelper.RegisterMediateR();
            var response = CSMateHelper.Mediator.Send(command).GetAwaiter().GetResult();
            return response;
        }
    }
    public static class CSMateHelper
    {
        public static MSBuildWorkspace Workspace { get; set; }
        public static Solution Solution { get; set; }
        internal static IMediator Mediator { get; set; }
        public static void RegisterMediateR()
        {
            var serviceCollection = new ServiceCollection()
                .AddMediatR(Assembly.GetExecutingAssembly())
                .BuildServiceProvider();
            Mediator = serviceCollection.GetRequiredService<IMediator>();
        }
        public static void Execute<T>(T command)
         where T : class, IRequest, ICodeFixCommandBase
        {
            RegisterMediateR();
            ExecuteAsync(command).GetAwaiter().GetResult();
        }
        public static void Send<T>(T command)
          where T : class, IRequest
        {
            RegisterMediateR();
            Mediator.Send(command).GetAwaiter().GetResult();
        }
        public static async Task<IEnumerable<Document>> GetDocuments(string projectfile)
        {
            var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
            var instance = visualStudioInstances.Length == 1
                ? visualStudioInstances[0]
                : SelectVisualStudioInstance(visualStudioInstances);
            MSBuildLocator.RegisterInstance(instance);
            // Load the project
            MSBuildWorkspace workspace = MSBuildWorkspace.Create();
            Project project = await workspace.OpenProjectAsync(projectfile);
            var docs = project.Documents;
            return docs;
        }
        public static async Task ExecuteAsync<T>(T command)
            where T : class, ICodeFixCommandBase
        {
            var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
            var instance = visualStudioInstances.Length == 1
                ? visualStudioInstances[0]
                : SelectVisualStudioInstance(visualStudioInstances);
            MSBuildLocator.RegisterInstance(instance);
            var solutionFileInfo = new FileInfo(CliHelper.SolutionPath);
            Logger.Log($"SolutionPath : {CliHelper.SolutionPath}");

            var properties = new Dictionary<string, string>() {
                    {"CheckForSystemRuntimeDependency", "true" },
                    { "DesignTimeBuild", "true" },
                    { "BuildingInsideVisualStudio", "true" },
                    //{ "TargetFrameworks", "net472" },
                    { "SignAssembly", "false" },
                    { "DelaySign", "true"},
                    };
            using (var workspace = MSBuildWorkspace.Create())
            {
                try
                {
                    await RunCodeFixAsync<T>(workspace, command, solutionFileInfo);
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message, LogLevel.Information);
                    throw;
                }
            }
        }
        private static async Task RunCodeFixAsync<T>(MSBuildWorkspace workspace, T command, FileInfo solutionFileInfo)
            where T : class, ICodeFixCommandBase
        {
            var solution = await workspace.OpenSolutionAsync(solutionFileInfo.FullName, new ConsoleProgressReporter(), command.CancellationToken);
            Solution = solution;
            Workspace = workspace;
            await Mediator.Send(command, command.CancellationToken);

        }
        private static VisualStudioInstance SelectVisualStudioInstance(VisualStudioInstance[] visualStudioInstances)
        {
            return visualStudioInstances.OrderByDescending(v => v.Version).First();
        }

        internal static void GetDocuments(object projectPath)
        {
            throw new NotImplementedException();
        }

        private class ConsoleProgressReporter : IProgress<ProjectLoadProgress>
        {
            public void Report(ProjectLoadProgress loadProgress)
            {
                if (CliHelper.LogEnabled)
                {
                    var projectDisplay = Path.GetFileName(loadProgress.FilePath);
                    if (loadProgress.TargetFramework != null)
                        projectDisplay += $" ({loadProgress.TargetFramework})";
                    if (loadProgress.Operation == ProjectLoadOperation.Resolve)
                        Console.WriteLine($"{projectDisplay}");
                }
            }
        }
    }
    public static class CliHelper
    {
        public static string SolutionPath { get; set; }
        public static string ProjectPath { get; set; }
        public static bool LogEnabled { get; set; }
    }
}
