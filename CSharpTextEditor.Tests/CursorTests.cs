using CSharpTextEditor.Languages.CS;
using CSharpTextEditor.Languages;
using CSharpTextEditor.Source;
using CSharpTextEditor.UndoRedoActions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using CSharpTextEditor.View;

namespace CSharpTextEditor.Tests
{
    [TestFixture]
    internal class CursorTests
    {
        private static ISyntaxHighlighter _highlighter;

        [OneTimeSetUp]
        public static void OneTimeSetup()
        {
            _highlighter = new CSharpSyntaxHighlighter();
        }

        [Test]
        public void ShiftUpAndDownLines_NoSelection_Test()
        {
            string[] lines = new[]
            {
                "public class Hello",
                "{",
                "   void Method() {}",
                "}"
            };
            SourceCode sourceCode = new SourceCode(string.Join(Environment.NewLine, lines));
            sourceCode.SetActivePosition(2, lines[2].Length);

            sourceCode.ShiftHeadUpOneLine(false);
            Assert.AreEqual(1, sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head.LineNumber);
            Assert.AreEqual(1, sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head.ColumnNumber);
            sourceCode.ShiftHeadUpOneLine(false);
            Assert.AreEqual(0, sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head.LineNumber);
            Assert.AreEqual(Math.Min(lines[0].Length, lines[2].Length), sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head.ColumnNumber);

            sourceCode.ShiftHeadDownOneLine(false);
            Assert.AreEqual(1, sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head.LineNumber);
            Assert.AreEqual(1, sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head.ColumnNumber);
            sourceCode.ShiftHeadDownOneLine(false);
            Assert.AreEqual(2, sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head.LineNumber);
            Assert.AreEqual(lines[2].Length, sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head.ColumnNumber);
        }

        [TestCaseSource(nameof(AllShiftActions))]
        public void ShiftAction_EmptyText_DoesntMove((string actionName, Func<Cursor, bool> action) testCase)
        {
            (string actionName, Func<Cursor, bool> action) = testCase;
            SourceCode sourceCode = new SourceCode("");
            Cursor pos = sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head;
            action(pos);
            Assert.AreEqual(0, pos.LineNumber);
            Assert.AreEqual(0, pos.ColumnNumber);
        }

        [TestCaseSource(nameof(ShiftOneWordCases))]
        public void ShiftOneWordToTheLeft_Test(string lineOfText)
        {
            TestHelper.GetBracketPositionsAndRemove(lineOfText, out string lineWithRemovedMarkup, out int expectedIndex, out int startIndex);
            SourceCode sourceCode = new SourceCode(lineWithRemovedMarkup);

            ISyntaxHighlighter highlighter = new CSharpSyntaxHighlighter();
            highlighter.Update(sourceCode.Lines);

            Cursor position = sourceCode.GetCursor(0, startIndex);
            position.ShiftOneWordToTheLeft(highlighter);
            AssertIsRegeneratedMarkupEqualToOriginal(lineOfText, lineWithRemovedMarkup, position.ColumnNumber, startIndex);
        }

        [TestCaseSource(nameof(ShiftOneWordCases))]
        public void ShiftOneWordToTheRight_Test(string lineOfText)
        {
            TestHelper.GetBracketPositionsAndRemove(lineOfText, out string lineWithRemovedMarkup, out int startIndex, out int expectedIndex);
            SourceCode sourceCode = new SourceCode(lineWithRemovedMarkup);

            ISyntaxHighlighter highlighter = new CSharpSyntaxHighlighter();
            highlighter.Update(sourceCode.Lines);

            Cursor position = sourceCode.GetCursor(0, startIndex);
            position.ShiftOneWordToTheRight(highlighter);
            AssertIsRegeneratedMarkupEqualToOriginal(lineOfText, lineWithRemovedMarkup, startIndex, position.ColumnNumber);
        }

        [TestCase("He[ll]o World", false, -2)]
        [TestCase("He[ll]o World", true, 2)]
        [TestCase("[\r\n]", false, -1)]
        [TestCase("[\r\n]", true, 1)]
        public void GetPositionDifference_Test(string text, bool swap, int expectedDifference)
        {
            var ranges = TestHelper.GetPositionRanges(text, out string textWithoutMarkup);
            if (ranges.Count != 1)
            {
                Assert.Fail("Invalid test - should only be one range");
                return;
            }
            SourceCode sourceCode = new SourceCode(textWithoutMarkup);
            (SourceCodePosition? start, SourceCodePosition end) = ranges.Single();
            if (start == null)
            {
                Assert.Fail("Invalid test - needs an open and closing bracket");
                return;
            }
            var tail = sourceCode.GetCursor(swap ? end : start.Value);
            var head = sourceCode.GetCursor(swap ? start.Value : end);

            var diff = head.GetPositionDifference(tail);
            Assert.That(diff, Is.EqualTo(expectedDifference));
        }

        private static void AssertIsRegeneratedMarkupEqualToOriginal(string lineOfText, string lineWithRemovedMarkup, int startIndex, int endIndex)
        {
            string readdedMarkup = lineWithRemovedMarkup.Substring(0, endIndex) + "]" + lineWithRemovedMarkup.Substring(endIndex);
            readdedMarkup = readdedMarkup.Substring(0, startIndex) + "[" + readdedMarkup.Substring(startIndex);
            Assert.AreEqual(lineOfText, readdedMarkup);
        }



        private static IEnumerable<string> ShiftOneWordCases()
        {
            // the [ ] indicates the range over which the expected shift happens
            // (i.e. the [ and ] refer to the start and expected end index for shifting to the right respectively, and for shifting to the left "anti-respectively"
            // it is done like this mainly to make it easy to visualise the tests
            yield return "      int result = 2 / 1[;]";
            yield return "      int result = 2 / [1];";
            yield return "      int result = 2 [/ ]1;";
            yield return "      int result = [2 ]/ 1;";
            yield return "      int result [= ]2 / 1;";
            yield return "      int [result ]= 2 / 1;";
            yield return "      [int ]result = 2 / 1;";
            yield return "[      ]int result = 2 / 1;";

            yield return "[IEnumerable]<object>";
            yield return "IEnumerable[<]object>";
            yield return "IEnumerable<[object]>";
            yield return "IEnumerable<object[>]";

            yield return "// [hello ]world";
        }

        private static IEnumerable<(string actionName, Func<Cursor, bool> action)> AllShiftActions()
        {
            yield return (nameof(Cursor.ShiftDownOneLine), (Cursor pos) => pos.ShiftDownOneLine());
            yield return (nameof(Cursor.ShiftOneCharacterToTheLeft), (Cursor pos) => pos.ShiftOneCharacterToTheLeft());
            yield return (nameof(Cursor.ShiftOneCharacterToTheRight), (Cursor pos) => pos.ShiftOneCharacterToTheRight());
            yield return (nameof(Cursor.ShiftOneWordToTheLeft), (Cursor pos) => pos.ShiftOneWordToTheLeft(_highlighter));
            yield return (nameof(Cursor.ShiftOneWordToTheRight), (Cursor pos) => pos.ShiftOneWordToTheRight(_highlighter));
            yield return (nameof(Cursor.ShiftToEndOfLine), (Cursor pos) => pos.ShiftToEndOfLine());
            yield return (nameof(Cursor.ShiftToHome), (Cursor pos) => pos.ShiftToHome());
            yield return (nameof(Cursor.ShiftUpOneLine), (Cursor pos) => pos.ShiftUpOneLine());
        }
    }
}
