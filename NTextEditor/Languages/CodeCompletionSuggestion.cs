using NTextEditor.View.ToolTips;
using System;

namespace NTextEditor.Languages
{
    public class CodeCompletionSuggestion : IEquatable<CodeCompletionSuggestion>
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

        public bool Equals(CodeCompletionSuggestion? other)
        {
            return other is not null &&
                   Name == other.Name &&
                   SymbolType == other.SymbolType &&
                   IsDeclaration == other.IsDeclaration;
        }
    }
}
