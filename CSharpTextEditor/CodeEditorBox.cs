using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSharpTextEditor
{
    public partial class CodeEditorBox : UserControl
    {
        public CodeEditorBox()
        {
            InitializeComponent();
            HighlightSyntax();
        }

        public string GetText() => richTextBox1.Text;

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
            CSharpSyntaxHighlightingWalker h = new CSharpSyntaxHighlightingWalker((s, c) => currentHighlight.Add((s, c)), SyntaxPalette.GetLightModePalette());
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

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            HighlightSyntax();
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
    }
}
