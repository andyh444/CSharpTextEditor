using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;

namespace CSharpTextEditor
{
    public interface ISyntaxHighlighter
    {
        IReadOnlyCollection<SyntaxHighlighting> GetHighlightings(string sourceText, SyntaxPalette palette);
    }

    public class CSharpSyntaxHighlighter : ISyntaxHighlighter
    {
        // TODO: Refactor to remove need for _sourceCode
        private SourceCode _sourceCode;

        internal CSharpSyntaxHighlighter(SourceCode sourceCode)
        {
            _sourceCode = sourceCode;
        }

        public IReadOnlyCollection<SyntaxHighlighting> GetHighlightings(string sourceText, SyntaxPalette palette)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(sourceText);
            List<SyntaxHighlighting> highlighting = new List<SyntaxHighlighting>();
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
                    AddSpanToHighlighting(token.Span, Color.DarkRed, highlighting);
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
                    AddSpanToHighlighting(trivium.Span, Color.Green, highlighting);
                }
            }
            CSharpSyntaxHighlightingWalker highlighter = new CSharpSyntaxHighlightingWalker((span, action) => AddSpanToHighlighting(span, action, highlighting), palette);
            highlighter.Visit(tree.GetRoot());
            return highlighting.OrderBy(x => x.Line).ThenBy(x => x.StartColumn).ToList();
        }

        private void AddSpanToHighlighting(TextSpan span, Color colour, List<SyntaxHighlighting> highlighting)
        {
            int characterCount = 0;
            int lineIndex = 0;
            // in theory, each piece of highlighting should always only be on one line. In theory??
            int foundLine = -1;
            int foundStartColumn = -1;
            int foundEndColumn = -1;
            foreach (string line in _sourceCode.Lines)
            {

                if (characterCount + line.Length > span.Start
                    && foundLine == -1)
                {
                    foundLine = lineIndex;
                    foundStartColumn = span.Start - characterCount;
                    foundEndColumn = span.End - characterCount;
                    break;
                }
                characterCount += line.Length + Environment.NewLine.Length;
                lineIndex++;
            }
            highlighting.Add(new SyntaxHighlighting(foundLine, foundStartColumn, foundEndColumn, colour));
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
