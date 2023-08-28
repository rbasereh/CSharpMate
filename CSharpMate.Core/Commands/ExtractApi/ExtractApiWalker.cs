using Generator;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System;

namespace CSharpMate.Core.ExtractApi
{
    internal class ExtractApiWalker : CLICSharpSyntaxWalkerBase
    {
        public ExtractApiWalker(SemanticModel model) : base(model)
        {
        }
        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            CheckNode(node);
            base.VisitClassDeclaration(node);
        }
        private void CheckNode(ClassDeclarationSyntax node)
        {
            ITypeSymbol symbol = model.GetDeclaredSymbol(node);


            var isController = node.AttributeLists.Any(attrList =>
                attrList.Attributes.Any(attr =>
                    attr.Name.ToString() == "ApiController"));

            if (!isController)
                return;

            foreach (var method in node.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                var httpAttribute = method.AttributeLists.SelectMany(attrList => attrList.Attributes)
                    .FirstOrDefault(attr => attr.Name.ToString() == "HttpGet" || attr.Name.ToString() == "HttpPost");

                if (httpAttribute != null)
                {
                    string methodName = method.Identifier.ValueText;
                    string httpMethod = httpAttribute.Name.ToString().Replace("Http", "").ToUpper();
                    string routeTemplate = node.AttributeLists.SelectMany(attrList => attrList.Attributes)
                        .FirstOrDefault(attr => attr.Name.ToString() == "Route")?
                        .ArgumentList?.Arguments.FirstOrDefault()?.Expression.ToString();

                    string routeMethodTemplate = method.AttributeLists.SelectMany(attrList => attrList.Attributes)
                    .FirstOrDefault(attr => attr.Name.ToString() == "Route")?
                    .ArgumentList?.Arguments.FirstOrDefault()?.Expression.ToString();

                    var authorizeAttribute = method.AttributeLists
                       .SelectMany(attrList => attrList.Attributes)
                       .FirstOrDefault(attr => attr.Name.ToString() == "Authorize")
                       ?.ArgumentList?.Arguments.FirstOrDefault()?.ToString();

                    ExtractApiCommandHandler.Result.Add(new ExtractApiResult()
                    {
                        ControllerName = symbol.Name,
                        HttpMethod = httpMethod,
                        RouteTemplate = routeTemplate,
                        RouteMethodTemplate = routeMethodTemplate,
                        MethodName = methodName,
                        Authorize = authorizeAttribute
                    });
                }
            }
        }
    }

}

