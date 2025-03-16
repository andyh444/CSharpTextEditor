using CSharpTextEditor.Languages.CS;
using CSharpTextEditor.Source;
using CSharpTextEditor.UndoRedoActions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor.Tests
{
    [TestFixture]
    internal class SourceCodePositionTests
    {
        [TestCaseSource(nameof(FromCharacterIndexCases))]
        public void FromCharacterIndex_Test(string[] lines, int expectedIndex)
        {
            TestHelper.GetBracketPositionsAndRemove(string.Join(Environment.NewLine, lines), out _, out int index, out _);
            Assert.AreEqual(expectedIndex, index);
        }

        [Test]
        public void FromCharacterIndex2_Test()
        {
            string code = @"This is
a test string
and it's a bit stupid";
            SourceCode sourceCode = new SourceCode(code);
            (_, System.Collections.Immutable.ImmutableList<int> lineLengths) = CSharpSyntaxHighlighter.GetText(sourceCode.Lines);

            Cursor cursor = sourceCode.GetCursor(0, 0);
            int index = 0;
            int currentCursorLine = 0;
            do
            {
                SourceCodePosition cursorPosition = cursor.GetPosition();
                if (cursorPosition.LineNumber != currentCursorLine)
                {
                    index += 1;
                    currentCursorLine = cursorPosition.LineNumber;
                }
                SourceCodePosition pos2 = SourceCodePosition.FromCharacterIndex(index, sourceCode.Lines);
                SourceCodePosition pos = SourceCodePosition.FromCharacterIndex(index, lineLengths);
                Assert.AreEqual(cursorPosition, pos);
                index++;
            }
            while (cursor.ShiftOneCharacterToTheRight());
        }

        private static IEnumerable<object[]> FromCharacterIndexCases()
        {
            int helloLength = 5;
            int newLineLength = Environment.NewLine.Length;
            yield return new object[] { new[] { "[Hello" }, 0 };
            yield return new object[] { new[] { "H[ello" }, 1 };
            yield return new object[] { new[] { "Hello[" }, 5 };
            yield return new object[] { new[] { "Hello", "[World" }, helloLength + newLineLength };
            yield return new object[] { new[] { "Hello", "W[orld" }, helloLength + newLineLength + 1 };
            yield return new object[] { new[] { "Hello", "World[" }, helloLength + newLineLength + 5 };
        }
    }
}
