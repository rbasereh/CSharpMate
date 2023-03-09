using System;
using CSharpMate.Core;
using CSharpMate.Core.ProtoToPoco;
namespace CSharpMate
{
/// <summary>
/// for run Core wihtout CLI
/// </summary>
    internal class Program
    {
        static void Main(string[] args)
        {
            CliHelper.ProjectPath = @"ProjectPath";
            CliHelper.LogEnabled = true;
            CSMateHelper.Send(new ProtoToPocoCommand());
            Console.WriteLine("Hello, World!");
        }
    }
}