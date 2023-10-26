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
            //HighlightSyntax();
        }

        

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            SyntaxTree tree = CSharpSyntaxTree.ParseText(codeEditorBox1.GetText());
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

}