using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor
{
    public class SourceCodeLine
    {
        public string Text { get; set; }

        public SourceCodeLine(string text)
        {
            Text = text;
        }

        public bool AtEndOfLine(int position) => position == Text.Length;

        public bool AtStartOfLine(int position) => position == 0;

        public void IncreaseIndent() => Text = SourceCode.TAB_REPLACEMENT + Text;

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
