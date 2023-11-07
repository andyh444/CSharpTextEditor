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
            int expectedIndex = lineOfText.IndexOf('[');
            lineOfText = lineOfText.Replace("[", string.Empty);
            int startIndex = lineOfText.IndexOf("]");
            lineOfText = lineOfText.Replace("]", string.Empty);

            SelectionPosition position = new SelectionPosition(new LinkedListNode<string>(lineOfText), startIndex, 0);
            position.ShiftOneWordToTheLeft();
            Assert.AreEqual(expectedIndex, position.ColumnNumber);
        }

        [TestCaseSource(nameof(ShiftOneWordCases))]
        public void ShiftOneWordToTheRight_Test(string lineOfText)
        {
            int startIndex = lineOfText.IndexOf('[');
            lineOfText = lineOfText.Replace("[", string.Empty);
            int expectedIndex = lineOfText.IndexOf("]");
            lineOfText = lineOfText.Replace("]", string.Empty);

            SelectionPosition position = new SelectionPosition(new LinkedListNode<string>(lineOfText), startIndex, 0);
            position.ShiftOneWordToTheRight();
            Assert.AreEqual(expectedIndex, position.ColumnNumber);
        }

        private static IEnumerable<object[]> ShiftOneWordCases()
        {
            // the [ ] indicates the range over which the expected shift happens
            // (i.e. the [ and ] refer to the start and expected end index for shifting to the right respectively, and for shifting to the left "anti-respectively"
            yield return new object[] { "      int result = 2 / 1[;]" };
            yield return new object[] { "      int result = 2 / [1];" };
            yield return new object[] { "      int result = 2 [/ ]1;" };
            yield return new object[] { "      int result = [2 ]/ 1;" };
            yield return new object[] { "      int result [= ]2 / 1;" };
            yield return new object[] { "      int [result ]= 2 / 1;" };
            yield return new object[] { "      [int ]result = 2 / 1;" };
            yield return new object[] { "[      ]int result = 2 / 1;" };
        }

    }
}
