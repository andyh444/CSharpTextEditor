using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;

namespace CSharpTextEditor
{
    internal class CSharpSyntaxHighlightingWalker : CSharpSyntaxWalker
    {
        private SyntaxPalette _palette;
        private readonly Action<TextSpan, Color> highlightAction;

        public CSharpSyntaxHighlightingWalker(Action<TextSpan, Color> highlightAction, SyntaxPalette palette)
        {
            this.highlightAction = highlightAction;
            _palette = palette;
        }

        public override void VisitAttribute(AttributeSyntax node)
        {
            base.VisitAttribute(node);
            highlightAction(node.Name.Span, Color.SteelBlue);
        }

        public override void VisitStructDeclaration(StructDeclarationSyntax node)
        {
            highlightAction(node.Identifier.Span, _palette.ClassColour);
            base.VisitStructDeclaration(node);
        }

        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            highlightAction(node.Identifier.Span, _palette.ClassColour);
            base.VisitEnumDeclaration(node);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            // Highlight class names
            highlightAction(node.Identifier.Span, _palette.ClassColour);
            if (node.BaseList != null)
            {
                foreach (BaseTypeSyntax baseType in node.BaseList.Types)
                {
                    HighlightTypeSyntax(baseType.Type);
                }
            }
            base.VisitClassDeclaration(node);
        }

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            base.VisitConstructorDeclaration(node);
            highlightAction(node.Identifier.Span, _palette.ClassColour);
            foreach (ParameterSyntax parameter in node.ParameterList.Parameters)
            {
                if (parameter.Type != null)
                {
                    HighlightTypeSyntax(parameter.Type);
                }
            }
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            // Highlight method names
            highlightAction(node.Identifier.Span, _palette.MethodColour);
            foreach (ParameterSyntax parameter in node.ParameterList.Parameters)
            {
                if (parameter.Type != null)
                {
                    HighlightTypeSyntax(parameter.Type);
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
            base.VisitInvocationExpression(node);
            if (node.Expression is MemberAccessExpressionSyntax syntax)
            {
                HighlightExpressionSyntax(syntax.Expression);
                highlightAction(syntax.Name.FullSpan, _palette.MethodColour);
            }
            else if (node.Expression is IdentifierNameSyntax syntax1)
            {
                highlightAction(syntax1.Identifier.FullSpan, _palette.MethodColour);
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
            HighlightTypeSyntax(node.Type);
            foreach (VariableDeclaratorSyntax variable in node.Variables)
            {
                if (variable.Initializer != null)
                {
                    HighlightExpressionSyntax(variable.Initializer.Value);
                }
            }
            //node.
        }

        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            base.VisitObjectCreationExpression(node);
            HighlightTypeSyntax(node.Type);
            if (node.ArgumentList != null)
            {
                foreach (ArgumentSyntax argument in node.ArgumentList.Arguments)
                {
                    HighlightExpressionSyntax(argument.Expression);
                }
            }
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            base.VisitPropertyDeclaration(node);
            HighlightTypeSyntax(node.Type);
        }

        private void HighlightExpressionSyntax(ExpressionSyntax expressionSyntax)
        {
            if (expressionSyntax is TypeSyntax typeSyntax)
            {
                HighlightTypeSyntax(typeSyntax);
            }
            else if (expressionSyntax is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
            {
                HighlightExpressionSyntax(memberAccessExpressionSyntax.Expression);
            }
            else if (expressionSyntax is ObjectCreationExpressionSyntax objectCreationExpressionSyntax)
            {
                // do nothing - handled by VisitObjectCreationExpression
            }
            else
            {
                 //Debugger.Break();
            }
        }

        private void HighlightTypeSyntax(TypeSyntax typeSyntax)
        {
            if (typeSyntax is IdentifierNameSyntax identifierNameSyntax)
            {
                highlightAction(identifierNameSyntax.Span, _palette.ClassColour);
            }
            else if (typeSyntax is GenericNameSyntax genericNameSyntax)
            {
                highlightAction(genericNameSyntax.Identifier.Span, _palette.ClassColour);
                foreach (TypeSyntax typeArgument in genericNameSyntax.TypeArgumentList.Arguments)
                {
                    HighlightTypeSyntax(typeArgument);
                }
            }
            else if (typeSyntax is NullableTypeSyntax nullableTypeSyntax)
            {
                HighlightTypeSyntax(nullableTypeSyntax.ElementType);
            }
            else if (typeSyntax is ArrayTypeSyntax arrayTypeSyntax)
            {
                HighlightTypeSyntax(arrayTypeSyntax.ElementType);
            }
            else if (typeSyntax is TupleTypeSyntax tupleTypeSyntax)
            {
                foreach (TupleElementSyntax element in tupleTypeSyntax.Elements)
                {
                    HighlightTypeSyntax(element.Type);
                }
            }
            else if (!(typeSyntax is PredefinedTypeSyntax))
            {
                Debugger.Break();
            }
        }

    }
}
