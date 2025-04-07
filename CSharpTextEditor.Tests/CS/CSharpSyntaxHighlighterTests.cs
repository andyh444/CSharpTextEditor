using CSharpTextEditor.Languages.CS;
using CSharpTextEditor.Source;
using CSharpTextEditor.UndoRedoActions;
using CSharpTextEditor.View;
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

        string suggestionsTestClass =
@"namespace Suggestions.Test
{
    class TestClass
    {
        public int TestProperty { get; }

        public TestClass() {}

        public void TestMethod(int a) {}
        public void TestMethod(int a, int b) {}
        public static void TestMethod() {}
    }

    class TestClass2 {}
}
";

        private static IEnumerable<TestCase<(string text, IReadOnlyCollection<string> expectedSuggestions, int? position)>> GetSuggestionTestCases()
        {
            yield return new(("Suggestions.Test.TestClass2 tc;",
                ["namespace Suggestions.Test"],
                12),
                "mid-string caret");

            yield return new(("Suggestions.Test.TestClass2 tc;",
                ["class Suggestions.Test.TestClass", "class Suggestions.Test.TestClass2"],
                17),
                "mid-string caret 2");

            yield return new(("\\\\Hello.",
                [],
                null),
                "don't get suggestions for a . added in a comment");

            yield return new(("Suggestions.",
                ["namespace Suggestions.Test"],
                null),
                "namespaces/namespace members");

            yield return new(("Suggestions.Test.",
                ["class Suggestions.Test.TestClass", "class Suggestions.Test.TestClass2"],
                null),
                "namespaces/namespace members 2");

            yield return new(("Suggestions.Test.TestClass.",
                ["bool object.Equals(object? objA, object? objB)", "bool object.ReferenceEquals(object? objA, object? objB)", "void TestClass.TestMethod()"],
                null),
                "static class members");

            yield return new(("Suggestions.Test.TestClass.TestMethod(",
                ["void TestClass.TestMethod()"],
                null),
                "static methods");

            yield return new(("Suggestions.Test.TestClass tc = new Suggestions.",
                ["namespace Suggestions.Test"],
                null),
                "namespaces/namespace members in object creation expression");

            yield return new(("Suggestions.Test.TestClass tc = new Suggestions.Test.",
                ["class Suggestions.Test.TestClass", "class Suggestions.Test.TestClass2"],
                null),
                "namespaces/namespace members in object creation expression 2");

            yield return new(("Suggestions.Test.TestClass tc = new Suggestions.Test.TestClass(",
                ["TestClass()"],
                null),
                "constructors");

            yield return new(("Suggestions.Test.TestClass tc = new Suggestions.Test.TestClass();\r\ntc.",
                ["bool object.Equals(object? obj)", "int object.GetHashCode()", "int TestClass.TestProperty { get; }", "string? object.ToString()", "Type object.GetType()", "void TestClass.TestMethod(int a, int b)", "void TestClass.TestMethod(int a)"],
                null),
                "instance members");

            yield return new(("Suggestions.Test.TestClass tc = new Suggestions.Test.TestClass();\r\ntc.TestMethod(",
                ["void TestClass.TestMethod(int a)", "void TestClass.TestMethod(int a, int b)"],
                null),
                "instance methods");

        }

        [TestCaseSource(nameof(GetSuggestionTestCases))]
        public void GetSuggestionAtPosition(TestCase<(string, IReadOnlyCollection<string>, int? position)> testCase)
        {
            (string text, var expectedSuggestions, int? position) = testCase.Value;
            SourceCode code = new SourceCode(text + "\r\n" + suggestionsTestClass);
            CSharpSyntaxHighlighter highlighter = new CSharpSyntaxHighlighter(false);
            highlighter.Update(code.Lines);

            position ??= text.Length;

            var suggestions = highlighter.GetSuggestionsAtPosition(position.Value, SyntaxPalette.GetLightModePalette(), out _);
            List<string> suggested = new List<string>();
            foreach (var suggestion in suggestions)
            {
                (string s, _) = suggestion.ToolTipSource.GetToolTip();
                suggested.Add(s);
            }
            suggested.Sort();
            Assert.That(suggested, Is.EquivalentTo(expectedSuggestions.OrderBy(x => x)));
        }

        [TestCaseSource(nameof(GetBeforePositionTestCases))]
        public void GetSymbolSpansBeforePosition(SpansTestCase testCase)
        {
            string testString = testCase.Text;
            SourceCode code = new SourceCode(testString);
            CSharpSyntaxHighlighter highlighter = new CSharpSyntaxHighlighter();
            highlighter.Update(code.Lines);
            (int start, int end)[] spans = highlighter.GetSymbolSpansBeforePosition(testCase.Position).ToArray();

            Assert.That(spans, Is.EqualTo(testCase.ExpectedSpans));
        }

        [TestCaseSource(nameof(GetAfterPositionTestCases))]
        public void GetSymbolSpansAfterPosition(SpansTestCase testCase)
        {
            string testString = testCase.Text;
            SourceCode code = new SourceCode(testString);
            CSharpSyntaxHighlighter highlighter = new CSharpSyntaxHighlighter();
            highlighter.Update(code.Lines);
            (int start, int end)[] spans = highlighter.GetSymbolSpansAfterPosition(testCase.Position).ToArray();

            Assert.That(spans, Is.EqualTo(testCase.ExpectedSpans));
        }

        private static IEnumerable<SpansTestCase> GetAfterPositionTestCases() => GetTestCases(false);

        private static IEnumerable<SpansTestCase> GetBeforePositionTestCases() => GetTestCases(true);

        private static IEnumerable<SpansTestCase> GetTestCases(bool reverse)
        {
            StringBuilder sb = new StringBuilder();
            string testString = @"
[//leading] [comment]
[class] [C] [//] [trailing] [comment]
[{]
[}]";
           // string testString = sb.ToString();

            // assume two characters per new line
            /*(int, int)[] expectedSpans = new[]
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
            };*/
            (int, int)[] expectedSpans = TestHelper.GetBracketPositionsAndRemove(testString, string.Empty, out string removedMarkup).ToArray();

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
                yield return new SpansTestCase(removedMarkup, reverse ? span.Item2 : span.Item1, thisExpectedSpans);
                index++;
            }
        }
    }
}
