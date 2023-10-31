using NUnit.Framework;

namespace CSharpTextEditor.Tests
{
    [TestFixture]
    public class SourceCodeTests
    {
        [Test]
        public void RemoveSelectedRange_Test()
        {
            // TODO: More cases
            string text = "Hello" + Environment.NewLine + "World";
            SourceCode code = new SourceCode(text);
            code.SelectRange(0, 2, 1, 3);
            code.RemoveSelectedRange();
            Assert.AreEqual("Held", code.Text);
        }
    }
}