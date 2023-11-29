namespace CSharpTextEditor
{
    public class CodeCompletionSuggestion
    {
        public int a;

        public string Name { get; }

        public SymbolType SymbolType { get; }

        public string ToolTipText { get; }

        public CodeCompletionSuggestion(string name, SymbolType symbolType, string toolTipText)
        {
            Name = name;
            SymbolType = symbolType;
            ToolTipText = toolTipText;
        }
    }
}
