using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor
{
    public struct SourceCodePosition
    {
        public int LineNumber { get; }

        public int ColumnNumber { get; }

        public SourceCodePosition(int lineNumber, int columnNumber)
        {
            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
        }

        public static SourceCodePosition FromCharacterIndex(int characterIndex, IReadOnlyCollection<string> lines)
        {
            int lineIndex = 0;
            foreach (string line in lines)
            {
                if (characterIndex <= line.Length)
                {
                    return new SourceCodePosition(lineIndex, characterIndex);
                }
                characterIndex -= line.Length;
                characterIndex -= Environment.NewLine.Length;
                lineIndex++;
            }
            throw new Exception("Couldn't find position");
        }
    }
}
