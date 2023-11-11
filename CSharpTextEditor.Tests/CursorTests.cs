using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor.Tests
{
    [TestFixture]
    internal class CursorTests
    {
        private static ISyntaxHighlighter _highlighter;

        [OneTimeSetUp]
        public static void OneTimeSetup()
        {
            _highlighter = new CSharpSyntaxHighlighter(i => new SourceCodePosition(0, i));
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

            sourceCode.ShiftActivePositionUpOneLine(false);
            Assert.AreEqual(1, sourceCode.SelectionEnd.LineNumber);
            Assert.AreEqual(1, sourceCode.SelectionEnd.ColumnNumber);
            sourceCode.ShiftActivePositionUpOneLine(false);
            Assert.AreEqual(0, sourceCode.SelectionEnd.LineNumber);
            Assert.AreEqual(Math.Min(lines[0].Length, lines[2].Length), sourceCode.SelectionEnd.ColumnNumber);

            sourceCode.ShiftActivePositionDownOneLine(false);
            Assert.AreEqual(1, sourceCode.SelectionEnd.LineNumber);
            Assert.AreEqual(1, sourceCode.SelectionEnd.ColumnNumber);
            sourceCode.ShiftActivePositionDownOneLine(false);
            Assert.AreEqual(2, sourceCode.SelectionEnd.LineNumber);
            Assert.AreEqual(lines[2].Length, sourceCode.SelectionEnd.ColumnNumber);
        }

        [TestCaseSource(nameof(AllShiftActions))]
        public void ShiftAction_EmptyText_DoesntMove(string actionName, Action<Cursor> action)
        {
            SourceCode sourceCode = new SourceCode("");
            if (sourceCode.SelectionEnd is not Cursor pos)
            {
                Assert.Fail("Selection End is no longer a SelectionPosition. This test needs updating");
            }
            else
            {
                action(pos);
                Assert.AreEqual(0, pos.LineNumber);
                Assert.AreEqual(0, pos.ColumnNumber);
            }
        }

        [TestCaseSource(nameof(ShiftOneWordCases))]
        public void ShiftOneWordToTheLeft_Test(string lineOfText)
        {
            GetIndices(lineOfText, out string lineWithRemovedMarkup, out int expectedIndex, out int startIndex);
            Cursor position = new Cursor(new LinkedListNode<SourceCodeLine>(new SourceCodeLine(lineWithRemovedMarkup)), startIndex, 0);
            position.ShiftOneWordToTheLeft(_highlighter);
            AssertIsRegeneratedMarkupEqualToOriginal(lineOfText, lineWithRemovedMarkup, position.ColumnNumber, startIndex);
        }

        [TestCaseSource(nameof(ShiftOneWordCases))]
        public void ShiftOneWordToTheRight_Test(string lineOfText)
        {
            GetIndices(lineOfText, out string lineWithRemovedMarkup, out int startIndex, out int expectedIndex);
            Cursor position = new Cursor(new LinkedListNode<SourceCodeLine>(new SourceCodeLine(lineWithRemovedMarkup)), startIndex, 0);
            position.ShiftOneWordToTheRight(_highlighter);
            AssertIsRegeneratedMarkupEqualToOriginal(lineOfText, lineWithRemovedMarkup, startIndex, position.ColumnNumber);
        }

        private static void AssertIsRegeneratedMarkupEqualToOriginal(string lineOfText, string lineWithRemovedMarkup, int startIndex, int endIndex)
        {
            string readdedMarkup = lineWithRemovedMarkup.Substring(0, endIndex) + "]" + lineWithRemovedMarkup.Substring(endIndex);
            readdedMarkup = readdedMarkup.Substring(0, startIndex) + "[" + readdedMarkup.Substring(startIndex);
            Assert.AreEqual(lineOfText, readdedMarkup);
        }

        private static void GetIndices(string lineOfText, out string lineWithRemovedMarkup, out int startIndex, out int endIndex)
        {
            lineWithRemovedMarkup = lineOfText;
            startIndex = lineWithRemovedMarkup.IndexOf('[');
            lineWithRemovedMarkup = lineWithRemovedMarkup.Replace("[", string.Empty);
            endIndex = lineWithRemovedMarkup.IndexOf("]");
            lineWithRemovedMarkup = lineWithRemovedMarkup.Replace("]", string.Empty);
        }

        private static IEnumerable<object[]> ShiftOneWordCases()
        {
            // the [ ] indicates the range over which the expected shift happens
            // (i.e. the [ and ] refer to the start and expected end index for shifting to the right respectively, and for shifting to the left "anti-respectively"
            // it is done like this mainly to make it easy to visualise the tests
            yield return new object[] { "      int result = 2 / 1[;]" };
            yield return new object[] { "      int result = 2 / [1];" };
            yield return new object[] { "      int result = 2 [/ ]1;" };
            yield return new object[] { "      int result = [2 ]/ 1;" };
            yield return new object[] { "      int result [= ]2 / 1;" };
            yield return new object[] { "      int [result ]= 2 / 1;" };
            yield return new object[] { "      [int ]result = 2 / 1;" };
            yield return new object[] { "[      ]int result = 2 / 1;" };

            yield return new object[] { "[IEnumerable]<object>" };
            yield return new object[] { "IEnumerable[<]object>" };
            yield return new object[] { "IEnumerable<[object]>" };
            yield return new object[] { "IEnumerable<object[>]" };
        }

        private static IEnumerable<object[]> AllShiftActions()
        {
            yield return new object[] { nameof(Cursor.ShiftDownOneLine), (Cursor pos) => pos.ShiftDownOneLine() };
            yield return new object[] { nameof(Cursor.ShiftOneCharacterToTheLeft), (Cursor pos) => pos.ShiftOneCharacterToTheLeft() };
            yield return new object[] { nameof(Cursor.ShiftOneCharacterToTheRight), (Cursor pos) => pos.ShiftOneCharacterToTheRight() };
            yield return new object[] { nameof(Cursor.ShiftOneWordToTheLeft), (Cursor pos) => pos.ShiftOneWordToTheLeft(_highlighter) };
            yield return new object[] { nameof(Cursor.ShiftOneWordToTheRight), (Cursor pos) => pos.ShiftOneWordToTheRight(_highlighter) };
            yield return new object[] { nameof(Cursor.ShiftToEndOfLine), (Cursor pos) => pos.ShiftToEndOfLine() };
            yield return new object[] { nameof(Cursor.ShiftToStartOfLine), (Cursor pos) => pos.ShiftToStartOfLine() };
            yield return new object[] { nameof(Cursor.ShiftUpOneLine), (Cursor pos) => pos.ShiftUpOneLine() };
        }
    }
}
