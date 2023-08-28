using CSharpMate.Core;
using CSharpMate.Core.Commands;
using System;
namespace CSharpMate
{
    /// <summary>
    /// for run Core wihtout CLI
    /// </summary>
    internal class Program
    {
        static void Main(string[] args)
        {
            var x = DateTime.Now;
            var str = Newtonsoft.Json.JsonConvert.SerializeObject(x);



            CliHelper.LogEnabled = true;
            CSMateHelper.Send(new ExtractApiCommand());
            Console.WriteLine("Hello, World!");
        }
    }
}