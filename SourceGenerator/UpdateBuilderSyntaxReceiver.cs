using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerator
{
    public class UpdateBuilderSyntaxReceiver : ISyntaxReceiver
    {
        public const string UpdateBuilderAttributeName = "UpdateBuilder";

        public List<ClassDeclarationSyntax> Classes { get; } = new List<ClassDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            // Business logic to decide what we're interested in goes here
            if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax)
            {
                var updateBuilderAttribute = classDeclarationSyntax.AttributeLists.SelectMany(x => x.Attributes).FirstOrDefault(y => y.Name.ToString() == UpdateBuilderAttributeName);

                if (updateBuilderAttribute != null)
                    Classes.Add(classDeclarationSyntax);
            }
        }
    }
}
