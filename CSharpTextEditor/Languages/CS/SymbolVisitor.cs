using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* Unmerged change from project 'CSharpTextEditor (net8.0-windows)'
Added:
using CSharpTextEditor;
using CSharpTextEditor.CS;
using CSharpTextEditor.Languages.CS;
*/

namespace CSharpTextEditor.Languages.CS
{
    internal class SymbolVisitor : CSharpSyntaxWalker
    {
        private readonly string _symbolName;
        private readonly SemanticModel _semanticModel;

        public ISymbol? FoundSymbol { get; private set; }

        private SymbolVisitor(string symbolName, SemanticModel semanticModel)
        {
            _symbolName = symbolName;
            _semanticModel = semanticModel;
        }

        public static ISymbol? FindSymbolWithName(string symbolName, SemanticModel semanticModel)
        {
            SymbolVisitor visitor = new SymbolVisitor(symbolName, semanticModel);
            visitor.Visit(semanticModel.SyntaxTree.GetRoot());
            return visitor.FoundSymbol;
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            if (FoundSymbol != null)
            {
                return;
            }
            if (_symbolName == node.Identifier.Text)
            {
                var symbolInfo = _semanticModel.GetSymbolInfo(node);
                if (symbolInfo.Symbol != null)
                {
                    FoundSymbol = _semanticModel.GetSymbolInfo(node).Symbol;
                }
                else if (symbolInfo.CandidateSymbols.Length == 1)
                {
                    FoundSymbol = symbolInfo.CandidateSymbols[0];
                }
            }
            else
            {
                base.VisitIdentifierName(node);
            }
        }
    }
}
