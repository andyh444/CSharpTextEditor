using CSharpTextEditor.Languages;
using CSharpTextEditor.Source;
using CSharpTextEditor.View;
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
    public partial class MainForm : Form
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

        public MainForm()
        {
            InitializeComponent();
            Console.SetOut(new TextBoxWriter(executionTextBox));
            codeEditorBox.UndoHistoryChanged += CodeEditorBox_UndoHistoryChanged;
            codeEditorBox.DiagnosticsChanged += CodeEditorBox_DiagnosticsChanged;
            paletteComboBox.SelectedIndex = 0;
            typeCombobox.SelectedIndex = 0;
        }

        private void CodeEditorBox_UndoHistoryChanged(object? sender, EventArgs e)
        {
            (IEnumerable<string> undoItems, IEnumerable<string> redoItems) = codeEditorBox.GetUndoAndRedoItems();
            undoButton.Enabled = undoItems.Any();
            redoButton.Enabled = redoItems.Any();
        }

        private void paletteComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (paletteComboBox.SelectedItem == null)
            {
                return;
            }
            if (paletteComboBox.SelectedItem.ToString() == "Light")
            {
                codeEditorBox.SetPalette(SyntaxPalette.GetLightModePalette());
            }
            else if (paletteComboBox.SelectedItem.ToString() == "Dark")
            {
                codeEditorBox.SetPalette(SyntaxPalette.GetDarkModePalette());
            }
        }

        private void undoButton_Click(object sender, EventArgs e)
        {
            codeEditorBox.Undo();
        }

        private void redoButton_Click(object sender, EventArgs e)
        {
            codeEditorBox.Redo();
        }

        private void CodeEditorBox_DiagnosticsChanged(object? sender, IReadOnlyCollection<SyntaxDiagnostic> e)
        {
            diagnosticsView.Items.Clear();
            foreach (SyntaxDiagnostic diagnostic in e)
            {
                ListViewItem item = new ListViewItem(diagnostic.Start.ToString());
                item.SubItems.Add(diagnostic.Id);
                item.SubItems.Add(diagnostic.Message);
                diagnosticsView.Items.Add(item);
            }
        }

        private void executeButton_Click(object sender, EventArgs e)
        {
            executionTextBox.Clear();
            codeEditorBox.Execute(Console.Out);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (typeCombobox.SelectedItem == null)
            {
                return;
            }
            if (typeCombobox.SelectedItem.ToString() == "Class Library")
            {
                codeEditorBox.SetLanguageToCSharp(true);
            }
            else if (typeCombobox.SelectedItem.ToString() == "Executable")
            {
                codeEditorBox.SetLanguageToCSharp(false);
            }
            executeButton.Enabled = codeEditorBox.CanExecuteCode();
        }

        private void diagnosticsView_DoubleClick(object sender, EventArgs e)
        {
            if (diagnosticsView.SelectedItems.Count == 1)
            {
                var item = diagnosticsView.SelectedItems[0];
                if (SourceCodePosition.TryParse(item.SubItems[0].Text, out SourceCodePosition? position)
                    && position != null)
                {
                    codeEditorBox.GoToPosition(position.Value.LineNumber, position.Value.ColumnNumber);
                    codeEditorBox.Focus();
                    codeEditorBox.Refresh();
                }
            }
        }
    }
}