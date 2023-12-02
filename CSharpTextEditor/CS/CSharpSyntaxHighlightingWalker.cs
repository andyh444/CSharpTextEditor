using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using System.Xml.Linq;
using System;
using System.Drawing;

namespace CSharpTextEditor.CS
{
    internal class CSharpSyntaxHighlightingWalker : CSharpSyntaxWalker
    {
        private readonly SyntaxPalette _palette;
        private readonly Action<TextSpan, Color> _highlightAction;
        private readonly Action<TextSpan> _addBlockAction;
        private readonly SemanticModel _semanticModel;

        public CSharpSyntaxHighlightingWalker(SemanticModel semanticModel, Action<TextSpan, Color> highlightAction, Action<TextSpan> addBlockAction, SyntaxPalette palette)
        {
            _semanticModel = semanticModel;
            _highlightAction = highlightAction;
            _addBlockAction = addBlockAction;
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

        public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            _highlightAction(node.NamespaceKeyword.Span, _palette.BlueKeywordColour);
            base.VisitNamespaceDeclaration(node);
        }

        public override void VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
        {
            _highlightAction(node.NamespaceKeyword.Span, _palette.BlueKeywordColour);
            base.VisitFileScopedNamespaceDeclaration(node);
        }

        public override void VisitTypeParameter(TypeParameterSyntax node)
        {
            _highlightAction(node.Identifier.Span, _palette.TypeColour);
            base.VisitTypeParameter(node);
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
            _highlightAction(node.Identifier.Span, _palette.MethodColour);
            HighlightExpressionSyntax(node.ReturnType);
            HighlightModifiers(node.Modifiers);
            base.VisitMethodDeclaration(node);
        }

        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            HighlightModifiers(node.Modifiers);
            HighlightExpressionSyntax(node.Declaration.Type);
            base.VisitFieldDeclaration(node);
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            HighlightModifiers(node.Modifiers);
            HighlightExpressionSyntax(node.Type);
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

        public override void VisitOperatorDeclaration(OperatorDeclarationSyntax node)
        {
            HighlightModifiers(node.Modifiers);
            _highlightAction(node.OperatorKeyword.Span, _palette.BlueKeywordColour);
            base.VisitOperatorDeclaration(node);
        }

        public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            HighlightModifiers(node.Modifiers);
            foreach (VariableDeclaratorSyntax identifier in node.Declaration.Variables)
            {
                HighlightExpressionSyntax(node.Declaration.Type);
                if (!identifier.Identifier.IsMissing)
                {
                    _highlightAction(identifier.Identifier.Span, _palette.LocalVariableColour);
                }
            }
            base.VisitLocalDeclarationStatement(node);
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

        public override void VisitUsingStatement(UsingStatementSyntax node)
        {
            _highlightAction(node.UsingKeyword.Span, _palette.BlueKeywordColour);
            base.VisitUsingStatement(node);
        }

        public override void VisitBreakStatement(BreakStatementSyntax node)
        {
            base.VisitBreakStatement(node);
            _highlightAction(node.BreakKeyword.Span, _palette.BlueKeywordColour);
        }

        public override void VisitContinueStatement(ContinueStatementSyntax node)
        {
            base.VisitContinueStatement(node);
            _highlightAction(node.ContinueKeyword.Span, _palette.BlueKeywordColour);
        }

        public override void VisitCheckedStatement(CheckedStatementSyntax node)
        {
            base.VisitCheckedStatement(node);
            _highlightAction(node.Keyword.Span, _palette.BlueKeywordColour);
        }

        public override void VisitFixedStatement(FixedStatementSyntax node)
        {
            base.VisitFixedStatement(node);
            _highlightAction(node.FixedKeyword.Span, _palette.BlueKeywordColour);
        }

        public override void VisitLockStatement(LockStatementSyntax node)
        {
            base.VisitLockStatement(node);
            _highlightAction(node.LockKeyword.Span, _palette.BlueKeywordColour);
        }

        public override void VisitUnsafeStatement(UnsafeStatementSyntax node)
        {
            base.VisitUnsafeStatement(node);
            _highlightAction(node.UnsafeKeyword.Span, _palette.BlueKeywordColour);
        }

        public override void VisitTryStatement(TryStatementSyntax node)
        {
            _highlightAction(node.TryKeyword.Span, _palette.PurpleKeywordColour);
            base.VisitTryStatement(node);
        }

        public override void VisitThrowStatement(ThrowStatementSyntax node)
        {
            _highlightAction(node.ThrowKeyword.Span, _palette.PurpleKeywordColour);
            base.VisitThrowStatement(node);
        }

        public override void VisitYieldStatement(YieldStatementSyntax node)
        {
            _highlightAction(node.YieldKeyword.Span, _palette.PurpleKeywordColour);
            _highlightAction(node.ReturnOrBreakKeyword.Span, _palette.PurpleKeywordColour);
            base.VisitYieldStatement(node);
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
            HighlightExpressionSyntax(node.Type);
            base.VisitObjectCreationExpression(node);
        }

        public override void VisitInterpolatedStringExpression(InterpolatedStringExpressionSyntax node)
        {
            base.VisitInterpolatedStringExpression(node);
            _highlightAction(node.StringStartToken.Span, _palette.StringLiteralColour);
            _highlightAction(node.StringEndToken.Span, _palette.StringLiteralColour);
        }

