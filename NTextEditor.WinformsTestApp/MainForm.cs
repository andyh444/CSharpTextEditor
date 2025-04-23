using NTextEditor.Languages.CSharp;
using NTextEditor.Languages;
using NTextEditor.Source;
using NTextEditor.View;
using Microsoft.CodeAnalysis;
using System.Text;
using NTextEditor.Languages.PlainText;

namespace NTextEditor.WinformsTestApp
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
                if (TextBox.InvokeRequired)
                {
                    TextBox.Invoke(new Action(() => Write(value)));
                    return;
                }
                TextBox.Text += value;
                TextBox.SelectionStart = TextBox.Text.Length;
                TextBox.Update();
            }

            public override void Write(char value)
            {
                if (TextBox.InvokeRequired)
                {
                    TextBox.Invoke(new Action(() => Write(value)));
                    return;
                }
                TextBox.Text += value;
                TextBox.SelectionStart = TextBox.Text.Length;
                TextBox.Update();
            }

            public override void WriteLine(string? value)
            {
                if (TextBox.InvokeRequired)
                {
                    TextBox.Invoke(new Action(() => WriteLine(value)));
                    return;
                }
                TextBox.Text += value + Environment.NewLine;
                TextBox.SelectionStart = TextBox.Text.Length;
                TextBox.Update();
            }
        }

        private class TextBoxReader : TextReader
        {
            private readonly Queue<TaskCompletionSource<int>> _completionSourceQueue;

            public TextBox TextBox { get; }

            public TextBoxReader(TextBox textBox)
            {
                TextBox = textBox;
                textBox.TextChanged += TextBox_TextChanged;

                _completionSourceQueue = new Queue<TaskCompletionSource<int>>();
            }

            private void TextBox_TextChanged(object? sender, EventArgs e)
            {
                if (_completionSourceQueue.Any()
                    && !string.IsNullOrEmpty(TextBox.Text))
                {
                    // This assumes that the text has changed because a new character was typed in, AND that the new character was at the end
                    // TODO: Something better
                    int newCharacter = TextBox.Text.Last();
                    _completionSourceQueue.Dequeue().SetResult(newCharacter);
                }
            }

            public override int Read()
            {
                var completionSource = new TaskCompletionSource<int>();
                _completionSourceQueue.Enqueue(completionSource);
                int result = completionSource.Task.GetAwaiter().GetResult();
                return result;
            }
        }

        public MainForm()
        {
            InitializeComponent();
            Console.SetOut(new TextBoxWriter(executionTextBox));
            Console.SetIn(new TextBoxReader(executionTextBox));
            codeEditorBox.UndoHistoryChanged += CodeEditorBox_UndoHistoryChanged;
            codeEditorBox.DiagnosticsChanged += CodeEditorBox_DiagnosticsChanged;

            int paletteStartIndex = 0;
#if NET9_0_OR_GREATER
#pragma warning disable WFO5001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            if (Application.IsDarkModeEnabled)
            {
                paletteStartIndex = 1;
            }
#pragma warning restore WFO5001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#endif

            paletteComboBox.SelectedIndex = paletteStartIndex;
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

        private async void executeButton_Click(object sender, EventArgs e)
        {
            executionTextBox.Clear();
            await Task.Run(() => codeEditorBox.Execute(Console.Out));
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (typeCombobox.SelectedItem == null)
            {
                return;
            }
            if (typeCombobox.SelectedItem.ToString() == "C# Class Library")
            {
                codeEditorBox.SetLanguageToCSharp(true);
            }
            else if (typeCombobox.SelectedItem.ToString() == "C# Executable")
            {
                codeEditorBox.SetLanguageToCSharp(false);
            }
            else if (typeCombobox.SelectedItem.ToString() == "PlainText")
            {
                codeEditorBox.SetLanguageToPlainText();
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