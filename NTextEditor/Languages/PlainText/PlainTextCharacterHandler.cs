using NTextEditor.Source;
using NTextEditor.UndoRedoActions;
using NTextEditor.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTextEditor.Languages.PlainText
{
    internal class PlainTextCharacterHandler : ISpecialCharacterHandler
    {
        void ISpecialCharacterHandler.HandleCharacterInserted(char character, SourceCode sourceCode, ICodeCompletionHandler codeCompletionHandler, SyntaxPalette syntaxPalette)
        {
        }

        void ISpecialCharacterHandler.HandleCharacterInserting(char character, SourceCode sourceCode)
        {
        }

        void ISpecialCharacterHandler.HandleLineBreakInserted(SourceCode sourceCode, SelectionRange activePosition, List<UndoRedoAction>? actionBuilder)
        {
        }
    }
}
