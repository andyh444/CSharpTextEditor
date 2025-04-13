using NTextEditor.Source;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTextEditor.Languages
{
    public class SyntaxDiagnostic(SourceCodePosition start, SourceCodePosition end, string id, string message)
    {
        public SourceCodePosition Start { get; } = start;

        public SourceCodePosition End { get; } = end;

        public string Id { get; } = id;

        public string Message { get; } = message;

        public string ToFullString() => $"{Id}: {Message}";
    }
}
