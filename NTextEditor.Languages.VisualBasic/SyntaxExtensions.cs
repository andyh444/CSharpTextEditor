using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTextEditor.Languages.VisualBasic
{
    internal static class SyntaxExtensions
    {
        internal static bool IsCommentTrivia(this SyntaxTrivia trivium)
        {
            return trivium.IsKind(SyntaxKind.CommentTrivia)
                || trivium.IsKind(SyntaxKind.DocumentationCommentLineBreakToken)
                || trivium.IsKind(SyntaxKind.DocumentationCommentTrivia)
                || trivium.IsKind(SyntaxKind.DocumentationCommentExteriorTrivia);
        }
    }
}
