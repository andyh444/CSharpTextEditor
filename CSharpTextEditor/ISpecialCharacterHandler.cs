using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor
{
    internal interface ISpecialCharacterHandler
    {
        void HandleLineBreakInserted(SourceCode sourceCode, SelectionPosition activePosition);

        void HandleCharacterInserting(char character, SourceCode sourceCode);
    }

    internal class CSharpSpecialCharacterHandler : ISpecialCharacterHandler
    {
        public void HandleLineBreakInserted(SourceCode sourceCode, SelectionPosition activePosition)
        {
            if (activePosition.Line.Previous != null)
            {
                sourceCode.InsertStringAtActivePosition(GetWhiteSpaceAtBeginningOfLine(activePosition.Line.Previous.Value));
            }
        }

        private string GetWhiteSpaceAtBeginningOfLine(string previousLine)
        {
            StringBuilder sb = new StringBuilder();
            using (StringReader sr = new StringReader(previousLine))
            {
                Span<char> buffer = new Span<char>(new char[1]);
                int currentReadAmount = sr.Read(buffer);
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
                    currentReadAmount = sr.Read(buffer);
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
