using System.IO;
using System.Linq;
using System.Text;

namespace CSharpTextEditor.CS
{
    internal class CSharpSpecialCharacterHandler : ISpecialCharacterHandler
    {
        private CSharpSyntaxHighlighter _syntaxHighlighter;

        public CSharpSpecialCharacterHandler(CSharpSyntaxHighlighter syntaxHighlighter)
        {
            _syntaxHighlighter = syntaxHighlighter;
        }

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

        public void HandleCharacterInserted(char character, SourceCode sourceCode, ICodeCompletionHandler codeCompletionHandler, SyntaxPalette syntaxPalette)
        {
            if (character == '.')
            {
                if (sourceCode.SelectionRangeCollection.Count == 1)
                {
                    codeCompletionHandler.ShowCodeCompletionForm();
                }
            }
            else if (!char.IsLetterOrDigit(character))
            {
                codeCompletionHandler.HideCodeCompletionForm(character != ' ');
            }

            if (character == '('
                || character == ',')
            {
                if (sourceCode.SelectionRangeCollection.Count == 1)
                {
                    Cursor head = sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head.Clone();
                    int originalLineNumber = head.LineNumber;

                    // back track until the opening bracket is found
                    bool shiftSuccess = true;
                    int parameterIndex = 0;
                    while (shiftSuccess
                        && head.Line.Value.GetCharacterAtIndex(head.ColumnNumber) != '('
                        && head.LineNumber == originalLineNumber)
                    {
                        if (head.Line.Value.GetCharacterAtIndex(head.ColumnNumber) == ',')
                        {
                            parameterIndex++;
                        }
                        shiftSuccess = head.ShiftOneCharacterToTheLeft();
                    }
                    if (shiftSuccess)
                    {
                        // shift one more, because the characterIndex needs to be one previously for the C# syntax highlighter
                        if (head.ShiftOneCharacterToTheLeft())
                        {
                            int characterPosition = head.GetPosition().ToCharacterIndex(sourceCode.Lines);
                            if (characterPosition != -1)
                            {
                                CodeCompletionSuggestion suggestion = _syntaxHighlighter.GetSuggestionAtPosition(characterPosition, syntaxPalette);
                                codeCompletionHandler.ShowMethodCompletion(head.GetPosition(), suggestion, parameterIndex);
                            }
                            else
                            {
                                // this will currently hide the method tool tip too. Maybe we don't want that
                                codeCompletionHandler.HideCodeCompletionForm(true);
                            }
                        }
                    }
                }
            }
        }
    }
}
