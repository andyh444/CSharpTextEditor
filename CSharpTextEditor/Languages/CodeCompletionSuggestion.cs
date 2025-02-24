using CSharpTextEditor.View;

namespace CSharpTextEditor.Languages
{
    public class CodeCompletionSuggestion
    {
        public string Name { get; }

        public SymbolType SymbolType { get; }

        public IToolTipSource ToolTipSource { get; }

        public bool IsDeclaration { get; }

        public CodeCompletionSuggestion(string name, SymbolType symbolType, IToolTipSource toolTipSource, bool isDeclaration)
        {
            Name = name;
            SymbolType = symbolType;
            ToolTipSource = toolTipSource;
            IsDeclaration = isDeclaration;
        }
    }
}
