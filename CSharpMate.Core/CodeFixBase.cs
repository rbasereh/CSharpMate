using MediatR;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CSharpMate.Core
{
    public interface ICodeFixBase<T>
        where T : ICodeFixCommandBase
    {
        Task RunAsync(T command, CancellationToken cancellationToken);
    }
    public abstract class CommandHandlerBase<T> : CodeFixBase<T>, IRequestHandler<T>
         where T : CommandBase
    {
        public async Task<Unit> Handle(T request, CancellationToken cancellationToken)
        {
            await RunAsync(request, cancellationToken);
            return await Unit.Task;
        }

    }
    public abstract class CodeFixBase<T> : ICodeFixBase<T>
    where T : ICodeFixCommandBase
    {
        public abstract Task RunAsync(T command, CancellationToken cancellationToken);

        public async Task Walker<TCustomWalker>(Solution solution, CancellationToken cancellationToken, IEnumerable<Document> documents = null)
           where TCustomWalker : CLICSharpSyntaxWalkerBase
        {
            documents ??= solution.Projects.SelectMany(e => e.Documents);
            foreach (var document in documents)
            {
                var root = await document.GetSyntaxRootAsync(cancellationToken);
                var model = await document.GetSemanticModelAsync(cancellationToken);
                if (model != null)
                {
                    var serviceSyntaxWalker = (TCustomWalker)Activator.CreateInstance(typeof(TCustomWalker), new object[] { model });
                    serviceSyntaxWalker.Visit(root);
                }
            }
        }

        public async Task ReWriter<TCustomWalker>(Solution solution, string[] layers, CancellationToken cancellationToken, string[] customSubsysList = null)
           where TCustomWalker : CLICSharpSyntaxRewriterBase
        {
            IEnumerable<Document> documents = documents = solution.Projects.SelectMany(e => e.Documents);
            foreach (var document in documents)
            {
                var root = await document.GetSyntaxRootAsync(cancellationToken);
                var model = await document.GetSemanticModelAsync(cancellationToken);
                var serviceSyntaxWalker = CreateDefaultWalker<TCustomWalker>(model);
                root = serviceSyntaxWalker.Visit(root);
                solution = solution.WithDocumentSyntaxRoot(document.Id, root);
            }
            var result = CSMateHelper.Workspace.TryApplyChanges(solution);
        }
        public virtual TWalker CreateDefaultWalker<TWalker>(SemanticModel model)
                => (TWalker)Activator.CreateInstance(typeof(TWalker), model);

    }
    public interface ICodeFixCommandBase
    {
        CancellationToken CancellationToken { get; set; }
    }
    public class CommandBase : CodeFixCommandBase, IRequest
    {
    }
    public class CodeFixCommandBase : ICodeFixCommandBase
    {
        public CancellationToken CancellationToken { get; set; } = new CancellationToken();
    }

    public class CLICSharpSyntaxWalkerBase : CSharpSyntaxWalker
    {
        protected SemanticModel model;
        public CLICSharpSyntaxWalkerBase(SemanticModel model)
        {
            this.model = model;
        }
    }
    public class CLICSharpSyntaxRewriterBase : CSharpSyntaxRewriter
    {
        protected SemanticModel model;
        public CLICSharpSyntaxRewriterBase(SemanticModel model)
        {
            this.model = model;
        }
    }
}