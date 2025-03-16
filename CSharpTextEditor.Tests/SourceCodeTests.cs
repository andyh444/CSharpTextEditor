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
            SourceCode code = new SourceCode(string.Empty);
            Assert.AreEqual(string.Empty, code.Text);
            Assert.AreEqual(1, code.SelectionRangeCollection.Count);
        }

        [Test]
        public void ColumnSelect_Test()
        {
            SourceCode code = new SourceCode(@"
                Hello
                Hello
                Hello
                Hello");
            code.ColumnSelect(0, 0, 3, 0);
            Assert.AreEqual(4, code.SelectionRangeCollection.Count);
        }

        [TestCaseSource(nameof(GetSelectedTextCases))]
        public void GetSelectedText_Test(string text)
        {
            TestHelper.GetBracketPositionsAndRemove(text, out string removedMarkup, out int startIndex, out int endIndex);

            SourceCode code = new SourceCode(removedMarkup);
            code.SelectRange(SourceCodePosition.FromCharacterIndex(startIndex, code.Lines), SourceCodePosition.FromCharacterIndex(endIndex, code.Lines));
            string selectedText = code.GetSelectedText();
            string expected = removedMarkup.Substring(startIndex, endIndex - startIndex);
            Assert.AreEqual(expected, selectedText);
        }

        [TestCase("Hel[lo\r\nWorl]d", "Hel[d")]
        [TestCase("H[el]lo W[or]ld", "H[lo W[ld")]
        public void MultiCaretRemoveSelectedRange_Test(string startText, string afterText)
        {
            MultiCaretTest(startText, afterText, code => code.RemoveSelectedRange());
        }


        [TestCaseSource(nameof(GetMultiCaretInsertLineBreakTests))]
        public void MultiCaretLineBreak_Test((string startText, string afterRemoving) testCase)
        {
            (string startText, string afterText) = testCase;
            MultiCaretTest(startText, afterText, code =>
            {
                CSharpSyntaxHighlighter highlighter = new CSharpSyntaxHighlighter();
                CSharpSpecialCharacterHandler handler = new CSharpSpecialCharacterHandler(highlighter);
                code.InsertLineBreakAtActivePosition(handler);
            });
        }

        [TestCase("[Hello World", "[Hello World")]
        [TestCase("[H[ello World", "[ello World")]
        [TestCase("H[el[lo World", "[e[lo World")]
        [TestCase("H[el[lo W[orld", "[e[lo [orld")]
        [TestCase("[Hello ]World", "[World")]
        [TestCase("[He]llo [Wo]rld", "[llo [rld")]
        [TestCase("[Hello[\r\n[World", "[Hell[World")]
        public void MultiCaretRemoveCharacterBefore_Test(string startText, string afterText)
        {
            MultiCaretTest(startText, afterText, code => code.RemoveCharacterBeforeActivePosition());
        }

        [TestCase("Hello World[", "Hello World[")]
        [TestCase("Hello Worl[d[", "Hello Worl[")]
        [TestCase("[Hello\r\n[World", "[ello\r\n[orld")]
        [TestCase("H[ell[o World", "H[ll[ World")]
        [TestCase("Hel[lo W]orld", "Hel[orld")]
        public void MultiCaretRemoveCharacterAfter_Test(string startText, string afterText)
        {
            MultiCaretTest(startText, afterText, code => code.RemoveCharacterAfterActivePosition());
        }

       
        [TestCase("Hello[ World[", "[ [")]
        [TestCase("He[llo] World", "[ World")]
        [TestCase("He[llo Wor]ld", "[ld")]
        [TestCase("He[ll]o Wo[rl]d", "[o [d")]
        public void MultiCaretRemoveWordBefore_Test(string startText, string afterText)
        {
            MultiCaretTest(startText, afterText, code =>
            {
                CSharpSyntaxHighlighter highlighter = new CSharpSyntaxHighlighter();
                highlighter.Update(code.Lines);
                code.RemoveWordBeforeActivePosition(highlighter);
            });
        }

        [TestCase("[Hello [World", "[")]
        [TestCase("[Hello\r\n[World", "[\r\n[")]
        [TestCase("Hel[lo Wo]rld", "Hel[")]
        [TestCase("He[ll]o Wo[rl]d", "He[Wo[")]
        public void MultiCaretRemoveWordAfter_Test(string startText, string afterText)
        {
            MultiCaretTest(startText, afterText, code =>
            {
                CSharpSyntaxHighlighter highlighter = new CSharpSyntaxHighlighter();
                highlighter.Update(code.Lines);
                code.RemoveWordAfterActivePosition(highlighter);
            });
        }

        [TestCase("Hello\r\n[World", "[Hello")]
        [TestCase("Hello\r\n[Wor[ld", "[Hello")]
        [TestCase("Hello\r\n[Wor]ld", "[Hello")]
        [TestCase("[Hello\r\nWorld\r\n[Hello", "[World")]
        [TestCase("Hello\r\n[World\r\nH]ell[o\r\nWorld]\r\nHello", "[Hello\r\n[Hello")]
        public void MultiCaretRemoveLine_Test(string startText, string afterText)
        {
            MultiCaretTest(startText, afterText, code => code.RemoveLineAtActivePosition());
        }


        [TestCase("[Hello World", "_[Hello World", '_', false)]
        [TestCase("[H[ello World", "_[H_[ello World", '_', false)]
        [TestCase("Hello [Wor[ld[", "Hello _[Wor_[ld_[", '_', false)]
        [TestCase("[He]llo [Wo]rld", "_[llo _[rld", '_', false)]
        [TestCase("[Hello World", $"{SourceCode.TAB_REPLACEMENT}[Hello World", '\t', false)]

        [TestCase("[Hello World", "_[ello World", '_', true)]
        [TestCase("[Hello [World", "_[ello _[orld", '_', true)]
        [TestCase("[He]llo World", "_[llo World", '_', true)]
        public void MultiCaretInsertCharacter_Test(string startText, string afterText, char characterInserted, bool overtype)
        {
            MultiCaretTest(startText, afterText, code => code.InsertCharacterAtActivePosition(characterInserted, null), overtype);
        }

        [TestCase("[Hello [World", "Foo[Hello Foo[World", "Foo")]
        [TestCase("[Hello [World", "Foo\r\n[Hello Foo\r\n[World", "Foo\r\n")]
        [TestCase("[Hello [World", "Foo\r\nBar[Hello Foo\r\nBar[World", "Foo\r\nBar")]
        [TestCase("[Hello] [World]", "Foo[ Foo[", "Foo")]
        [TestCase("[\r\n[\r\n[", "A[\r\nB[\r\nC[", "A\r\nB\r\nC")]
        public void MultiCaretInsertString_Test(string startText, string afterText, string stringAdded)
        {
            MultiCaretTest(startText, afterText, code => code.InsertStringAtActivePosition(stringAdded));
        }

        [TestCase("[H]ello World", "H[H]ello World")]
        [TestCase("[Hello\r\n[World", "Hello\r\n[Hello\r\nWorld\r\n[World")]
        [TestCase("[H]ello [W]orld", "H[H]ello W[W]orld")]
        public void MultiCaretDuplicateSelection_Test(string startText, string afterText)
        {
            MultiCaretTest(startText, afterText, code => code.DuplicateSelection());
        }

        [TestCase("[Hello World]", "[HELLO WORLD]")]
        [TestCase("He[llo] Wo[rld]", "He[LLO] Wo[RLD]")]
        [TestCase("[Hello World", "[Hello World")]
        public void MultiCaretToUpperCase_Test(string startText, string afterText)
        {
            MultiCaretTest(startText, afterText, code => code.SelectionToUpperCase());
        }

        [TestCase("[Hello World]", "[hello world]")]
        [TestCase("[He]llo [Wo]rld", "[he]llo [wo]rld")]
        [TestCase("[Hello World", "[Hello World")]
        public void MultiCaretToLowerCase_Test(string startText, string afterText)
        {
            MultiCaretTest(startText, afterText, code => code.SelectionToLowerCase());
        }

        [TestCase("[Hello World", "    [Hello World")]
        [TestCase("[    Hello World", "    [    Hello World")]
        [TestCase("[Hello [World", "    [Hello   [World")]
        [TestCase("H[ello World", "H   [ello World")]
        [TestCase("[H]ello World", "    [ello World")]
        [TestCase("[H]ello [W]orld", "    [ello    [orld")]
        [TestCase("[Hello [W]orld", "    [Hello   [orld")]
        [TestCase("[Hello\r\n[World", "    [Hello\r\n    [World")]
        [TestCase("[Hello\r\nWorld]", "    [Hello\r\n    World]")]
        [TestCase("He[llo\r\nWor]ld", "    He[llo\r\n    Wor]ld")]
        [TestCase("[Hel[lo\r\nWo]rld", "    [Hel[lo\r\n    Wo]rld")] // TODO: More tests like this
        public void MultiCaretIncreaseIndent_Test(string startText, string afterText)
        {
            MultiCaretTest(startText, afterText, code => code.IncreaseIndentAtActivePosition());
        }

        private static IEnumerable<object[]> DecreaseIndentCases()
        {
            foreach (var testCase in SourceCodeLineTests.DecreaseIndentCases())
            {
                yield return testCase;
            }
            yield return new object[] { "\t[Hello\r\n\t[World", "[Hello\r\n[World" };
            yield return new object[] { "\t[He]llo\r\n\t[Wo]rld", "[He]llo\r\n[Wo]rld" };
            yield return new object[] { "\t[He]llo\r\n\tW[or]ld", "[He]llo\r\n\tW[or]ld" };
            yield return new object[] { "\t[Hello\r\n\tWorld]", "[Hello\r\nWorld]" };
            yield return new object[] { "\tHe[llo\r\n\tWor]ld", "He[llo\r\nWor]ld" };
            yield return new object[] { "\tHe[llo\r\n\tWor]ld\r\n\t[Hello\r\n\tWorld]", "He[llo\r\nWor]ld\r\n[Hello\r\nWorld]" };
            yield return new object[] { "\tH[el]lo World", "\tH[el]lo World" };
            yield return new object[] { "\t\t[Hel[lo\r\n\t\tWo]rld", "\t[Hel[lo\r\n\tWo]rld" }; // TODO: More tests like this
        }

        [TestCaseSource(nameof(DecreaseIndentCases))]
        public void MultiCaretDecreaseIndent_Test(string startText, string afterText)
        {
            startText = startText.Replace(".", " ").Replace("\t", SourceCode.TAB_REPLACEMENT);
            afterText = afterText.Replace(".", " ").Replace("\t", SourceCode.TAB_REPLACEMENT);
            if (!startText.Contains("["))
            {
                if (startText.Contains("H"))
                {
                    startText = startText.Replace("H", "[H");
                    afterText = afterText.Replace("H", "[H");
                }
                else
                {
                    startText += "[";
                    afterText += "[";
                }
            }
            MultiCaretTest(startText, afterText, code => code.DecreaseIndentAtActivePosition());
        }

        private void MultiCaretTest(string startText, string expectedAfter, Action<SourceCode> action, bool overtype = false)
        {
            SetupMultiCaretTest(startText, out string sourceText, out SourceCode code, out List<SourceCodePosition> positions);
            code.OvertypeEnabled = overtype;

            SetupMultiCaretTest(expectedAfter, out string after, out _, out List<SourceCodePosition> afterPositions);

            AssertMultiCaretPositions(code, positions, "initial");

            action(code);
            AssertPositionsBeforeAndAfterUndo(code, sourceText, after, positions, afterPositions);
        }

        private static void SetupMultiCaretTest(string startText, out string sourceText, out SourceCode code, out List<SourceCodePosition> positions)
        {
            List<(SourceCodePosition?, SourceCodePosition)> ranges = TestHelper.GetPositionRanges(startText, out sourceText);
            code = new SourceCode(sourceText);
            code.SelectRanges(ranges);
            positions = code.SelectionRangeCollection.Select(x => x.Head.GetPosition()).ToList();
        }

        private void AssertPositionsBeforeAndAfterUndo(SourceCode code, string before, string after, List<SourceCodePosition> beforePositions, List<SourceCodePosition> afterPositions)
        {
            Assert.That(code.Text, Is.EqualTo(after), "Text not expected after action");
            AssertMultiCaretPositions(code, afterPositions, "after action");

            code.Undo();
            Assert.That(code.Text, Is.EqualTo(before), "Text not expected after undo");
            AssertMultiCaretPositions(code, beforePositions, "after undo");

            code.Redo();
            Assert.That(code.Text, Is.EqualTo(after), "Text not expected after redo");
            AssertMultiCaretPositions(code, afterPositions, "after redo");
        }

        private void AssertMultiCaretPositions(SourceCode code, List<SourceCodePosition> positions, string stepName)
        {
            Assert.That(code.SelectionRangeCollection.Count, Is.EqualTo(positions.Count), $"Unexpected selection range count for step \"{stepName}\"");
            int count = 0;
            foreach (var range in code.SelectionRangeCollection)
            {
                Assert.That(range.Head.LineNumber, Is.EqualTo(positions[count].LineNumber), $"Unexpected line number for step \"{stepName}\"");
                Assert.That(range.Head.ColumnNumber, Is.EqualTo(positions[count].ColumnNumber), $"Unexpected column number for step \"{stepName}\"");
                count++;
            }
        }

        private static IEnumerable<(string startText, string afterRemoving)> GetMultiCaretInsertLineBreakTests()
        {
            yield return ("[Hello[World", "\r\n[Hello\r\n[World");
            yield return ("[He]llo[Wo]rld", "\r\n[llo\r\n[rld");

            string multiLineTextBefore =
@"class TestClass
{
    void TestMethod()
    {
        int a = 1;[
        int b = 2;[
    }
}";

            string multiLineTextAfter =
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
            yield return (multiLineTextBefore, multiLineTextAfter);
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