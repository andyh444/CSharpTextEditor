using System.Collections.Generic;

namespace CSharpTextEditor
{
    public class CodeCompletionSuggestion
    {
        public string Name { get; }

        public SymbolType SymbolType { get; }

        public string ToolTipText { get; }

        public IEnumerable<SyntaxHighlighting> Highlightings { get; }

        public CodeCompletionSuggestion(string name, SymbolType symbolType, string toolTipText, IEnumerable<SyntaxHighlighting> highlightings)
        {
            Name = name;
            SymbolType = symbolType;
            ToolTipText = toolTipText;
            Highlightings = highlightings;
        }
    }
}
