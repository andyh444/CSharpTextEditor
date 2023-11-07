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
    internal class SelectionPositionTests
    {
        [TestCaseSource(nameof(ShiftOneWordCases))]
        public void ShiftOneWordToTheLeft_Test(string lineOfText)
        {
            GetIndices(lineOfText, out string lineWithRemovedMarkup, out int expectedIndex, out int startIndex);
            SelectionPosition position = new SelectionPosition(new LinkedListNode<string>(lineWithRemovedMarkup), startIndex, 0);
            position.ShiftOneWordToTheLeft();
            AssertIsRegeneratedMarkupEqualToOriginal(lineOfText, lineWithRemovedMarkup, position.ColumnNumber, startIndex);
        }

        [TestCaseSource(nameof(ShiftOneWordCases))]
        public void ShiftOneWordToTheRight_Test(string lineOfText)
        {
            GetIndices(lineOfText, out string lineWithRemovedMarkup, out int startIndex, out int expectedIndex);
            SelectionPosition position = new SelectionPosition(new LinkedListNode<string>(lineWithRemovedMarkup), startIndex, 0);
            position.ShiftOneWordToTheRight();
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

    }
}
