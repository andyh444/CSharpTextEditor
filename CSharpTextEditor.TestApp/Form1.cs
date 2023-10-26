using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace CSharpTextEditor.TestApp
{
    public partial class Form1 : Form
    {
        private class TextBoxWriter : TextWriter
        {
            public override Encoding Encoding => Encoding.Default;

            public TextBox TextBox { get; }

            public TextBoxWriter(TextBox textBox)
            {
                TextBox = textBox;
            }

            public override void Write(string? value)
            {
                TextBox.Text += value;
            }

            public override void Write(char value)
            {
                TextBox.Text += value;
            }

            public override void WriteLine(string? value)
            {
                TextBox.Text += value + Environment.NewLine;
            }
        }


        public Form1()
        {
            InitializeComponent();
            Console.SetOut(new TextBoxWriter(textBox1));
        }

        private void highlightButton_Click(object sender, EventArgs e)
        {
            HighlightSyntax();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            HighlightSyntax();
        }

        private Color GetKeywordColour(SyntaxKind syntaxKind)
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
                || syntaxKind == SyntaxKind.FinallyKeyword)
            {
                return Color.Purple;
            }
            return Color.Blue;
        }

        private void HighlightSyntax()
        {
            richTextBox1.BeginUpdate();
            SyntaxTree tree = CSharpSyntaxTree.ParseText(richTextBox1.Text);
            var compilation = CSharpCompilation.Create("MyCompilation")
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location), // Reference to mscorlib
                               MetadataReference.CreateFromFile(typeof(Console).Assembly.Location))   // Reference to System.Console)
                .AddSyntaxTrees(tree);
            var semanticModel = compilation.GetSemanticModel(tree);

            int cursorPosition = richTextBox1.SelectionStart;
            IEnumerable<SyntaxToken> tokens = tree.GetRoot().DescendantTokens().ToList();
            var trivia = tree.GetRoot().DescendantTrivia().ToList();
            HashSet<(TextSpan, Color)> currentHighlight = new HashSet<(TextSpan, Color)>();
            foreach (var token in tokens)
            {
                if (token.IsKeyword())
                {
                    currentHighlight.Add((token.Span, GetKeywordColour(token.Kind())));
                    //HighlightSyntax(token.Span, Color.Blue);
                }
                if (token.IsKind(SyntaxKind.StringLiteralToken)
                    || token.IsKind(SyntaxKind.InterpolatedStringStartToken)
                    || token.IsKind(SyntaxKind.InterpolatedStringTextToken)
                    || token.IsKind(SyntaxKind.InterpolatedStringEndToken))
                {
                    HighlightSyntax(token.Span, Color.DarkRed);
                    currentHighlight.Add((token.Span, Color.DarkRed));
                }
                if (token.IsVerbatimIdentifier())
                {
                    Debugger.Break();
                }
            }
            foreach (var trivium in tree.GetRoot().DescendantTrivia())
            {
                if (trivium.IsKind(SyntaxKind.SingleLineCommentTrivia)
                    || trivium.IsKind(SyntaxKind.MultiLineCommentTrivia))
                {
                    //HighlightSyntax(trivium.Span, Color.Green);
                    currentHighlight.Add((trivium.Span, Color.Green));
                }
            }
            SyntaxHighlighter h = new SyntaxHighlighter(tree, (s, c) => currentHighlight.Add((s, c)));
            h.Visit(tree.GetRoot());
            foreach (var diagnostic in tree.GetDiagnostics())
            {
                //richTextBox1.Select(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length);
                //richTextBox1.SelectionColor = Color.Red;
                //currentHighlight.Add((diagnostic.Location.SourceSpan, Color.Red));
            }

            richTextBox1.SelectAll();
            richTextBox1.SelectionColor = richTextBox1.ForeColor;
            foreach ((TextSpan span, Color colour) in currentHighlight)
            {
                HighlightSyntax(span, colour);
            }
            //previousHighlight = highlightsToAdd;
            richTextBox1.DeselectAll();
            richTextBox1.Select(cursorPosition, 0);
            richTextBox1.EndUpdate();
        }

        private void HighlightSyntax(TextSpan span, Color colour)
        {
            richTextBox1.DeselectAll();
            richTextBox1.Select(span.Start, span.Length);
            richTextBox1.SelectionColor = colour;
        }

        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int cursorPosition = richTextBox1.SelectionStart;
                int charIndex = richTextBox1.GetFirstCharIndexOfCurrentLine();
                StringBuilder sb = new StringBuilder(Environment.NewLine);
                while (charIndex < richTextBox1.Text.Length
                    && richTextBox1.Text[charIndex++] == '\t')
                {
                    sb.Append("\t");
                }
                if (GetPreviousCharOnLine(cursorPosition, out char ch)
                    && ch == '{')
                {
                    sb.Append("\t");
                }
                string result = sb.ToString();
                richTextBox1.Text = richTextBox1.Text.Insert(cursorPosition, result);
                richTextBox1.Select(cursorPosition + result.Length - 1, 0);
                e.Handled = true;
            }
        }

        private bool GetPreviousCharOnLine(int position, out char ch)
        {
            ch = default;
            while (--position >= 0)
            {
                ch = richTextBox1.Text[position];
                //if (char.IsWhiteSpace(ch))
                if (ch == '\t'
                    || ch == ' ')
                {
                    continue;
                }
                return true;
            }
            return false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            SyntaxTree tree = CSharpSyntaxTree.ParseText(richTextBox1.Text);
            var compilation = CSharpCompilation.Create("MyCompilation")
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location), // Reference to mscorlib
                               MetadataReference.CreateFromFile(typeof(decimal).Assembly.Location),
                               MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Runtime")).Location),
                               MetadataReference.CreateFromFile(typeof(Console).Assembly.Location))   // Reference to System.Console)
                .AddSyntaxTrees(tree);
            var semanticModel = compilation.GetSemanticModel(tree);
            using MemoryStream ms = new MemoryStream();
            EmitResult emitResult = compilation.Emit(ms);
            if (!emitResult.Success)
            {
                foreach (var diagnostic in emitResult.Diagnostics)
                {
                    Console.WriteLine(diagnostic);
                }
            }
            else
            {
                Assembly assembly = Assembly.Load(ms.ToArray());
                Type? type = assembly.GetType("Program");
                if (type != null)
                {
                    var mainMethod = type.GetMethod("Main");
                    if (mainMethod != null)
                    {
                        try
                        {
                            mainMethod.Invoke(null, null);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Exception found during execution: {ex.Message}, {ex}");
                        }
                    }
                }
            }
        }
    }

    public class SyntaxHighlighter : CSharpSyntaxWalker
    {
        private readonly SyntaxTree syntaxTree;
        private readonly Action<TextSpan, Color> highlightAction;

        public SyntaxHighlighter(SyntaxTree syntaxTree, Action<TextSpan, Color> highlightAction)
        {
            this.syntaxTree = syntaxTree;
            this.highlightAction = highlightAction;
        }

        [Obsolete]
        public void Highlight()
        {
            Visit(syntaxTree.GetRoot());
        }

        public override void VisitAttribute(AttributeSyntax node)
        {
            base.VisitAttribute(node);
            highlightAction(node.Name.Span, Color.SteelBlue);
        }

        public override void VisitStructDeclaration(StructDeclarationSyntax node)
        {
            highlightAction(node.Identifier.Span, Color.SteelBlue);
            base.VisitStructDeclaration(node);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            // Highlight class names
            highlightAction(node.Identifier.Span, Color.SteelBlue);
            if (node.BaseList != null)
            {
                foreach (var baseType in node.BaseList.Types)
                {
                    highlightAction(baseType.Span, Color.SteelBlue);
                }
            }
            base.VisitClassDeclaration(node);
        }

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            base.VisitConstructorDeclaration(node);
            highlightAction(node.Identifier.Span, Color.SteelBlue);
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            // Highlight method names
            highlightAction(node.Identifier.Span, Color.FromArgb(136, 108, 64));
            foreach (ParameterSyntax parameter in node.ParameterList.Parameters)
            {
                if (parameter.Type is IdentifierNameSyntax ins)
                {
                    highlightAction(ins.FullSpan, Color.SteelBlue);
                }
            }
            base.VisitMethodDeclaration(node);
        }

        public override void VisitArgumentList(ArgumentListSyntax node)
        {
            base.VisitArgumentList(node);
        }

        public override void VisitArgument(ArgumentSyntax node)
        {
            base.VisitArgument(node);
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            //highlightAction(node.Expression.Span, Color.FromArgb(136, 108, 64));
            base.VisitInvocationExpression(node);
            if (node.Expression is MemberAccessExpressionSyntax syntax)
            {
                highlightAction(syntax.Name.FullSpan, Color.FromArgb(136, 108, 64));
                //if (syntax.Expression is IdentifierNameSyntax name)
                {
                    //highlightAction(name.FullSpan, Color.Blue);
                }

            }
            else if (node.Expression is IdentifierNameSyntax syntax1)
            {
                highlightAction(syntax1.Identifier.FullSpan, Color.FromArgb(136, 108, 64));
            }
        }

        public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            base.VisitAssignmentExpression(node);
        }

        public override void VisitDeclarationExpression(DeclarationExpressionSyntax node)
        {
            base.VisitDeclarationExpression(node);
        }

        public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            base.VisitVariableDeclaration(node);
            //node.
        }
    }
}