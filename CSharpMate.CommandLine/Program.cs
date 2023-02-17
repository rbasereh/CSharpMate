using McMaster.Extensions.CommandLineUtils;
namespace CSharpMate.CommandLine
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CommandLineApplication.Execute<CSharpMateCommand>(args);
        }
    }
}
