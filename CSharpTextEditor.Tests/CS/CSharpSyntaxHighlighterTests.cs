using CSharpTextEditor.CS;
using CSharpTextEditor.UndoRedoActions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor.Tests.CS
{
    [TestFixture]
    internal class CSharpSyntaxHighlighterTests
    {
        internal class SpansTestCase
        {
            public SpansTestCase(string text, int position, (int, int)[] expectedSpans)
            {
                Text = text;
                Position = position;
                ExpectedSpans = expectedSpans;
            }

            public string Text { get; set; }
            public int Position { get; set; }
            public (int, int)[] ExpectedSpans { get; set; }

            public override string ToString()
            {
                return $"Position {Position}";
            }
        }

        [TestCaseSource(nameof(GetBeforePositionTestCases))]
        public void GetSymbolSpansBeforePosition(SpansTestCase testCase)
        {
            string testString = testCase.Text;
            SourceCode code = new SourceCode(testString, new HistoryManager());
            CSharpSyntaxHighlighter highlighter = new CSharpSyntaxHighlighter();
            highlighter.GetHighlightings(code.Lines, new SyntaxPalette());
            (int start, int end)[] spans = highlighter.GetSymbolSpansBeforePosition(testCase.Position).ToArray();

            Assert.That(spans, Is.EqualTo(testCase.ExpectedSpans));
        }

        [TestCaseSource(nameof(GetAfterPositionTestCases))]
        public void GetSymbolSpansAfterPosition(SpansTestCase testCase)
        {
            string testString = testCase.Text;
            SourceCode code = new SourceCode(testString, new HistoryManager());
            CSharpSyntaxHighlighter highlighter = new CSharpSyntaxHighlighter();
            highlighter.GetHighlightings(code.Lines, new SyntaxPalette());
            (int start, int end)[] spans = highlighter.GetSymbolSpansAfterPosition(testCase.Position).ToArray();

            Assert.That(spans, Is.EqualTo(testCase.ExpectedSpans));
        }

        private static IEnumerable<SpansTestCase> GetAfterPositionTestCases() => GetTestCases(false);

        private static IEnumerable<SpansTestCase> GetBeforePositionTestCases() => GetTestCases(true);

        private static IEnumerable<SpansTestCase> GetTestCases(bool reverse)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("//leading comment");
            sb.AppendLine("class C // trailing comment");
            sb.AppendLine("{");
            sb.AppendLine("}");
            string testString = sb.ToString();

            // assume two characters per new line
            (int, int)[] expectedSpans = new[]
            {
                (0, 9),   // "//leading"
                (10, 17), // "comment"
                (19, 24), // "class"
                (25, 26), // "C"
                (27, 29), // "//"
                (30, 38), // "trailing"
                (39, 46), // "comment"
                (48, 49), // "{"
                (51, 52), // "}"
            };

            int index = 0;
            foreach ((int, int) span in expectedSpans)
            {
                (int, int)[] thisExpectedSpans;
                if (reverse)
                {
                    thisExpectedSpans = expectedSpans.Take(index + 1).Reverse().ToArray();
                }
                else
                {
                    thisExpectedSpans = expectedSpans.Skip(index).ToArray();
                }
                yield return new SpansTestCase(testString, reverse ? span.Item2 : span.Item1, thisExpectedSpans);
                index++;
            }
        }
    }
}
