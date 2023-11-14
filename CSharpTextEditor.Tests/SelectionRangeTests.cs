using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor.Tests
{
    [TestFixture]
    [Timeout(1000)]
    public class SelectionRangeTests
    {
        [TestCaseSource(nameof(DecreaseIndentOnSelectedLinesCases))]
        public void DecreaseIndentOnSelectedLines_Test(string inputText, string expectedAfterIndentDecrease)
        {
            SourceCode code = new SourceCode(inputText.Replace(".", " "));
            code.SelectAll();
            SelectionRange range = code.SelectionRangeCollection.PrimarySelectionRange;
            range.DecreaseIndentOnSelectedLines();
            Assert.AreEqual(expectedAfterIndentDecrease.Replace(".", " "), code.Text);
        }

        private static IEnumerable<object[]> DecreaseIndentOnSelectedLinesCases()
        {
            // use dots instead of spaces here to make it easier to visualise
            yield return new object[] { "", "" };
            yield return new object[] { ".", "" };
            yield return new object[] { "..", "" };
            yield return new object[] { "...", "" };
            yield return new object[] { "....", "" };
            yield return new object[] { ".....", "...." };
            yield return new object[] { "Hello", "Hello" };
            yield return new object[] { ".Hello", "Hello" };
            yield return new object[] { "..Hello", "Hello" };
            yield return new object[] { "...Hello", "Hello" };
            yield return new object[] { "....Hello", "Hello" }; // a tab is currently 4 spaces. If that changes, then this test will need to change
            yield return new object[] { ".....Hello",    "....Hello" };
            yield return new object[] { "......Hello",   "....Hello" };
            yield return new object[] { ".......Hello",  "....Hello" };
            yield return new object[] { "........Hello", "....Hello" };
            yield return new object[] { ".........Hello",    "........Hello" };
            yield return new object[] { "..........Hello",   "........Hello" };
            yield return new object[] { "...........Hello",  "........Hello" };
            yield return new object[] { "............Hello", "........Hello" };

            // TODO: Multi-line strings
        }
    }
}
