using CSharpTextEditor.CS;
using System.Collections.Generic;

namespace CSharpTextEditor
{
    public class CodeCompletionSuggestion
    {
        public string Name { get; }

        public SymbolType SymbolType { get; }

        public IToolTipSource ToolTipSource { get; }

        public CodeCompletionSuggestion(string name, SymbolType symbolType, IToolTipSource toolTipSource)
        {
            Name = name;
            SymbolType = symbolType;
            ToolTipSource = toolTipSource;
        }
    }
}
