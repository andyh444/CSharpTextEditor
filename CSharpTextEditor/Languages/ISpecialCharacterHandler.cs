using NTextEditor.Source;
using NTextEditor.UndoRedoActions;
using NTextEditor.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTextEditor.Languages
{
    public interface ISpecialCharacterHandler
    {
        internal void HandleLineBreakInserted(SourceCode sourceCode, SelectionRange activePosition, List<UndoRedoAction>? actionBuilder);

        /// <summary>
        /// Called before a character gets inserted, including in multi-caret mode
        /// </summary>
        internal void HandleCharacterInserting(char character, SourceCode sourceCode);

        /// <summary>
        /// Called after a character gets inserted apart from in multi-caret mode
        /// </summary>
        internal void HandleCharacterInserted(char character, SourceCode sourceCode, ICodeCompletionHandler codeCompletionHandler, SyntaxPalette syntaxPalette);
    }
}
