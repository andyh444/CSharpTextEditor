using System.IO;
using System.Text;

namespace CSharpTextEditor.CS
{
    internal class CSharpSpecialCharacterHandler : ISpecialCharacterHandler
    {
        public void HandleLineBreakInserted(SourceCode sourceCode, Cursor activePosition)
        {
            if (activePosition.Line.Previous != null)
            {
                sourceCode.InsertStringAtActivePosition(GetWhiteSpaceAtBeginningOfLine(activePosition.Line.Previous.Value.Text));
            }
        }

        private string GetWhiteSpaceAtBeginningOfLine(string previousLine)
        {
            StringBuilder sb = new StringBuilder();
            using (StringReader sr = new StringReader(previousLine))
            {
                char[] buffer = new char[1];
                int currentReadAmount = sr.Read(buffer, 0, 1);
                while (currentReadAmount > 0)
                {
                    char currentChar = buffer[0];
                    if (char.IsWhiteSpace(currentChar))
                    {
                        sb.Append(currentChar);
                    }
                    else if (currentChar == '{')
                    {
                        sb.Append(SourceCode.TAB_REPLACEMENT);
                        break;
                    }
                    else
                    {
                        break;
                    }
                    currentReadAmount = sr.Read(buffer, 0, 1);
                }
            }
            return sb.ToString();
        }

        public void HandleCharacterInserting(char character, SourceCode sourceCode)
        {
            if (character == '}')
            {
                sourceCode.RemoveTabFromBeforeActivePosition();
            }
        }
    }
}