        public override void VisitBlock(BlockSyntax node)
        {
            _addBlockAction(node.FullSpan);
            base.VisitBlock(node);
        }

        public override void VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            base.VisitLiteralExpression(node);
            switch (node.Kind())
            {
                case SyntaxKind.StringLiteralExpression:
                case SyntaxKind.CharacterLiteralExpression:
                    _highlightAction(node.Span, _palette.StringLiteralColour);
                    break;
                case SyntaxKind.TrueLiteralExpression:
                case SyntaxKind.FalseLiteralExpression:
                case SyntaxKind.NullLiteralExpression:
                case SyntaxKind.DefaultLiteralExpression:
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

        public override void VisitTypeOfExpression(TypeOfExpressionSyntax node)
        {
            _highlightAction(node.Keyword.Span, _palette.BlueKeywordColour);
            base.VisitTypeOfExpression(node);
        }

        public override void VisitSizeOfExpression(SizeOfExpressionSyntax node)
        {
            _highlightAction(node.Keyword.Span, _palette.BlueKeywordColour);
            base.VisitSizeOfExpression(node);
        }
        #endregion

        #region Clauses
        public override void VisitTypeParameterConstraintClause(TypeParameterConstraintClauseSyntax node)
        {
            _highlightAction(node.WhereKeyword.Span, _palette.BlueKeywordColour);
            base.VisitTypeParameterConstraintClause(node);
        }

        public override void VisitElseClause(ElseClauseSyntax node)
        {
            _highlightAction(node.ElseKeyword.Span, _palette.PurpleKeywordColour);
            base.VisitElseClause(node);
        }

        public override void VisitCatchClause(CatchClauseSyntax node)
        {
            _highlightAction(node.CatchKeyword.Span, _palette.PurpleKeywordColour);
            if (node.Declaration != null)
            {
                _highlightAction(node.Declaration.Identifier.Span, _palette.LocalVariableColour);
            }
            base.VisitCatchClause(node);
        }

        public override void VisitFinallyClause(FinallyClauseSyntax node)
        {
            _highlightAction(node.FinallyKeyword.Span, _palette.PurpleKeywordColour);
            base.VisitFinallyClause(node);
        }
        #endregion

        public override void VisitConstructorConstraint(ConstructorConstraintSyntax node)
        {
            _highlightAction(node.NewKeyword.Span, _palette.BlueKeywordColour);
            base.VisitConstructorConstraint(node);
        }

        public override void VisitClassOrStructConstraint(ClassOrStructConstraintSyntax node)
        {
            _highlightAction(node.ClassOrStructKeyword.Span, _palette.BlueKeywordColour);
            base.VisitClassOrStructConstraint(node);
        }

        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            _highlightAction(node.UsingKeyword.Span, _palette.BlueKeywordColour);
            base.VisitUsingDirective(node);
        }

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
            //HighlightExpressionSyntax(node.Name, true);
            //_highlightAction(node.Name.Span, Color.SteelBlue);
        }

        public override void VisitInterpolatedStringText(InterpolatedStringTextSyntax node)
        {
            base.VisitInterpolatedStringText(node);
            _highlightAction(node.Span, _palette.StringLiteralColour);
        }

        public override void VisitParameter(ParameterSyntax node)
        {
            HighlightModifiers(node.Modifiers);
            if (node.Type != null)
            {
                //HighlightTypeSyntax(node.Type);
                HighlightExpressionSyntax(node.Type);
            }
            _highlightAction(node.Identifier.Span, _palette.LocalVariableColour);
            base.VisitParameter(node);
        }

        public override void VisitBaseList(BaseListSyntax node)
        {
            foreach (BaseTypeSyntax type in node.Types)
            {
                HighlightExpressionSyntax(type.Type);
            }
            base.VisitBaseList(node);
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            HighlightExpressionSyntax(node);
            base.VisitIdentifierName(node);
        }

        private void HighlightExpressionSyntax(ExpressionSyntax node, bool isAttribute = false)
        {
            ISymbol symbol = _semanticModel.GetSymbolInfo(node).Symbol;
            string identifierText = (node as IdentifierNameSyntax)?.Identifier.Text;
            if (symbol != null)
            {
                if (symbol is ITypeSymbol typeSymbol)
                {
                    if (identifierText == "var")
                    {
                        _highlightAction(node.Span, _palette.BlueKeywordColour);
                    }
                    else if (node is TypeSyntax t)
                    {
                        HighlightTypeSyntax(t);
                    }
                }
                else if (symbol is IMethodSymbol methodSymbol)
                {
                    _highlightAction(node.Span, isAttribute ? _palette.TypeColour : _palette.MethodColour);
                }
                else if (symbol is IParameterSymbol || symbol is ILocalSymbol)
                {
                    _highlightAction(node.Span, _palette.LocalVariableColour);
                }
            }
            else
            {
                if (identifierText == "nameof")
                {
                    _highlightAction(node.Span, _palette.BlueKeywordColour);
                }
            }
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
                    HighlightExpressionSyntax(typeArgument);
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
            else if (typeSyntax is QualifiedNameSyntax qualifiedNameSyntax)
            {
                HighlightTypeSyntax(qualifiedNameSyntax.Right);
            }
            else if (typeSyntax is PredefinedTypeSyntax p)
            {
                //_highlightAction(p.Span, _palette.BlueKeywordColour);
            }
        }

    }
}
