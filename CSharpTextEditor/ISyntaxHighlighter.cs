using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpTextEditor
{
    public enum SymbolType
    {
        None,
        Method,
        Property
    }

    public class CodeCompletionSuggestion
    {
        public string Name { get; }

        public SymbolType SymbolType { get; }

        public CodeCompletionSuggestion(string name, SymbolType symbolType)
        {
            Name = name;
            SymbolType = symbolType;
        }
    }

    public interface ISyntaxHighlighter
    {
        SyntaxHighlightingCollection GetHighlightings(string sourceText, SyntaxPalette palette);

        IEnumerable<(int start, int end)> GetSpansFromTextLine(string textLine);

        IEnumerable<CodeCompletionSuggestion> GetCodeCompletionSuggestions(string textLine, int position);
    }
}
