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
        private MetadataReference[] references;
        private CSharpCompilation? _compilation;
        private SyntaxTree? _previousTree;

        internal CSharpSyntaxHighlighter(Func<int, SourceCodePosition> getLineAndColumnNumber)
        {
            _getLineAndColumnNumber = getLineAndColumnNumber;
            var dd = typeof(Enumerable).GetTypeInfo().Assembly.Location;
            var coreDir = Directory.GetParent(dd);
            references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Task).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(coreDir.FullName + Path.DirectorySeparatorChar + "mscorlib.dll"),
                MetadataReference.CreateFromFile(coreDir.FullName + Path.DirectorySeparatorChar + "System.Runtime.dll")
            };
        }


        public SyntaxHighlightingCollection GetHighlightings(string sourceText, SyntaxPalette palette)
        {
            List<string> timings = new List<string>();
            Stopwatch sw1 = Stopwatch.StartNew();
            SyntaxTree tree = CSharpSyntaxTree.ParseText(sourceText);
            sw1.Stop();
            timings.Add($"ParseText took {sw1.Elapsed.TotalMilliseconds} ms");
            sw1.Restart();
            
            if (_compilation != null
                && _previousTree != null)
            {
                _compilation = _compilation.ReplaceSyntaxTree(_previousTree, tree);
            }
            else
            {
                _compilation = CSharpCompilation.Create("MyCompilation")
                    .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                    .AddReferences(references)
                    .AddSyntaxTrees(tree);
            }
            _previousTree = tree;

            sw1.Stop();
            timings.Add($"create compilation took {sw1.Elapsed.TotalMilliseconds} ms");
            sw1.Restart();
            SemanticModel semanticModel = _compilation.GetSemanticModel(tree);
            sw1.Stop();
            timings.Add($"getsemanticmodel took {sw1.Elapsed.TotalMilliseconds} ms");
            sw1.Restart();
            List<(int, int)> blockLines = new List<(int, int)>();
            List<SyntaxHighlighting> highlighting = new List<SyntaxHighlighting>();
            List<(SourceCodePosition start, SourceCodePosition end, string message)> errors = new List<(SourceCodePosition start, SourceCodePosition end, string message)>();

            foreach (var trivium in tree.GetRoot().DescendantTrivia())
            {
                // comments don't get visited by the syntax walker
                if (trivium.IsKind(SyntaxKind.SingleLineCommentTrivia)
                    || trivium.IsKind(SyntaxKind.MultiLineCommentTrivia))
                {
                    AddSpanToHighlighting(trivium.Span, palette.CommentColour, highlighting);
                }
            }
            foreach (var diagnostic in tree.GetDiagnostics().Concat(_compilation.GetDiagnostics()))
            {
                if (diagnostic.Severity == DiagnosticSeverity.Error)
                {
                    SourceCodePosition start = _getLineAndColumnNumber(diagnostic.Location.SourceSpan.Start);
                    SourceCodePosition end = _getLineAndColumnNumber(diagnostic.Location.SourceSpan.End);
                    errors.Add((start, end, $"{diagnostic.Id}: {diagnostic.GetMessage()}"));
                }
            }
            sw1.Stop();
            timings.Add($"iterate over errors and descendent trivia took {sw1.Elapsed.TotalMilliseconds} ms");
            sw1.Restart();
            CSharpSyntaxHighlightingWalker highlighter = new CSharpSyntaxHighlightingWalker(semanticModel,
                (span, action) => AddSpanToHighlighting(span, action, highlighting),
                (span) => blockLines.Add((_getLineAndColumnNumber(span.Start).LineNumber, _getLineAndColumnNumber(span.End).LineNumber)),
                palette);
            highlighter.Visit(tree.GetRoot());
            sw1.Stop();
            timings.Add($"walking syntax took {sw1.Elapsed.TotalMilliseconds} ms");

            return new SyntaxHighlightingCollection(highlighting.OrderBy(x => x.Start.LineNumber).ThenBy(x => x.Start.ColumnNumber).ToList(), errors, blockLines);
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
    }
}
