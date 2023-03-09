using Generator;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace CSharpMate.Core.ProtoToPoco
{
    internal class ProtoToPocoWalker : CLICSharpSyntaxWalkerBase
    {
        public ProtoToPocoWalker(SemanticModel model) : base(model)
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

            if (symbol != null
                && symbol.IsAssignableFrom("IBufferMessage"))
            {
                CreateRequest(symbol, node);
            }
            else
            {
                var servicefieldDeclaration = node.Members.Where(e => e.Kind() == SyntaxKind.FieldDeclaration)
                    .FirstOrDefault(e => (e as FieldDeclarationSyntax).Declaration.Variables[0].Identifier.Text == "__ServiceName")
                    as FieldDeclarationSyntax;
                if (servicefieldDeclaration != null)
                {
                    var serviceName = servicefieldDeclaration.Declaration.Variables[0].Initializer.Value.ToString().Replace("\"", "");
                    CreateService(symbol, node, serviceName);
                }
            }
        }

        private void CreateService(ITypeSymbol symbol, ClassDeclarationSyntax node, string serviceName)
        {
            var types = node.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList()
                .Where(e => (e.BaseList != null && e.BaseList.ToString().Contains("ClientBase<"))
                || e.Identifier.Text.EndsWith("Base")).ToList();
            if (!types.Any())
                return;

            var globalNamespace = symbol.ContainingNamespace.ToString();
            foreach (var type in types)
            {
                var mem = type.DescendantNodes().OfType<MethodDeclarationSyntax>()
                    .ToList().Where(e => e.Modifiers.ToString().Contains("public virtual")).ToList();
                var members = mem.GroupBy(e => e.Identifier.Text)
                    .Where(e => !e.Key.EndsWith("Async"))
                    .Select(e => new
                    {
                        e.Key,
                        Method = e.First()
                    }).ToList();

                List<(string methodName, string returnType, string parameterType, string parameterName)>
                    methodsMeta = new();
                foreach (var Method in members)
                {
                    var methodSymbol = model.GetDeclaredSymbol(Method.Method);
                    var returnType = methodSymbol.ReturnType.ToString();
                    var taskusing = "System.Threading.Tasks.Task";
                    if (returnType.StartsWith(taskusing))
                        returnType = returnType.Substring(taskusing.Length + 1, returnType.Length - taskusing.Length - 2);
                    var request = methodSymbol.Parameters.First();
                    returnType = returnType.Replace(globalNamespace + ".", "");
                    methodsMeta.Add((Method.Key, returnType, request.Type.ToString().Replace(globalNamespace + ".", ""), request.Name));
                }
                var service = "I" + type.Identifier.Text.RemoveFromEnd("Base", "Client");
                var result = ClassAssembler.Init()
                    .CreateInterface(service)
                    .AddAtribute($"ServiceContract(Name = \"{serviceName}\")")
                    .CreateNamespace(globalNamespace)
                    .AddMembers(methodsMeta)
                    .Generate();
                result = result.Insert(0,
@"using ProtoBuf.Grpc;
using System.ServiceModel;
using System.Threading.Tasks;
");
                var foldername = Path.GetFileNameWithoutExtension(node.SyntaxTree.FilePath);
                FileHelper.CreateFile(Path.Combine(ProtoToPocoHelper.Path, service + ".cs"), result);
            }

        }

        private void CreateRequest(ITypeSymbol symbol, ClassDeclarationSyntax node)
        {
            var fields = node.DescendantNodes().OfType<FieldDeclarationSyntax>()
                    .Where(e => e.Declaration.Variables[0].Identifier.Text.EndsWith("FieldNumber"))
                    .ToList()
                    .ToDictionary(e =>
                        e.Declaration.Variables[0].Identifier.Text.RemoveFromEnd("FieldNumber")
                        , v => v.Declaration.Variables[0].Initializer.Value.ToString());

            if (fields.Count == 0)
                return;

            var props = node.DescendantNodes().OfType<PropertyDeclarationSyntax>().ToList()
                 .Where(e => fields.ContainsKey(e.Identifier.Text)).ToList();

            List<(string propName, string propType, string propNumber)> properties = new();
            props.ForEach(e =>
            {
                var propSymbol = model.GetDeclaredSymbol(e);
                properties.Add((propSymbol.Name, propSymbol.Type.ToString(), fields[propSymbol.Name]));
            });
            var globalNamespace = symbol.ContainingNamespace.ToString();
            var objectName = symbol.Name;
            var result = ClassAssembler.Init()
                .CreateClass(objectName)
                .CreateNamespace(globalNamespace)
                .AddAtribute("ProtoContract")
                .AddProperties(properties)
                .Generate();


            result = result.Insert(0,
@"using ProtoBuf;
using Google.Protobuf.Collections;
");
            var foldername = Path.GetFileNameWithoutExtension(node.SyntaxTree.FilePath);
            FileHelper.CreateFile(Path.Combine(ProtoToPocoHelper.Path, foldername, objectName + ".cs"), result);
        }
    }

}

