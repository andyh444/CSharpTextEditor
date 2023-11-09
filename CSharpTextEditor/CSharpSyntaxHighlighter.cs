using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Runtime.Serialization;
using System.Runtime;
using System.Reflection;

namespace CSharpTextEditor
{
    public class CSharpSyntaxHighlighter : ISyntaxHighlighter
    {
        private Func<int, SourceCodePosition> _getLineAndColumnNumber;

        internal CSharpSyntaxHighlighter(Func<int, SourceCodePosition> getLineAndColumnNumber)
        {
            _getLineAndColumnNumber = getLineAndColumnNumber;
        }

        public SyntaxHighlightingCollection GetHighlightings(string sourceText, SyntaxPalette palette)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(sourceText);

            var dd = typeof(Enumerable).GetTypeInfo().Assembly.Location;
            var coreDir = Directory.GetParent(dd);

            var compilation = CSharpCompilation.Create("MyCompilation")
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(
                    MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Console).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Task).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(coreDir.FullName + Path.DirectorySeparatorChar + "mscorlib.dll"),
                    MetadataReference.CreateFromFile(coreDir.FullName + Path.DirectorySeparatorChar + "System.Runtime.dll"))
                .AddSyntaxTrees(tree);

            var diags = compilation.GetDiagnostics();

            SemanticModel semanticModel = compilation.GetSemanticModel(tree);

            List<SyntaxHighlighting> highlighting = new List<SyntaxHighlighting>();
            List<(SourceCodePosition start, SourceCodePosition end, string message)> errors = new List<(SourceCodePosition start, SourceCodePosition end, string message)>();

            foreach (var trivium in tree.GetRoot().DescendantTrivia())
            {
                if (trivium.IsKind(SyntaxKind.SingleLineCommentTrivia)
                    || trivium.IsKind(SyntaxKind.MultiLineCommentTrivia))
                {
                    AddSpanToHighlighting(trivium.Span, palette.CommentColour, highlighting);
                }
            }
            foreach (var diagnostic in tree.GetDiagnostics().Concat(compilation.GetDiagnostics()))
            {
                if (diagnostic.Severity == DiagnosticSeverity.Error)
                {
                    SourceCodePosition start = _getLineAndColumnNumber(diagnostic.Location.SourceSpan.Start);
                    SourceCodePosition end = _getLineAndColumnNumber(diagnostic.Location.SourceSpan.End);
                    errors.Add((start, end, $"{diagnostic.Id}: {diagnostic.GetMessage()}"));
                }
            }
            CSharpSyntaxHighlightingWalker highlighter = new CSharpSyntaxHighlightingWalker(semanticModel, (span, action) => AddSpanToHighlighting(span, action, highlighting), palette);
            highlighter.Visit(tree.GetRoot());
            return new SyntaxHighlightingCollection(highlighting.OrderBy(x => x.Start.LineNumber).ThenBy(x => x.Start.ColumnNumber).ToList(), errors);
        }

        public IEnumerable<(int start, int end)> GetSpansFromTextLine(string textLine)
        {
            foreach (SyntaxToken token in CSharpSyntaxTree.ParseText(textLine).GetRoot().DescendantTokens())
            {
                yield return (token.Span.Start, token.Span.End);
            }
        }

        private void AddSpanToHighlighting(TextSpan span, Color colour, List<SyntaxHighlighting> highlighting)
        {
            SourceCodePosition start = _getLineAndColumnNumber(span.Start);
            SourceCodePosition end = _getLineAndColumnNumber(span.End);
            highlighting.Add(new SyntaxHighlighting(start, end, colour));
        }

        private Color GetKeywordColour(SyntaxKind syntaxKind, SyntaxPalette palette)
        {
            if (syntaxKind == SyntaxKind.IfKeyword
                || syntaxKind == SyntaxKind.ElseKeyword
                || syntaxKind == SyntaxKind.ForEachKeyword
                || syntaxKind == SyntaxKind.ForKeyword
                || syntaxKind == SyntaxKind.WhileKeyword
                || syntaxKind == SyntaxKind.DoKeyword
                || syntaxKind == SyntaxKind.ReturnKeyword
                || syntaxKind == SyntaxKind.TryKeyword
                || syntaxKind == SyntaxKind.CatchKeyword
                || syntaxKind == SyntaxKind.FinallyKeyword
                || syntaxKind == SyntaxKind.SwitchKeyword
                || syntaxKind == SyntaxKind.CaseKeyword
                || syntaxKind == SyntaxKind.BreakKeyword)
            {
                return palette.PurpleKeywordColour;
            }
            return palette.BlueKeywordColour;
        }
    }
}
