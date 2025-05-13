using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NTextEditor.View;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System.Xml.Linq;

namespace NTextEditor.Languages.VisualBasic
{
    internal class VisualBasicSyntaxHighlightingWalker : VisualBasicSyntaxWalker
    {
        private readonly SyntaxPalette _palette;
        private readonly Action<TextSpan, Color> _highlightAction;
        private readonly Action<TextSpan> _addBlockAction;
        private readonly SemanticModel _semanticModel;

        public VisualBasicSyntaxHighlightingWalker(SemanticModel semanticModel, Action<TextSpan, Color> highlightAction, Action<TextSpan> addBlockAction, SyntaxPalette palette)
        {
            _semanticModel = semanticModel;
            _highlightAction = highlightAction;
            _addBlockAction = addBlockAction;
            _palette = palette;
        }

        public override void VisitImportsStatement(ImportsStatementSyntax node)
        {
            base.VisitImportsStatement(node);
            _highlightAction(node.ImportsKeyword.Span, _palette.BlueKeywordColour);
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
                    _highlightAction(node.Span, _palette.BlueKeywordColour);
                    break;
                case SyntaxKind.NumericLiteralExpression:
                    _highlightAction(node.Span, _palette.NumericLiteralColour);
                    break;
            }
        }

        public override void VisitDelegateStatement(DelegateStatementSyntax node)
        {
            base.VisitDelegateStatement(node);
            HighlightModifiers(node.Modifiers);
            _highlightAction(node.DelegateKeyword.Span, _palette.BlueKeywordColour);
            _highlightAction(node.SubOrFunctionKeyword.Span, _palette.BlueKeywordColour);
            _highlightAction(node.Identifier.Span, _palette.TypeColour);
        }

        public override void VisitPropertyStatement(PropertyStatementSyntax node)
        {
            base.VisitPropertyStatement(node);
            

            HighlightModifiers(node.Modifiers);
            _highlightAction(node.PropertyKeyword.Span, _palette.BlueKeywordColour);
        }

        public override void VisitPredefinedType(PredefinedTypeSyntax node)
        {
            base.VisitPredefinedType(node);
            _highlightAction(node.Span, _palette.BlueKeywordColour);
        }

        public override void VisitTryBlock(TryBlockSyntax node)
        {
            base.VisitTryBlock(node);
            _highlightAction(node.TryStatement.TryKeyword.Span, _palette.PurpleKeywordColour);
            foreach (CatchBlockSyntax catchBlock in node.CatchBlocks)
            {
                _highlightAction(catchBlock.CatchStatement.CatchKeyword.Span, _palette.PurpleKeywordColour);
            }
            _highlightAction(node.FinallyBlock.FinallyStatement.FinallyKeyword.Span, _palette.PurpleKeywordColour);
            HighlightEndBlock(node.EndTryStatement, _palette.PurpleKeywordColour);
        }

        public override void VisitEnumBlock(EnumBlockSyntax node)
        {
            base.VisitEnumBlock(node);
            HighlightModifiers(node.EnumStatement.Modifiers);
            _highlightAction(node.EnumStatement.EnumKeyword.Span, _palette.BlueKeywordColour);
            _highlightAction(node.EnumStatement.Identifier.Span, _palette.TypeColour);
            HighlightEndBlock(node.EndEnumStatement, _palette.BlueKeywordColour);
        }

        public override void VisitClassBlock(ClassBlockSyntax node)
        {
            base.VisitClassBlock(node);
            HighlightTypeStatement(node.ClassStatement);
            HighlightEndBlock(node.EndClassStatement, _palette.BlueKeywordColour);
        }

        public override void VisitStructureBlock(StructureBlockSyntax node)
        {
            base.VisitStructureBlock(node);
            HighlightTypeStatement(node.StructureStatement);
            HighlightEndBlock(node.EndStructureStatement, _palette.BlueKeywordColour);
        }

        public override void VisitInterfaceBlock(InterfaceBlockSyntax node)
        {
            base.VisitInterfaceBlock(node);
            HighlightTypeStatement(node.InterfaceStatement);
            HighlightEndBlock(node.EndInterfaceStatement, _palette.BlueKeywordColour);
        }

        private void HighlightTypeStatement(TypeStatementSyntax node)
        {
            HighlightModifiers(node.Modifiers);
            _highlightAction(node.DeclarationKeyword.Span, _palette.BlueKeywordColour);
            _highlightAction(node.Identifier.Span, _palette.TypeColour);
        }

        public override void VisitModuleBlock(ModuleBlockSyntax node)
        {
            base.VisitModuleBlock(node);
            HighlightTypeStatement(node.ModuleStatement);
            HighlightEndBlock(node.EndModuleStatement, _palette.BlueKeywordColour);
        }

        public override void VisitMethodBlock(MethodBlockSyntax node)
        {
            base.VisitMethodBlock(node);
            HighlightModifiers(node.BlockStatement.Modifiers);
            _highlightAction(node.BlockStatement.DeclarationKeyword.Span, _palette.BlueKeywordColour);
            _highlightAction(node.SubOrFunctionStatement.Identifier.Span, _palette.MethodColour);
            HighlightEndBlock(node.EndBlockStatement, _palette.BlueKeywordColour);
        }

