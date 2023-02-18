using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Generator
{
    public class ClassAssembler
    {

        private CompilationUnitSyntax _syntaxFactory;
        private NamespaceDeclarationSyntax _namespace;
        private TypeDeclarationSyntax _class;
        private List<string> _usingDirectives;

        private static SyntaxToken[] _defaultClassModifier => new SyntaxToken[] { SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.SealedKeyword) };
        private static SyntaxToken[] _defaultInterfaceModifier => new SyntaxToken[] { SyntaxFactory.Token(SyntaxKind.PublicKeyword) };

        public ClassAssembler()
        {
            _syntaxFactory = SyntaxFactory.CompilationUnit();
            _usingDirectives = new List<string>();
        }

        public static ClassAssembler Init()
            => new();

        public ClassAssembler CreateNamespace(string namesapce)
        {
            _namespace = SyntaxFactory.NamespaceDeclaration(
               SyntaxFactory.ParseName(namesapce))
               .NormalizeWhitespace();
            return this;
        }
        public ClassAssembler CreateClass(string name)
        {
            _class = SyntaxFactory.ClassDeclaration(name);
            _class = _class.AddModifiers(_defaultClassModifier);
            return this;
        }
        public ClassAssembler CreateInterface(string name)
        {
            _class = SyntaxFactory.InterfaceDeclaration(name);
            _class = _class.AddModifiers(_defaultInterfaceModifier);
            return this;
        }
        public string Generate()
        {
            _namespace = _namespace.AddMembers(_class);
            _syntaxFactory = _syntaxFactory.AddMembers(_namespace);

            var data = _syntaxFactory
                .NormalizeWhitespace()
                .ToString();
            CleanUp();
            return data;
        }

        private void CleanUp()
        {
            _class = null;
            _namespace = null;
            _syntaxFactory = null;
        }

        public ClassAssembler AddMembers(List<(string methodName, string returnType, string parameterType, string parameterName)> methodsMeta)
        {
            foreach (var item in methodsMeta)
            {
                _class = _class.AddMembers(SyntaxFactory.ParseMemberDeclaration($@"
        [OperationContract]
        Task<{item.returnType}> {item.methodName}({item.parameterType} {item.parameterName}, CallContext context = default);
"));
            }
            return this;
        }
        public ClassAssembler AddProperties(List<(string propName, string propType, string propNumber)> properties)
        {
            foreach (var item in properties)
            {
                _class = _class.AddMembers(SyntaxFactory.ParseMemberDeclaration($@"
        [ProtoMember({item.propNumber})]
        public {item.propType} {item.propName} {{ get; set; }}"));
            }
            return this;
        }

        internal ClassAssembler AddAtribute(string atribute)
        {
            var attribute = SyntaxFactory.Attribute(
                        SyntaxFactory.ParseName(atribute));
            _class = _class.AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(attribute));
            return this;
        }

    }
}
