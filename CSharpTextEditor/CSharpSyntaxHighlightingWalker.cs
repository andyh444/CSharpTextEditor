using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using System.Xml.Linq;

namespace CSharpTextEditor
{
    internal class CSharpSyntaxHighlightingWalker : CSharpSyntaxWalker
    {
        private readonly SyntaxPalette _palette;
        private readonly Action<TextSpan, Color> _highlightAction;
        private readonly SemanticModel _semanticModel;

        public CSharpSyntaxHighlightingWalker(SemanticModel semanticModel, Action<TextSpan, Color> highlightAction, SyntaxPalette palette)
        {
            _semanticModel = semanticModel;
            _highlightAction = highlightAction;
            _palette = palette;
        }

        #region Declarations
        public override void VisitStructDeclaration(StructDeclarationSyntax node)
        {
            HighlightTypeDeclarationSyntax(node);
            _highlightAction(node.Keyword.Span, _palette.BlueKeywordColour);
            base.VisitStructDeclaration(node);
        }

        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            HighlightTypeDeclarationSyntax(node);
            _highlightAction(node.EnumKeyword.Span, _palette.BlueKeywordColour);
            base.VisitEnumDeclaration(node);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            HighlightTypeDeclarationSyntax(node);
            _highlightAction(node.Keyword.Span, _palette.BlueKeywordColour);
            base.VisitClassDeclaration(node);
        }

        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            HighlightTypeDeclarationSyntax(node);
            _highlightAction(node.Keyword.Span, _palette.BlueKeywordColour);
            base.VisitInterfaceDeclaration(node);
        }

        public override void VisitRecordDeclaration(RecordDeclarationSyntax node)
        {
            HighlightTypeDeclarationSyntax(node);
            _highlightAction(node.Keyword.Span, _palette.BlueKeywordColour);
            base.VisitRecordDeclaration(node);
        }

        private void HighlightTypeDeclarationSyntax(BaseTypeDeclarationSyntax node)
        {
            _highlightAction(node.Identifier.Span, _palette.TypeColour);
            HighlightModifiers(node.Modifiers);
        }

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            _highlightAction(node.Identifier.Span, _palette.TypeColour);
            HighlightModifiers(node.Modifiers);
            base.VisitConstructorDeclaration(node);
        }

        public override void VisitConstructorInitializer(ConstructorInitializerSyntax node)
        {
            base.VisitConstructorInitializer(node);
            _highlightAction(node.ThisOrBaseKeyword.Span, _palette.BlueKeywordColour);
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            // Highlight method names
            _highlightAction(node.Identifier.Span, _palette.MethodColour);
            HighlightModifiers(node.Modifiers);
            base.VisitMethodDeclaration(node);
        }

        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            HighlightModifiers(node.Modifiers);
            base.VisitFieldDeclaration(node);
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            HighlightModifiers(node.Modifiers);
            base.VisitPropertyDeclaration(node);
        }

        public override void VisitIndexerDeclaration(IndexerDeclarationSyntax node)
        {
            HighlightModifiers(node.Modifiers);
            _highlightAction(node.ThisKeyword.Span, _palette.BlueKeywordColour);
            base.VisitIndexerDeclaration(node);
        }

        public override void VisitAccessorDeclaration(AccessorDeclarationSyntax node)
        {
            HighlightModifiers(node.Modifiers);
            _highlightAction(node.Keyword.Span, _palette.BlueKeywordColour);
            base.VisitAccessorDeclaration(node);
        }

        public override void VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
        {
            HighlightModifiers(node.Modifiers);
            _highlightAction(node.EventKeyword.Span, _palette.BlueKeywordColour);
            base.VisitEventFieldDeclaration(node);
        }

        public override void VisitEventDeclaration(EventDeclarationSyntax node)
        {
            HighlightModifiers(node.Modifiers);
            _highlightAction(node.EventKeyword.Span, _palette.BlueKeywordColour);
            base.VisitEventDeclaration(node);
        }

        public override void VisitDelegateDeclaration(DelegateDeclarationSyntax node)
        {
            HighlightModifiers(node.Modifiers);
            _highlightAction(node.DelegateKeyword.Span, _palette.BlueKeywordColour);
            base.VisitDelegateDeclaration(node);
        }

        private void HighlightModifiers(SyntaxTokenList modifiers)
        {
            foreach (var modifier in modifiers)
            {
                _highlightAction(modifier.Span, _palette.BlueKeywordColour);
            }
        }
        #endregion

        #region Statements
        public override void VisitIfStatement(IfStatementSyntax node)
        {
            _highlightAction(node.IfKeyword.Span, _palette.PurpleKeywordColour);
            base.VisitIfStatement(node);
        }

        public override void VisitElseClause(ElseClauseSyntax node)
        {
            _highlightAction(node.ElseKeyword.Span, _palette.PurpleKeywordColour);
            base.VisitElseClause(node);
        }

        public override void VisitDoStatement(DoStatementSyntax node)
        {
            _highlightAction(node.DoKeyword.Span, _palette.PurpleKeywordColour);
            _highlightAction(node.WhileKeyword.Span, _palette.PurpleKeywordColour);
            base.VisitDoStatement(node);
        }

        public override void VisitWhileStatement(WhileStatementSyntax node)
        {
            _highlightAction(node.WhileKeyword.Span, _palette.PurpleKeywordColour);
            base.VisitWhileStatement(node);
        }

        public override void VisitForStatement(ForStatementSyntax node)
        {
            _highlightAction(node.ForKeyword.Span, _palette.PurpleKeywordColour);
            base.VisitForStatement(node);
        }

        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            _highlightAction(node.ForEachKeyword.Span, _palette.PurpleKeywordColour);
            base.VisitForEachStatement(node);
        }

        public override void VisitSwitchStatement(SwitchStatementSyntax node)
        {
            _highlightAction(node.SwitchKeyword.Span, _palette.PurpleKeywordColour);
            base.VisitSwitchStatement(node);
        }

        public override void VisitCaseSwitchLabel(CaseSwitchLabelSyntax node)
        {
            _highlightAction(node.Keyword.Span, _palette.PurpleKeywordColour);
            base.VisitCaseSwitchLabel(node);
        }

        public override void VisitDefaultSwitchLabel(DefaultSwitchLabelSyntax node)
        {
            _highlightAction(node.Keyword.Span, _palette.PurpleKeywordColour);
            base.VisitDefaultSwitchLabel(node);
        }

        public override void VisitGotoStatement(GotoStatementSyntax node)
        {
            _highlightAction(node.GotoKeyword.Span, _palette.PurpleKeywordColour);
            base.VisitGotoStatement(node);
        }

        public override void VisitReturnStatement(ReturnStatementSyntax node)
        {
            _highlightAction(node.ReturnKeyword.Span, _palette.PurpleKeywordColour);
            base.VisitReturnStatement(node);
        }

        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            _highlightAction(node.UsingKeyword.Span, _palette.BlueKeywordColour);
            base.VisitUsingDirective(node);
        }

        public override void VisitUsingStatement(UsingStatementSyntax node)
        {
            _highlightAction(node.UsingKeyword.Span, _palette.BlueKeywordColour);
            base.VisitUsingStatement(node);
        }
        #endregion

        #region Expressions
        public override void VisitThrowExpression(ThrowExpressionSyntax node)
        {
            _highlightAction(node.ThrowKeyword.Span, _palette.PurpleKeywordColour);
            base.VisitThrowExpression(node);
        }

        public override void VisitAwaitExpression(AwaitExpressionSyntax node)
        {
            _highlightAction(node.AwaitKeyword.Span, _palette.BlueKeywordColour);
            base.VisitAwaitExpression(node);
        }

        public override void VisitBaseExpression(BaseExpressionSyntax node)
        {
            _highlightAction(node.Span, _palette.BlueKeywordColour);
            base.VisitBaseExpression(node);
        }

        public override void VisitThisExpression(ThisExpressionSyntax node)
        {
            _highlightAction(node.Span, _palette.BlueKeywordColour);
            base.VisitThisExpression(node);
        }

        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            _highlightAction(node.NewKeyword.Span, _palette.BlueKeywordColour);
            base.VisitObjectCreationExpression(node);
        }

        public override void VisitInterpolatedStringExpression(InterpolatedStringExpressionSyntax node)
        {
            base.VisitInterpolatedStringExpression(node);
            _highlightAction(node.StringStartToken.Span, _palette.StringLiteralColour);
            _highlightAction(node.StringEndToken.Span, _palette.StringLiteralColour);
        }

        public override void VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            base.VisitLiteralExpression(node);
            switch (node.Kind())
            {
                case SyntaxKind.StringLiteralExpression:
                    _highlightAction(node.Span, _palette.StringLiteralColour);
                    break;
                case SyntaxKind.TrueLiteralExpression:
                case SyntaxKind.FalseLiteralExpression:
                    _highlightAction(node.Span, _palette.BlueKeywordColour);
                    break;
            }
        }

        public override void VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            base.VisitBinaryExpression(node);
            if (node.IsKind(SyntaxKind.AsExpression)
                || node.IsKind(SyntaxKind.IsExpression))
            {
                _highlightAction(node.OperatorToken.Span, _palette.BlueKeywordColour);
            }
        }
        #endregion

        public override void VisitPredefinedType(PredefinedTypeSyntax node)
        {
            base.VisitPredefinedType(node);
            _highlightAction(node.Span, _palette.BlueKeywordColour);
        }

        public override void VisitIncompleteMember(IncompleteMemberSyntax node)
        {
            HighlightModifiers(node.Modifiers);
            base.VisitIncompleteMember(node);
        }

        public override void VisitAttribute(AttributeSyntax node)
        {
            base.VisitAttribute(node);
            _highlightAction(node.Name.Span, Color.SteelBlue);
        }

        public override void VisitInterpolatedStringText(InterpolatedStringTextSyntax node)
        {
            base.VisitInterpolatedStringText(node);
            _highlightAction(node.Span, _palette.StringLiteralColour);
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            ISymbol? symbol = _semanticModel.GetSymbolInfo(node).Symbol;
            if (symbol != null)
            {
                if (symbol is ITypeSymbol typeSymbol)
                {
                    _highlightAction(node.Span, _palette.TypeColour);
                }
                else if (symbol is IMethodSymbol methodSymbol)
                {
                    _highlightAction(node.Span, _palette.MethodColour);
                }
            }
            base.VisitIdentifierName(node);
        }

        private void HighlightTypeSyntax(TypeSyntax typeSyntax)
        {
            if (typeSyntax is IdentifierNameSyntax identifierNameSyntax)
            {
                _highlightAction(identifierNameSyntax.Span, _palette.TypeColour);
            }
            else if (typeSyntax is GenericNameSyntax genericNameSyntax)
            {
                _highlightAction(genericNameSyntax.Identifier.Span, _palette.TypeColour);
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
