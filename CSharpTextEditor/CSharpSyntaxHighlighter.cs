﻿using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace CSharpTextEditor
{
    public class CSharpSyntaxHighlighter : ISyntaxHighlighter
    {
        // TODO: Refactor to remove need for _sourceCode
        private Func<int, (int, int)> _getLineAndColumnNumber;

        internal CSharpSyntaxHighlighter(Func<int, (int, int)> getLineAndColumnNumber)
        {
            _getLineAndColumnNumber = getLineAndColumnNumber;
        }

        public SyntaxHighlightingCollection GetHighlightings(string sourceText, SyntaxPalette palette)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(sourceText);
            List<SyntaxHighlighting> highlighting = new List<SyntaxHighlighting>();
            List<(int line, int startColumn, int endColumn, string message)> errors = new List<(int line, int startColumn, int endColumn, string message)>();
            foreach (var token in tree.GetRoot().DescendantTokens())
            {
                if (token.IsKeyword())
                {
                    AddSpanToHighlighting(token.Span, GetKeywordColour(token.Kind(), palette), highlighting);
                }
                if (token.IsKind(SyntaxKind.StringLiteralToken)
                    || token.IsKind(SyntaxKind.InterpolatedStringStartToken)
                    || token.IsKind(SyntaxKind.InterpolatedStringTextToken)
                    || token.IsKind(SyntaxKind.InterpolatedStringEndToken))
                {
                    AddSpanToHighlighting(token.Span, palette.StringLiteralColour, highlighting);
                }
                if (token.IsVerbatimIdentifier())
                {
                    Debugger.Break();
                }
            }
            foreach (var trivium in tree.GetRoot().DescendantTrivia())
            {
                if (trivium.IsKind(SyntaxKind.SingleLineCommentTrivia)
                    || trivium.IsKind(SyntaxKind.MultiLineCommentTrivia))
                {
                    //HighlightSyntax(trivium.Span, Color.Green);
                    AddSpanToHighlighting(trivium.Span, palette.CommentColour, highlighting);
                }
            }
            foreach (var diagnostic in tree.GetDiagnostics())
            {
                if (diagnostic.Severity == DiagnosticSeverity.Error)
                {
                    (int startLine, int startColumn) = _getLineAndColumnNumber(diagnostic.Location.SourceSpan.Start);
                    (int endLine, int endColumn) = _getLineAndColumnNumber(diagnostic.Location.SourceSpan.End);
                    if (startLine != endLine)
                    {
                        Debugger.Break();
                    }
                    errors.Add((startLine, startColumn, endColumn, $"{diagnostic.Id}: {diagnostic.GetMessage()}"));
                }
            }
            CSharpSyntaxHighlightingWalker highlighter = new CSharpSyntaxHighlightingWalker((span, action) => AddSpanToHighlighting(span, action, highlighting), palette);
            highlighter.Visit(tree.GetRoot());
            return new SyntaxHighlightingCollection(highlighting.OrderBy(x => x.Line).ThenBy(x => x.StartColumn).ToList(), errors);
        }

        private void AddSpanToHighlighting(TextSpan span, Color colour, List<SyntaxHighlighting> highlighting)
        {
            (int startLine, int startColumn) = _getLineAndColumnNumber(span.Start);
            (int endLine, int endColumn) = _getLineAndColumnNumber(span.End);
            if (startLine != endLine)
            {
                throw new Exception("Cannot handle multi-line syntax highlighting");
            }
            highlighting.Add(new SyntaxHighlighting(startLine, startColumn, endColumn, colour));
        }

        private Color GetKeywordColour(SyntaxKind syntaxKind, SyntaxPalette palette)
        {
            if (syntaxKind == SyntaxKind.IfKeyword
                || syntaxKind == SyntaxKind.ElseKeyword
                || syntaxKind == SyntaxKind.ForEachKeyword
                || syntaxKind == SyntaxKind.ForKeyword
                || syntaxKind == SyntaxKind.WhileKeyword
                || syntaxKind == SyntaxKind.DoKeyword
                || syntaxKind == SyntaxKind.ReturnKeyword
                || syntaxKind == SyntaxKind.TryKeyword
                || syntaxKind == SyntaxKind.CatchKeyword
                || syntaxKind == SyntaxKind.FinallyKeyword
                || syntaxKind == SyntaxKind.SwitchKeyword
                || syntaxKind == SyntaxKind.CaseKeyword
                || syntaxKind == SyntaxKind.BreakKeyword)
            {
                return palette.PurpleKeywordColour;
            }
            return palette.BlueKeywordColour;
        }
    }
}