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
