using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTextEditor.Languages.CSharp
{
    internal class SymbolVisitor : CSharpSyntaxWalker
    {
        private readonly string _symbolName;
        private readonly SemanticModel _semanticModel;
        private readonly List<ISymbol> _foundSymbols;

        private SymbolVisitor(string symbolName, SemanticModel semanticModel)
        {
            _symbolName = symbolName;
            _semanticModel = semanticModel;
            _foundSymbols = new List<ISymbol>();
        }

        public static IReadOnlyList<ISymbol> FindSymbolsWithName(string symbolName, SemanticModel semanticModel)
        {
            SymbolVisitor visitor = new SymbolVisitor(symbolName, semanticModel);
            visitor.Visit(semanticModel.SyntaxTree.GetRoot());
            return visitor._foundSymbols;
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            if (_symbolName == node.Identifier.Text)
            {
                var symbolInfo = _semanticModel.GetSymbolInfo(node);
                if (symbolInfo.Symbol != null)
                {
                    ISymbol? symbol = _semanticModel.GetSymbolInfo(node).Symbol;
                    if (symbol != null)
                    {
                        _foundSymbols.Add(symbol);
                    }
                }
                else if (symbolInfo.CandidateSymbols.Any())
                {
                    _foundSymbols.AddRange(symbolInfo.CandidateSymbols);
                }
            }
            base.VisitIdentifierName(node);
        }
    }
}
