using CSharpTextEditor.Source;
using CSharpTextEditor.View;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpTextEditor.Languages
{
    public class SyntaxHighlightingCollection
    {
        public IReadOnlyCollection<SyntaxHighlighting> Highlightings { get; }

        public IReadOnlyCollection<SyntaxDiagnostic> Diagnostics { get; }

        public IReadOnlyCollection<(int, int)> BlockLines { get; }

        public SyntaxHighlightingCollection(IReadOnlyCollection<SyntaxHighlighting> highlightings, IReadOnlyCollection<SyntaxDiagnostic> diagnostics, IReadOnlyCollection<(int, int)> blockLines)
        {
            Highlightings = highlightings;
            Diagnostics = diagnostics;
            BlockLines = blockLines;
        }

        internal string GetErrorMessagesAtPosition(SourceCodePosition position, SourceCode sourceCode)
        {
            StringBuilder sb = new StringBuilder();
            foreach (SyntaxDiagnostic diagnostic in Diagnostics)
            {
                var start = diagnostic.Start;
                var end = diagnostic.End;

                int startColumn = start.ColumnNumber;
                for (int lineIndex = start.LineNumber; lineIndex <= end.LineNumber; lineIndex++)
                {
                    int endColumn = lineIndex == end.LineNumber ? end.ColumnNumber : sourceCode.Lines.ElementAt(lineIndex).Length;
                    if (lineIndex == position.LineNumber
                        && position.ColumnNumber >= startColumn
                        && position.ColumnNumber <= endColumn)
                    {
                        sb.AppendLine(diagnostic.ToFullString()).AppendLine();
                    }
                    startColumn = 0;
                }
            }
            return sb.ToString();
        }
    }
}