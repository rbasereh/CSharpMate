using McMaster.Extensions.CommandLineUtils;

namespace CSharpMate.CommandLine
{
    public abstract class CommandBase
    {
        public virtual CSharpMateCommand RootCommand { get; set; }
        protected virtual void OnExecute(CommandLineApplication app)
        {
        }
    }
}