        public override void VisitNamespaceBlock(NamespaceBlockSyntax node)
        {
            base.VisitNamespaceBlock(node);
            _highlightAction(node.NamespaceStatement.NamespaceKeyword.Span, _palette.BlueKeywordColour);
            HighlightEndBlock(node.EndNamespaceStatement, _palette.BlueKeywordColour);
        }

        public override void VisitForEachBlock(ForEachBlockSyntax node)
        {
            base.VisitForEachBlock(node);
            _highlightAction(node.ForEachStatement.ForKeyword.Span, _palette.PurpleKeywordColour);
            _highlightAction(node.ForEachStatement.EachKeyword.Span, _palette.PurpleKeywordColour);
            _highlightAction(node.ForEachStatement.InKeyword.Span, _palette.PurpleKeywordColour);
            _highlightAction(node.NextStatement.NextKeyword.Span, _palette.PurpleKeywordColour);
        }

        public override void VisitElseIfBlock(ElseIfBlockSyntax node)
        {
            base.VisitElseIfBlock(node);
            _highlightAction(node.ElseIfStatement.ElseIfKeyword.Span, _palette.PurpleKeywordColour);
            _highlightAction(node.ElseIfStatement.ThenKeyword.Span, _palette.PurpleKeywordColour);
        }

        public override void VisitSelectBlock(SelectBlockSyntax node)
        {
            base.VisitSelectBlock(node);
            _highlightAction(node.SelectStatement.SelectKeyword.Span, _palette.PurpleKeywordColour);
            _highlightAction(node.SelectStatement.CaseKeyword.Span, _palette.PurpleKeywordColour);
            foreach (var caseBlock in node.CaseBlocks)
            {
                _highlightAction(caseBlock.CaseStatement.CaseKeyword.Span, _palette.PurpleKeywordColour);
            }
            _highlightAction(node.EndSelectStatement.EndKeyword.Span, _palette.PurpleKeywordColour);
            _highlightAction(node.EndSelectStatement.BlockKeyword.Span, _palette.PurpleKeywordColour);
        }

        public override void VisitDoLoopBlock(DoLoopBlockSyntax node)
        {
            base.VisitDoLoopBlock(node);
            _highlightAction(node.DoStatement.DoKeyword.Span, _palette.PurpleKeywordColour);
            _highlightAction(node.DoStatement.WhileOrUntilClause.WhileOrUntilKeyword.Span, _palette.PurpleKeywordColour);
            _highlightAction(node.LoopStatement.LoopKeyword.Span, _palette.PurpleKeywordColour);
        }

        public override void VisitInheritsStatement(InheritsStatementSyntax node)
        {
            base.VisitInheritsStatement(node);
            _highlightAction(node.InheritsKeyword.Span, _palette.BlueKeywordColour);
            foreach (TypeSyntax t in node.Types)
            {
                HighlightExpressionSyntax(t);
            }
        }

        public override void VisitImplementsStatement(ImplementsStatementSyntax node)
        {
            base.VisitImplementsStatement(node);
            _highlightAction(node.ImplementsKeyword.Span, _palette.BlueKeywordColour);
            foreach (TypeSyntax t in node.Types)
            {
                HighlightExpressionSyntax(t);
            }
        }

        public override void VisitSimpleAsClause(SimpleAsClauseSyntax node)
        {
            base.VisitSimpleAsClause(node);
            _highlightAction(node.AsKeyword.Span, _palette.BlueKeywordColour);
            HighlightExpressionSyntax(node.Type());
        }

        public override void VisitAsNewClause(AsNewClauseSyntax node)
        {
            base.VisitAsNewClause(node);
            _highlightAction(node.AsKeyword.Span, _palette.BlueKeywordColour);
            _highlightAction(node.NewExpression.NewKeyword.Span, _palette.BlueKeywordColour);
        }

        public override void VisitWithStatement(WithStatementSyntax node)
        {
            base.VisitWithStatement(node);
            _highlightAction(node.WithKeyword.Span, _palette.BlueKeywordColour);
        }

        public override void VisitWithBlock(WithBlockSyntax node)
        {
            base.VisitWithBlock(node);
        }

        public override void VisitTypeArgumentList(TypeArgumentListSyntax node)
        {
            base.VisitTypeArgumentList(node);
            _highlightAction(node.OfKeyword.Span, _palette.BlueKeywordColour);
        }

        public override void VisitMultiLineIfBlock(MultiLineIfBlockSyntax node)
        {
            base.VisitMultiLineIfBlock(node);
            _highlightAction(node.IfStatement.IfKeyword.Span, _palette.PurpleKeywordColour);
            _highlightAction(node.IfStatement.ThenKeyword.Span, _palette.PurpleKeywordColour);
            HighlightEndBlock(node.EndIfStatement, _palette.PurpleKeywordColour);
        }

