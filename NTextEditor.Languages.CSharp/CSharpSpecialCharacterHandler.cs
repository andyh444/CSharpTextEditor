using CSharpTextEditor.Languages;
using CSharpTextEditor.Source;
using CSharpTextEditor.UndoRedoActions;
using CSharpTextEditor.View;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NTextEditor.Languages.CSharp
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
                    null);
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
                sourceCode.DecreaseIndentAtActivePosition();
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
                bool hideTooltip = true;
                if (character == ' '
                    || character == ',')
                {
                    hideTooltip = false;
                }
                codeCompletionHandler.HideCodeCompletionForm(hideTooltip);
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

            int characterPosition = head.GetPosition().ToCharacterIndex(sourceCode.Lines);
            if (characterPosition != -1)
            {
                var suggestions = _syntaxHighlighter.GetSuggestionsAtPosition(characterPosition, syntaxPalette, out int argumentIndex);
                if (suggestions.Any())
                {
                    codeCompletionHandler.ShowMethodCompletion(head.GetPosition(), suggestions, argumentIndex);
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
