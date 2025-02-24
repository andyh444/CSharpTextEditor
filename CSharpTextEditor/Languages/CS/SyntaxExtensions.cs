using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor.Languages.CS
{
    internal static class SyntaxExtensions
    {
        internal static bool IsCommentTrivia(this SyntaxTrivia trivium)
        {
            return trivium.IsKind(SyntaxKind.SingleLineCommentTrivia)
                || trivium.IsKind(SyntaxKind.MultiLineCommentTrivia)
                || trivium.IsKind(SyntaxKind.DocumentationCommentExteriorTrivia)
                || trivium.IsKind(SyntaxKind.EndOfDocumentationCommentToken)
                || trivium.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)
                || trivium.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia);
        }

        internal static bool IsDeclaration(this SyntaxNode node)
        {
            // TODO: Something better
            return node.GetType().ToString().Contains("DeclarationSyntax");
        }
    }
}
