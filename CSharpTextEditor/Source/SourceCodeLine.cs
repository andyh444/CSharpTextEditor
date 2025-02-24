using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor.Source
{
    internal class SourceCodeLine
    {
        public int FirstNonWhiteSpaceIndex
        {
            get
            {
                int index = 0;
                foreach (char ch in Text)
                {
                    if (!char.IsWhiteSpace(ch))
                    {
                        return index;
                    }
                    index++;
                }
                return Text.Length;
            }
        }

        public string Text { get; set; }

        public SourceCodeLine(string text)
        {
            Text = text;
        }

        public char GetCharacterAtIndex(int index) => index >= 0 && index < Text.Length ? Text[index] : ' ';

        public bool AtEndOfLine(int position) => position == Text.Length;

        public bool AtStartOfLine(int position) => position == 0;

        public void PartialIncreaseIndent(int position, int spaceAmount, out int shiftAmount)
        {
            if (spaceAmount >= SourceCode.TAB_REPLACEMENT.Length)
            {
                IncreaseIndentAtPosition(position, out shiftAmount);
                return;
            }
            shiftAmount = spaceAmount;
            InsertText(position, SourceCode.TAB_REPLACEMENT.Substring(0, spaceAmount));
        }

        public void IncreaseIndentAtPosition(int position, out int shiftAmount)
        {
            int indentSize = SourceCode.TAB_REPLACEMENT.Length;
            int rounded = (position + indentSize) / indentSize * indentSize;
            shiftAmount = rounded - position;
            InsertText(position, SourceCode.TAB_REPLACEMENT.Substring(0, shiftAmount));
        }

        public void DecreaseIndentAtPosition(int position, out int shiftAmount)
        {
            int indentSize = SourceCode.TAB_REPLACEMENT.Length;
            int rounded = (position - 1) / indentSize * indentSize;
            for (int index = position - 1; index >= rounded; index--)
            {
                if (!char.IsWhiteSpace(Text[index]))
                {
                    rounded = index + 1;
                    break;
                }
            }
            shiftAmount = position - rounded;
            Text = GetStringBeforePosition(rounded) + GetStringAfterPosition(position);
        }

        public void AppendText(string text) => InsertText(Text.Length, text);

        public void InsertText(int position, string text)
        {
            if (text.Contains(Environment.NewLine))
            {
                throw new Exception("Cannot insert text with a line break");
            }
            if (position > Text.Length)
            {
                Text = string.Concat(Text.PadRight(position), text);
            }
            else if (AtEndOfLine(position))
            {
                Text += text;
            }
            else
            {
                Text = string.Concat(GetStringBeforePosition(position), text, GetStringAfterPosition(position));
            }
        }

        public void InsertCharacter(int position, char character) => InsertText(position, character.ToString());

        public bool RemoveCharacterBefore(int position)
        {
            if (AtStartOfLine(position))
            {
                return false;
            }
            if (position > Text.Length)
            {
                return true;
            }
            if (AtEndOfLine(position))
            {
                Text = GetStringBeforePosition(position - 1);
            }
            else if (position == 1)
            {
                Text = GetStringAfterPosition(1);
            }
            else
            {
                Text = string.Concat(GetStringBeforePosition(position - 1), GetStringAfterPosition(position));
            }
            return true;
        }

        public bool RemoveCharacterAfter(int position)
        {
            if (AtEndOfLine(position))
            {
                return false;
            }
            if (AtStartOfLine(position))
            {
                Text = GetStringAfterPosition(1);
            }
            else if (position == Text.Length - 1)
            {
                Text = GetStringBeforePosition(position);
            }
            else
            {
                Text = string.Concat(GetStringBeforePosition(position), GetStringAfterPosition(position + 1));
            }
            return true;
        }

        public string GetStringBeforePosition(int position) => Text.Substring(0, position);

        public string GetStringAfterPosition(int position) => Text.Substring(position);
    }
}