        public override void VisitAttributeList(AttributeListSyntax node)
        {
            base.VisitAttributeList(node);
            foreach (var attribute in node.Attributes)
            {
                HighlightExpressionSyntax(attribute.Name, true);
                //_highlightAction(attribute.Span, _palette.BlueKeywordColour);
                //HighlightExpressionSyntax(attribute);
            }
        }

        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            base.VisitObjectCreationExpression(node);
            _highlightAction(node.NewKeyword.Span, _palette.BlueKeywordColour);
        }

        public override void VisitThrowStatement(ThrowStatementSyntax node)
        {
            base.VisitThrowStatement(node);
            _highlightAction(node.ThrowKeyword.Span, _palette.PurpleKeywordColour);

        }

        public override void VisitMeExpression(MeExpressionSyntax node)
        {
            base.VisitMeExpression(node);
            _highlightAction(node.Keyword.Span, _palette.BlueKeywordColour);
        }

        public override void VisitRaiseEventStatement(RaiseEventStatementSyntax node)
        {
            base.VisitRaiseEventStatement(node);
            _highlightAction(node.RaiseEventKeyword.Span, _palette.BlueKeywordColour);
        }

        public override void VisitAddRemoveHandlerStatement(AddRemoveHandlerStatementSyntax node)
        {
            base.VisitAddRemoveHandlerStatement(node);
            _highlightAction(node.AddHandlerOrRemoveHandlerKeyword.Span, _palette.BlueKeywordColour);
            if (node.DelegateExpression is UnaryExpressionSyntax ues
                && ues.OperatorToken.IsKind(SyntaxKind.AddressOfKeyword))
            {
                _highlightAction(ues.OperatorToken.Span, _palette.BlueKeywordColour);
            }
        }

        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            base.VisitFieldDeclaration(node);
            HighlightModifiers(node.Modifiers);
        }

        public override void VisitEventStatement(EventStatementSyntax node)
        {
            base.VisitEventStatement(node);
            HighlightModifiers(node.Modifiers);
            _highlightAction(node.EventKeyword.Span, _palette.BlueKeywordColour);
        }

        public override void VisitInterpolatedStringExpression(InterpolatedStringExpressionSyntax node)
        {
            base.VisitInterpolatedStringExpression(node);
            _highlightAction(node.DollarSignDoubleQuoteToken.Span, _palette.StringLiteralColour);
            _highlightAction(node.DoubleQuoteToken.Span, _palette.StringLiteralColour);
        }

        public override void VisitInterpolatedStringText(InterpolatedStringTextSyntax node)
        {
            base.VisitInterpolatedStringText(node);
            _highlightAction(node.Span, _palette.StringLiteralColour);
        }

        public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            base.VisitLocalDeclarationStatement(node);
            HighlightModifiers(node.Modifiers);
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            base.VisitIdentifierName(node);
            HighlightExpressionSyntax(node);
        }

        public override void VisitImplementsClause(ImplementsClauseSyntax node)
        {
            base.VisitImplementsClause(node);
            _highlightAction(node.ImplementsKeyword.Span, _palette.BlueKeywordColour);
        }

        private void HighlightModifiers(SyntaxTokenList modifiers)
        {
            foreach (var modifier in modifiers)
            {
                _highlightAction(modifier.Span, _palette.BlueKeywordColour);
            }
        }

        private void HighlightEndBlock(EndBlockStatementSyntax endBlock, Color colour)
        {
            _highlightAction(endBlock.EndKeyword.Span, colour);
            _highlightAction(endBlock.BlockKeyword.Span, colour);
        }

        private void HighlightExpressionSyntax(ExpressionSyntax node, bool isAttribute = false)
        {
            // copied from CSharp implementation TODO Update
            ISymbol? symbol = _semanticModel.GetSymbolInfo(node).Symbol;
            string? identifierText = (node as IdentifierNameSyntax)?.Identifier.Text;
            if (symbol != null)
            {
                if (symbol is IArrayTypeSymbol arrayTypeSymbol)
                {
                    // ignore this case - the element type will be picked up later
                }
                else if (symbol is ITypeSymbol typeSymbol)
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
                    if (identifierText == "value")
                    {
                        _highlightAction(node.Span, _palette.BlueKeywordColour);
                    }
                    else
                    {
                        _highlightAction(node.Span, _palette.LocalVariableColour);
                    }
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
                HighlightExpressionSyntax(nullableTypeSyntax.ElementType);
            }
            else if (typeSyntax is ArrayTypeSyntax arrayTypeSyntax)
            {
                HighlightExpressionSyntax(arrayTypeSyntax.ElementType);
            }
            else if (typeSyntax is TupleTypeSyntax tupleTypeSyntax)
            {
                foreach (TupleElementSyntax element in tupleTypeSyntax.Elements)
                {
                    //HighlightExpressionSyntax(element.Type);
                }
            }
            else if (typeSyntax is QualifiedNameSyntax qualifiedNameSyntax)
            {
                HighlightExpressionSyntax(qualifiedNameSyntax.Right);
            }
            else if (typeSyntax is PredefinedTypeSyntax p)
            {
                //_highlightAction(p.Span, _palette.BlueKeywordColour);
            }
        }
    }
}
