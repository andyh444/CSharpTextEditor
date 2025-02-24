using CSharpTextEditor.Source;
using CSharpTextEditor.UndoRedoActions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CSharpTextEditor.Languages.CS
{
    internal class CSharpSpecialCharacterHandler : ISpecialCharacterHandler
    {
        private CSharpSyntaxHighlighter _syntaxHighlighter;

        public CSharpSpecialCharacterHandler(CSharpSyntaxHighlighter syntaxHighlighter)
        {
            _syntaxHighlighter = syntaxHighlighter;
        }

        public void HandleLineBreakInserted(SourceCode sourceCode, SelectionRange activePosition, List<UndoRedoAction>? actionBuilder)
        {
            if (activePosition.Head.Line.Previous != null)
            {
                activePosition.InsertStringAtActivePosition(
                    GetWhiteSpaceAtBeginningOfLine(activePosition.Head.Line.Previous.Value.Text),
                    sourceCode,
                    actionBuilder,
                    null,
                    false);
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
                HandleMethodInvocationGrammar(sourceCode, codeCompletionHandler, syntaxPalette);
            }
        }

        private void HandleMethodInvocationGrammar(SourceCode sourceCode, ICodeCompletionHandler codeCompletionHandler, SyntaxPalette syntaxPalette)
        {
            if (sourceCode.SelectionRangeCollection.Count != 1)
            {
                // don't do anything for multi-caret select
                return;
            }
            Cursor head = sourceCode.SelectionRangeCollection.PrimarySelectionRange.Head.Clone();

            bool shiftSuccess = BacktrackCursorToMethodStartAndCountParameters(head, out int parameterCount);
            if (shiftSuccess)
            {
                // shift one more, because the characterIndex needs to be one previously for the C# syntax highlighter
                if (head.ShiftOneCharacterToTheLeft())
                {
                    int characterPosition = head.GetPosition().ToCharacterIndex(sourceCode.Lines);
                    if (characterPosition != -1)
                    {
                        CodeCompletionSuggestion? suggestion = _syntaxHighlighter.GetSuggestionAtPosition(characterPosition, syntaxPalette);
                        if (suggestion != null)
                        {
                            codeCompletionHandler.ShowMethodCompletion(head.GetPosition(), suggestion, parameterCount);
                        }
                    }
                    else
                    {
                        // this will currently hide the method tool tip too. Maybe we don't want that
                        codeCompletionHandler.HideCodeCompletionForm(true);
                    }
                }
            }
        }

        private static bool BacktrackCursorToMethodStartAndCountParameters(Cursor head, out int parameterIndex)
        {
            // back track until the opening bracket is found
            bool shiftSuccess = true;
            parameterIndex = 0;
            int originalLineNumber = head.LineNumber;
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
            return shiftSuccess;
        }
    }
}
