using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpTextEditor
{
    internal class SyntaxHighlighter : CSharpSyntaxWalker
    {
        private readonly SyntaxTree syntaxTree;
        private readonly Action<TextSpan, Color> highlightAction;

        public SyntaxHighlighter(SyntaxTree syntaxTree, Action<TextSpan, Color> highlightAction)
        {
            this.syntaxTree = syntaxTree;
            this.highlightAction = highlightAction;
        }

        public override void VisitAttribute(AttributeSyntax node)
        {
            base.VisitAttribute(node);
            highlightAction(node.Name.Span, Color.SteelBlue);
        }

        public override void VisitStructDeclaration(StructDeclarationSyntax node)
        {
            highlightAction(node.Identifier.Span, Color.SteelBlue);
            base.VisitStructDeclaration(node);
        }

        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            highlightAction(node.Identifier.Span, Color.SteelBlue);
            base.VisitEnumDeclaration(node);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            // Highlight class names
            highlightAction(node.Identifier.Span, Color.SteelBlue);
            if (node.BaseList != null)
            {
                foreach (var baseType in node.BaseList.Types)
                {
                    highlightAction(baseType.Span, Color.SteelBlue);
                }
            }
            base.VisitClassDeclaration(node);
        }

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            base.VisitConstructorDeclaration(node);
            highlightAction(node.Identifier.Span, Color.SteelBlue);
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            // Highlight method names
            highlightAction(node.Identifier.Span, Color.FromArgb(136, 108, 64));
            foreach (ParameterSyntax parameter in node.ParameterList.Parameters)
            {
                if (parameter.Type is IdentifierNameSyntax ins)
                {
                    highlightAction(ins.FullSpan, Color.SteelBlue);
                }
            }
            base.VisitMethodDeclaration(node);
        }

        public override void VisitArgumentList(ArgumentListSyntax node)
        {
            base.VisitArgumentList(node);
        }

        public override void VisitArgument(ArgumentSyntax node)
        {
            base.VisitArgument(node);
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            //highlightAction(node.Expression.Span, Color.FromArgb(136, 108, 64));
            base.VisitInvocationExpression(node);
            if (node.Expression is MemberAccessExpressionSyntax syntax)
            {
                highlightAction(syntax.Name.FullSpan, Color.FromArgb(136, 108, 64));
                //if (syntax.Expression is IdentifierNameSyntax name)
                {
                    //highlightAction(name.FullSpan, Color.Blue);
                }

            }
            else if (node.Expression is IdentifierNameSyntax syntax1)
            {
                highlightAction(syntax1.Identifier.FullSpan, Color.FromArgb(136, 108, 64));
            }
        }

        public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            base.VisitAssignmentExpression(node);
        }

        public override void VisitDeclarationExpression(DeclarationExpressionSyntax node)
        {
            base.VisitDeclarationExpression(node);
        }

        public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            base.VisitVariableDeclaration(node);
            //node.
        }
    }
}
