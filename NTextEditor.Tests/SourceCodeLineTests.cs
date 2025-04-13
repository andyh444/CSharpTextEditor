using NTextEditor.Source;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTextEditor.Tests
{
    [TestFixture]
    [Timeout(1000)]
    public class SourceCodeLineTests
    {
        [TestCaseSource(nameof(DecreaseIndentCases))]
        public void FirstNonWhiteSpaceIndex_Test(string inputText, string _)
        {
            SourceCodeLine line = new SourceCodeLine(inputText.Replace(".", " "));
            Assert.AreEqual(inputText.LastIndexOf(".") + 1, line.FirstNonWhiteSpaceIndex);
        }

        public static IEnumerable<object[]> DecreaseIndentCases()
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
        }
    }
}
