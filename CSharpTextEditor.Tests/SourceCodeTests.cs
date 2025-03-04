using CSharpTextEditor.Languages.CS;
using CSharpTextEditor.Source;
using CSharpTextEditor.UndoRedoActions;
using CSharpTextEditor.Utility;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;
using System.Text;

namespace CSharpTextEditor.Tests
{
    [TestFixture]
    public class SourceCodeTests
    {
        [Test]
        public void Constructor_Test()
        {
            SourceCode code = new SourceCode(string.Empty, new HistoryManager());
            Assert.AreEqual(string.Empty, code.Text);
            Assert.AreEqual(1, code.SelectionRangeCollection.Count);
        }

        [Test]
        public void RemoveSelectedRange_Test()
        {
            // TODO: More cases
            string text = "Hello" + Environment.NewLine + "World";
            SourceCode code = new SourceCode(text, new HistoryManager());
            code.SelectRange(0, 2, 1, 3);
            code.RemoveSelectedRange();
            Assert.AreEqual("Held", code.Text);
        }

        [Test]
        public void ColumnSelect_Test()
        {
            SourceCode code = new SourceCode(@"
                Hello
                Hello
                Hello
                Hello", new HistoryManager());
            code.ColumnSelect(0, 0, 3, 0);
            Assert.AreEqual(4, code.SelectionRangeCollection.Count);
        }

        [Test]
        public void DecreaseIndentAndUndo_Test([Range(0, 10)]int numberOfSpaces)
        {
            string text = new string(' ', numberOfSpaces);
            SourceCode code = new SourceCode(text, new HistoryManager());
            code.SetActivePosition(0, text.Length);

            code.DecreaseIndentAtActivePosition();
            code.Undo();

            Assert.AreEqual(text, code.Text);
        }

        [TestCaseSource(nameof(GetSelectedTextCases))]
        public void GetSelectedText_Test(string text)
        {
            TestHelper.GetBracketPositionsAndRemove(text, out string removedMarkup, out int startIndex, out int endIndex);

            SourceCode code = new SourceCode(removedMarkup, new HistoryManager());
            code.SelectRange(SourceCodePosition.FromCharacterIndex(startIndex, code.Lines), SourceCodePosition.FromCharacterIndex(endIndex, code.Lines));
            string selectedText = code.GetSelectedText();
            string expected = removedMarkup.Substring(startIndex, endIndex - startIndex);
            Assert.AreEqual(expected, selectedText);
        }

        [TestCaseSource(nameof(GetMultiCaretInsertLineBreakTests))]
        public void MultiCaretLineBreak_Test((string startText, string afterRemoving) testCase)
        {
            (string startText, string afterText) = testCase;
            SetupMultiCaretTest(startText, out string sourceText, out SourceCode code, out List<SourceCodePosition> positions);
            SetupMultiCaretTest(afterText, out string after, out _, out List<SourceCodePosition> afterPositions);

            AssertMultiCaretPositions(code, positions);

            CSharpSyntaxHighlighter highlighter = new CSharpSyntaxHighlighter();
            CSharpSpecialCharacterHandler handler = new CSharpSpecialCharacterHandler(highlighter);
            code.InsertLineBreakAtActivePosition(handler);

            AssertPositionsBeforeAndAfterUndo(code, sourceText, after, positions, afterPositions);
        }

        [TestCaseSource(nameof(GetMultiCaretRemoveCharacterTests))]
        public void MultiCaretRemoveCharacter_Test((string startText, string afterRemoving) testCase)
        {
            (string startText, string afterText) = testCase;
            SetupMultiCaretTest(startText, out string sourceText, out SourceCode code, out List<SourceCodePosition> positions);
            SetupMultiCaretTest(afterText, out string after, out _, out List<SourceCodePosition> afterPositions);

            AssertMultiCaretPositions(code, positions);

            code.RemoveCharacterBeforeActivePosition();
            AssertPositionsBeforeAndAfterUndo(code, sourceText, after, positions, afterPositions);
        }

        [TestCaseSource(nameof(GetMultiCaretInsertCharacterTests))]
        public void MultiCaretInsertCharacter_Test((string startText, string afterRemoving) testCase)
        {
            (string startText, string afterText) = testCase;
            SetupMultiCaretTest(startText, out string sourceText, out SourceCode code, out List<SourceCodePosition> positions);
            SetupMultiCaretTest(afterText, out string after, out _, out List<SourceCodePosition> afterPositions);

            AssertMultiCaretPositions(code, positions);

            code.InsertCharacterAtActivePosition('_', null);
            AssertPositionsBeforeAndAfterUndo(code, sourceText, after, positions, afterPositions);
        }

        private static void SetupMultiCaretTest(string startText, out string sourceText, out SourceCode code, out List<SourceCodePosition> positions)
        {
            StringBuilder sourceTextBuilder = new StringBuilder();
            List<(SourceCodePosition?, SourceCodePosition)> ranges = new List<(SourceCodePosition?, SourceCodePosition)>();
            bool firstLine = true;
            int lineIndex = 0;
            foreach (string startTextLine in startText.SplitIntoLines())
            {
                if (!firstLine)
                {
                    sourceTextBuilder.AppendLine();
                }
                firstLine = false;
                var caretPositions = TestHelper.GetBracketPositionsAndRemove(startTextLine, string.Empty, out sourceText).ToList();
                sourceTextBuilder.Append(sourceText);
                //code = new SourceCode(sourceText, new HistoryManager());
                foreach (var (startIndex, endIndex) in caretPositions)
                {
                    SourceCodePosition? tail;
                    SourceCodePosition head;
                    if (endIndex == -1)
                    {
                        tail = null;
                        head = new SourceCodePosition(lineIndex, startIndex);
                    }
                    else
                    {
                        tail = new SourceCodePosition(lineIndex, startIndex);
                        head = new SourceCodePosition(lineIndex, endIndex);
                    }
                    ranges.Add((tail, head));
                }
                lineIndex++;
            }
            sourceText = sourceTextBuilder.ToString();
            code = new SourceCode(sourceText, new HistoryManager());
            positions = new List<SourceCodePosition>();
            bool firstPosition = true;

            // TODO: Add this method to SourceCode
            foreach (var (tail, head) in ranges)
            {
                if (firstPosition)
                {
                    if (tail == null)
                    {
                        code.SetActivePosition(head.LineNumber, head.ColumnNumber);
                    }
                    else
                    {
                        code.SelectRange(tail.Value, head);
                    }
                    firstPosition = false;
                }
                else
                {
                    Cursor? start = null;
                    if (tail != null)
                    {
                        start = code.GetCursor(tail.Value.LineNumber, tail.Value.ColumnNumber);
                    }
                    Cursor end = code.GetCursor(head.LineNumber, head.ColumnNumber);
                    code.SelectionRangeCollection.AddSelectionRange(start, end);
                }
                positions.Add(head);
            }
        }

        private void AssertPositionsBeforeAndAfterUndo(SourceCode code, string before, string after, List<SourceCodePosition> beforePositions, List<SourceCodePosition> afterPositions)
        {
            Assert.That(code.Text, Is.EqualTo(after));
            AssertMultiCaretPositions(code, afterPositions);

            code.Undo();
            Assert.That(code.Text, Is.EqualTo(before));
            AssertMultiCaretPositions(code, beforePositions);

            code.Redo();
            Assert.That(code.Text, Is.EqualTo(after));
            AssertMultiCaretPositions(code, afterPositions);
        }

        private void AssertMultiCaretPositions(SourceCode code, List<SourceCodePosition> positions)
        {
            Assert.That(code.SelectionRangeCollection.Count, Is.EqualTo(positions.Count));
            int count = 0;
            foreach (var range in code.SelectionRangeCollection)
            {
                Assert.That(range.Head.LineNumber, Is.EqualTo(positions[count].LineNumber));
                Assert.That(range.Head.ColumnNumber, Is.EqualTo(positions[count].ColumnNumber));
                count++;
            }
        }

        private static IEnumerable<(string startText, string afterRemoving)> GetMultiCaretRemoveCharacterTests()
        {
            yield return ("[Hello World", "[Hello World");
            yield return ("H[el[lo World", "[e[lo World");
            yield return ("H[el[lo W[orld", "[e[lo [orld");
            yield return ("[Hello ]World", "[World");
            yield return ("[He]llo [Wo]rld", "[llo [rld");

            //yield return ("[Hello[\r\n[World", "HellWorld");
        }

        private static IEnumerable<(string startText, string afterRemoving)> GetMultiCaretInsertCharacterTests()
        {
            // assume '_' is always the character added
            yield return ("[Hello World", "_[Hello World");
            yield return ("[H[ello World", "_[H_[ello World");
            yield return ("Hello [Wor[ld[", "Hello _[Wor_[ld_[");

            yield return ("[He]llo [Wo]rld", "_[llo _[rld");
        }

        private static IEnumerable<(string startText, string afterRemoving)> GetMultiCaretInsertLineBreakTests()
        {
            yield return ("[Hello[World", "\r\n[Hello\r\n[World");

            string multiLineTextBefore =
@"class TestClass
{
    void TestMethod()
    {
        int a = 1;[
        int b = 2;[
    }
}";

            string multiLineTextAftere =
@"class TestClass
{
    void TestMethod()
    {
        int a = 1;
        [
        int b = 2;
        [
    }
}";
            yield return (multiLineTextBefore, multiLineTextAftere);
        }

        private static IEnumerable<object[]> GetSelectedTextCases()
        {
            yield return new[] { "[Hello World]" };
            yield return new[] { "[Hello] World" };
            yield return new[] { "Hello [World]" };
            yield return new[] { "He[llo Wo]rld" };
            yield return new[] { "Hello Worl[d]" };
            yield return new[] { "Hello World[]" };
            yield return new[] { "[]Hello World" };
            yield return new[] { "He[llo" + Environment.NewLine + "Wo]rld" };
            yield return new[] { "[Hello" + Environment.NewLine + "World]" };
            yield return new[] { "Hello" + Environment.NewLine + "[World]" };
        }
    }
}