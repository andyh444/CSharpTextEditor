using CSharpTextEditor.UndoRedoActions;
using NUnit.Framework;

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
        public void InsertStringAtActivePosition_UndoRedo_Test()
        {
            SourceCode code = new SourceCode("", new HistoryManager());
            code.SetActivePosition(0, 0);

            string toInsert = @"
void Method()
{
    Foo.Bar();
}";
            code.InsertStringAtActivePosition(toInsert);
            Assert.That(code.Text, Is.EqualTo(toInsert));
            code.Undo();
            Assert.That(code.Text, Is.EqualTo(""));
            code.Redo();
            Assert.That(code.Text, Is.EqualTo(toInsert));
        }

        private static IEnumerable<string> GetSelectedTextCases()
        {
            yield return "[Hello World]";
            yield return "[Hello] World";
            yield return "Hello [World]";
            yield return "He[llo Wo]rld";
            yield return "Hello Worl[d]";
            yield return "Hello World[]";
            yield return "[]Hello World";
            yield return "He[llo" + Environment.NewLine + "Wo]rld";
            yield return "[Hello" + Environment.NewLine + "World]";
            yield return "Hello" + Environment.NewLine + "[World]";
        }
    }
}