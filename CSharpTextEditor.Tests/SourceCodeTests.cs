﻿using CSharpTextEditor.Languages.CS;
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

        /* TODO: Multi-caret tests for:
         * RemoveSelectedRange
         * DecreaseIndentOnSelectedLines
         * IncreaseIndentOnSelectedLines
         * RemoveTabFromBeforeActivePosition
         * IncreaseIndentAtActivePosition
         * DecreaseIndentAtActivePosition
         * SelectionToLowerCase
         * SelectionToUpperCase
        */


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

        [TestCaseSource(nameof(GetMultiCaretRemoveCharacterBeforeTests))]
        public void MultiCaretRemoveCharacterBefore_Test((string startText, string afterRemoving) testCase)
        {
            (string startText, string afterText) = testCase;
            MultiCaretTest(startText, afterText, code => code.RemoveCharacterBeforeActivePosition());
        }

        [TestCaseSource(nameof(GetMultiCaretRemoveCharacterAfterTests))]
        public void MultiCaretRemoveCharacterAfter_Test((string startText, string afterRemoving) testCase)
        {
            (string startText, string afterText) = testCase;
            MultiCaretTest(startText, afterText, code => code.RemoveCharacterAfterActivePosition());
        }

        [TestCaseSource(nameof(GetMultiCaretRemoveWordBeforeTests))]
        public void MultiCaretRemoveWordBefore_Test((string startText, string afterRemoving) testCase)
        {
            (string startText, string afterText) = testCase;
            MultiCaretTest(startText, afterText, code =>
            {
                CSharpSyntaxHighlighter highlighter = new CSharpSyntaxHighlighter();
                highlighter.Update(code.Lines);
                code.RemoveWordBeforeActivePosition(highlighter);
            });
        }

        [TestCaseSource(nameof(GetMultiCaretRemoveWordAfterTests))]
        public void MultiCaretRemoveWordAfter_Test((string startText, string afterRemoving) testCase)
        {
            (string startText, string afterText) = testCase;
            MultiCaretTest(startText, afterText, code =>
            {
                CSharpSyntaxHighlighter highlighter = new CSharpSyntaxHighlighter();
                highlighter.Update(code.Lines);
                code.RemoveWordAfterActivePosition(highlighter);
            });
        }

        [TestCaseSource(nameof(GetMultiCaretInsertCharacterTests))]
        public void MultiCaretInsertCharacter_Test((string startText, string after, char characterInserted) testCase)
        {
            (string startText, string afterText, char characterInserted) = testCase;
            MultiCaretTest(startText, afterText, code => code.InsertCharacterAtActivePosition(characterInserted, null));
        }

        [TestCaseSource(nameof(GetMultiCaretInsertStringTests))]
        public void MultiCaretInsertString_Test((string startText, string after, string stringAdded) testCase)
        {
            (string startText, string afterText, string stringAdded) = testCase;
            MultiCaretTest(startText, afterText, code => code.InsertStringAtActivePosition(stringAdded));
        }

        [TestCaseSource(nameof(GetMultiCaretDuplicateSelectionTests))]
        public void MultiCaretDuplicateSelection_Test((string startText, string after) testCase)
        {
            (string startText, string afterText) = testCase;
            MultiCaretTest(startText, afterText, code => code.DuplicateSelection());
        }

        private void MultiCaretTest(string startText, string expectedAfter, Action<SourceCode> action)
        {
            SetupMultiCaretTest(startText, out string sourceText, out SourceCode code, out List<SourceCodePosition> positions);
            SetupMultiCaretTest(expectedAfter, out string after, out _, out List<SourceCodePosition> afterPositions);

            AssertMultiCaretPositions(code, positions, "initial");

            action(code);
            AssertPositionsBeforeAndAfterUndo(code, sourceText, after, positions, afterPositions);
        }

        private static void SetupMultiCaretTest(string startText, out string sourceText, out SourceCode code, out List<SourceCodePosition> positions)
        {
            StringBuilder sourceTextBuilder = new StringBuilder();
            List<(SourceCodePosition?, SourceCodePosition)> ranges = new List<(SourceCodePosition?, SourceCodePosition)>();
            positions = new List<SourceCodePosition>();
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
                    positions.Add(head);
                }
                lineIndex++;
            }
            sourceText = sourceTextBuilder.ToString();
            code = new SourceCode(sourceText, new HistoryManager());
            code.SelectRanges(ranges);
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

        private static IEnumerable<(string startText, string afterRemoving)> GetMultiCaretRemoveWordBeforeTests()
        {
            yield return ("Hello[ World[", "[ [");
            yield return ("He[llo] World", "[ World");
            yield return ("He[llo Wor]ld", "[ld");
            yield return ("He[ll]o Wo[rl]d", "[o [d");
        }

        private static IEnumerable<(string startText, string afterRemoving)> GetMultiCaretRemoveWordAfterTests()
        {
            yield return ("[Hello [World", "[");
            yield return ("Hel[lo Wo]rld", "Hel[");
            yield return ("He[ll]o Wo[rl]d", "He[Wo[");
        }

        private static IEnumerable<(string startText, string afterRemoving)> GetMultiCaretRemoveCharacterBeforeTests()
        {
            yield return ("[Hello World", "[Hello World");
            yield return ("[H[ello World", "[ello World"); // two carets should merge to one here
            yield return ("H[el[lo World", "[e[lo World");
            yield return ("H[el[lo W[orld", "[e[lo [orld");
            yield return ("[Hello ]World", "[World");
            yield return ("[He]llo [Wo]rld", "[llo [rld");

            yield return ("[Hello[\r\n[World", "[Hell[World");
        }

        private static IEnumerable<(string startText, string afterRemoving)> GetMultiCaretRemoveCharacterAfterTests()
        {
            yield return ("Hello World[", "Hello World[");
            yield return ("Hello Worl[d[", "Hello Worl[");
            yield return ("[Hello\r\n[World", "[ello\r\n[orld");
            yield return ("H[ell[o World", "H[ll[ World");
            yield return ("Hel[lo W]orld", "Hel[orld");
        }

        private static IEnumerable<(string startText, string after)> GetMultiCaretDuplicateSelectionTests()
        {
            yield return ("[H]ello World", "H[H]ello World");
            yield return ("[Hello\r\n[World", "Hello\r\n[Hello\r\nWorld\r\n[World");
            yield return ("[H]ello [W]orld", "H[H]ello W[W]orld");
        }

        private static IEnumerable<(string startText, string afterRemoving, string stringAdded)> GetMultiCaretInsertStringTests()
        {
            yield return ("[Hello [World", "Foo[Hello Foo[World", "Foo");
            yield return ("[Hello [World", "Foo\r\n[Hello Foo\r\n[World", "Foo\r\n");
            yield return ("[Hello [World", "Foo\r\nBar[Hello Foo\r\nBar[World", "Foo\r\nBar");
            yield return ("[Hello] [World]", "Foo[ Foo[", "Foo");
        }

        private static IEnumerable<(string startText, string afterRemoving, char characterInserted)> GetMultiCaretInsertCharacterTests()
        {
            yield return ("[Hello World", "_[Hello World", '_');
            yield return ("[H[ello World", "_[H_[ello World", '_');
            yield return ("Hello [Wor[ld[", "Hello _[Wor_[ld_[", '_');

            yield return ("[He]llo [Wo]rld", "_[llo _[rld", '_');
            yield return ("[Hello World", $"{SourceCode.TAB_REPLACEMENT}[Hello World", '\t');
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