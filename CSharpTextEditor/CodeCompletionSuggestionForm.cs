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
    internal partial class CodeCompletionSuggestionForm : Form
    {
        private CodeEditorBox2 editorBox;
        private SourceCodePosition? position;
        private CodeCompletionSuggestion[] suggestions;
        private Bitmap spannerIcon;
        private Bitmap methodIcon;

        protected override bool ShowWithoutActivation => false;

        public CodeCompletionSuggestionForm()
        {
            InitializeComponent();

            spannerIcon = Properties.Resources.spanner;
            methodIcon = Properties.Resources.box;
        }

        public SourceCodePosition? GetPosition() => position;

        public void SetEditorBox(CodeEditorBox2 editorBox)
        {
            this.editorBox = editorBox;
        }

        public void Show(IWin32Window owner, SourceCodePosition startPosition, IEnumerable<CodeCompletionSuggestion> suggestions)
        {
            position = startPosition;
            this.suggestions = suggestions.ToArray();
            PopulateSuggestions(suggestions);
            Show(owner);
        }

        public void PopulateSuggestions(IEnumerable<CodeCompletionSuggestion> suggestions)
        {
            listBox.Items.Clear();
            foreach (CodeCompletionSuggestion suggestion in suggestions.Distinct())
            {
                listBox.Items.Add(suggestion);
            }
            if (listBox.Items.Count > 0)
            {
                listBox.SelectedIndices.Add(0);
            }
        }

        public void MoveSelectionUp()
        {
            if (listBox.SelectedIndex > 0)
            {
                listBox.SelectedIndex--;
            }
        }

        public void MoveSelectionDown()
        {
            if (listBox.SelectedIndex < listBox.Items.Count - 1)
            {
                listBox.SelectedIndex++;
            }
        }

        public string GetSelectedItem()
        {
            if (listBox.SelectedIndex != -1)
            {
                return ((CodeCompletionSuggestion)listBox.Items[listBox.SelectedIndex]).Name.ToString();
            }
            throw new Exception("No item selected");
        }

        private void listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            editorBox?.Focus();
        }

        private void listBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = listBox.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                editorBox?.ChooseCodeCompletionItem(GetSelectedItem());
            }
        }

        internal void FilterSuggestions(string textLine, int columnNumber)
        {
            textLine = textLine.Substring(position.Value.ColumnNumber, columnNumber - position.Value.ColumnNumber);
            string lowerTextLine = textLine.ToLower();
            PopulateSuggestions(suggestions.Where(x => x.Name.ToLower().Contains(lowerTextLine)));
        }

        private void listBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index != -1)
            {
                CodeCompletionSuggestion item = (CodeCompletionSuggestion)listBox.Items[e.Index];
                e.DrawBackground();
                e.DrawFocusRectangle();
                Bitmap icon = null;
                if (item.SymbolType == SymbolType.Property)
                {
                    icon = spannerIcon;
                }

                else if (item.SymbolType == SymbolType.Method)
                {
                    icon = methodIcon;
                }
                if (icon != null)
                {
                    e.Graphics.DrawImage(icon, e.Bounds.Location);
                }
                e.Graphics.DrawString(item.Name, e.Font, Brushes.Black, new Point(e.Bounds.Location.X + 16, e.Bounds.Location.Y));
            }
        }
    }
}
