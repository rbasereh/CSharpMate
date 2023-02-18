using System;
using CSharpMate.Core;
using CSharpMate.Core.ProtoToPoco;
namespace CSharpMate
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CliHelper.ProjectPath = @"D:\GitHub\rbasereh\GrpcSample\GrpcSampleClient\GrpcSampleClient\GrpcSampleClient.csproj";
            CliHelper.SolutionPath = @"D:\GitHub\rbasereh\GrpcSample\GrpcSampleClient\GrpcSampleClient.sln";
            CliHelper.LogEnabled = true;
            CSharpMate.Core.CSMateHelper.Send(new ProtoToPocoCommand());
            Console.WriteLine("Hello, World!");
        }
    }
}