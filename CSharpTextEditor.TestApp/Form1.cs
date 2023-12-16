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
            codeEditorBox21.UndoHistoryChanged += CodeEditorBox21_UndoHistoryChanged;
            comboBox1.SelectedIndex = 0;
        }

        private void CodeEditorBox21_UndoHistoryChanged(object? sender, EventArgs e)
        {
            (IEnumerable<string> undoItems, IEnumerable<string> redoItems) = codeEditorBox21.GetUndoAndRedoItems();
            undoButton.Enabled = undoItems.Any();
            redoButton.Enabled = redoItems.Any();
        }

        private void highlightButton_Click(object sender, EventArgs e)
        {
            //HighlightSyntax();
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem.ToString() == "Light")
            {
                codeEditorBox21.SetPalette(SyntaxPalette.GetLightModePalette());
            }
            else if (comboBox1.SelectedItem.ToString() == "Dark")
            {
                codeEditorBox21.SetPalette(SyntaxPalette.GetDarkModePalette());
            }
        }

        private void undoButton_Click(object sender, EventArgs e)
        {
            codeEditorBox21.Undo();
        }

        private void redoButton_Click(object sender, EventArgs e)
        {
            codeEditorBox21.Redo();
        }
    }

}