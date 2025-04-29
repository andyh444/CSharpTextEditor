using NTextEditor.Source;
using NTextEditor.UndoRedoActions;
using NTextEditor.View;

namespace NTextEditor.Languages.VisualBasic
{
    internal class VisualBasicSpecialCharacterHandler : ISpecialCharacterHandler
    {
        public void HandleLineBreakInserted(SourceCode sourceCode, SelectionRange activePosition, List<UndoRedoAction>? actionBuilder)
        {
        }

        public void HandleCharacterInserting(char character, SourceCode sourceCode)
        {
        }

        public void HandleCharacterInserted(char character, SourceCode sourceCode, ICodeCompletionHandler codeCompletionHandler, SyntaxPalette syntaxPalette)
        {
        }
    }
}
