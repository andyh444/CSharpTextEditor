using CSharpTextEditor.Languages.CS;
using CSharpTextEditor.Source;
using CSharpTextEditor.UndoRedoActions;
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

        [Test]
        public void MulticaretLineBreak_DifferentLines_Test()
        {
            string text =
@"class TestClass
{
    void TestMethod()
    {
        int a = 1;
        int b = 2;
    }
}";
            SourceCode code = new SourceCode(text, new HistoryManager());
            code.ColumnSelect(4, 18, 5, 18); // put two carets at the end of the int assignment lines
            AssertMultiCaretPositions(code, [new SourceCodePosition(4, 18), new SourceCodePosition(5, 18)]);
            CSharpSyntaxHighlighter highlighter = new CSharpSyntaxHighlighter();
            CSharpSpecialCharacterHandler handler = new CSharpSpecialCharacterHandler(highlighter);
            code.InsertLineBreakAtActivePosition(handler);

            AssertMultiCaretPositions(code, [new SourceCodePosition(5, 8), new SourceCodePosition(7, 8)]);
            code.Undo();

            AssertMultiCaretPositions(code, [new SourceCodePosition(4, 18), new SourceCodePosition(5, 18)]);

            code.Redo();
            AssertMultiCaretPositions(code, [new SourceCodePosition(5, 8), new SourceCodePosition(7, 8)]);
        }

        [Test]
        public void MulticaretLineBreak_SameLine_Test()
        {
            string text = "Hello World";
            SourceCode code = new SourceCode(text, new HistoryManager());
            code.SetActivePosition(0, 0);
            code.AddCaret(0, 1);
            AssertMultiCaretPositions(code, [new SourceCodePosition(0, 0), new SourceCodePosition(0, 1)]);

            CSharpSyntaxHighlighter highlighter = new CSharpSyntaxHighlighter();
            CSharpSpecialCharacterHandler handler = new CSharpSpecialCharacterHandler(highlighter);
            code.InsertLineBreakAtActivePosition(handler);

            StringBuilder expectedText = new StringBuilder();
            expectedText.AppendLine()
                .AppendLine("H")
                .Append("ello World");

            Assert.That(code.Text, Is.EqualTo(expectedText.ToString()));
            AssertMultiCaretPositions(code, [new SourceCodePosition(1, 0), new SourceCodePosition(2, 0)]);

            code.Undo();
            Assert.That(code.Text, Is.EqualTo(text));
            AssertMultiCaretPositions(code, [new SourceCodePosition(0, 0), new SourceCodePosition(0, 1)]);

            code.Redo();
            Assert.That(code.Text, Is.EqualTo(expectedText.ToString()));
            AssertMultiCaretPositions(code, [new SourceCodePosition(1, 0), new SourceCodePosition(2, 0)]);
        }

        [Test]
        public void MulticaretInsertCharacter_SameLine_Test()
        {
            string text = "Hello World";
            SourceCode code = new SourceCode(text, new HistoryManager());
            code.SetActivePosition(0, 0);
            code.AddCaret(0, 1);
            AssertMultiCaretPositions(code, [new SourceCodePosition(0, 0), new SourceCodePosition(0, 1)]);

            code.InsertCharacterAtActivePosition('#', null);
            Assert.That(code.Text, Is.EqualTo("#H#ello World"));
            AssertMultiCaretPositions(code, [new SourceCodePosition(0, 1), new SourceCodePosition(0, 3)]);

            code.Undo();
            AssertMultiCaretPositions(code, [new SourceCodePosition(0, 0), new SourceCodePosition(0, 1)]);

            code.Redo();
            AssertMultiCaretPositions(code, [new SourceCodePosition(0, 1), new SourceCodePosition(0, 3)]);
        }

        [Test]
        public void MulticaretRemoveCharacter_SameLine_Test()
        {
            string text = "Hello World";
            SourceCode code = new SourceCode(text, new HistoryManager());
            code.SetActivePosition(0, 1);
            code.AddCaret(0, 3);
            AssertMultiCaretPositions(code, [new SourceCodePosition(0, 1), new SourceCodePosition(0, 3)]);

            code.RemoveCharacterBeforeActivePosition();
            Assert.That(code.Text, Is.EqualTo("elo World"));
            AssertMultiCaretPositions(code, [new SourceCodePosition(0, 0), new SourceCodePosition(0, 1)]);

            code.Undo();
            AssertMultiCaretPositions(code, [new SourceCodePosition(0, 1), new SourceCodePosition(0, 3)]);

            code.Redo();
            AssertMultiCaretPositions(code, [new SourceCodePosition(0, 0), new SourceCodePosition(0, 1)]);
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