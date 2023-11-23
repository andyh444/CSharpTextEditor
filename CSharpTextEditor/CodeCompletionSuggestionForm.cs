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
    public partial class CodeCompletionSuggestionForm : Form
    {
        private CodeEditorBox2? editorBox;
        private SourceCodePosition? position;
        private string[] suggestions;

        protected override bool ShowWithoutActivation => false;

        public CodeCompletionSuggestionForm()
        {
            InitializeComponent();
        }

        public SourceCodePosition? GetPosition() => position;

        public void SetEditorBox(CodeEditorBox2 editorBox)
        {
            this.editorBox = editorBox;
        }

        public void Show(IWin32Window owner, SourceCodePosition startPosition, IEnumerable<string> suggestions)
        {
            position = startPosition;
            this.suggestions = suggestions.ToArray();
            PopulateSuggestions(suggestions);
            Show(owner);
        }

        public void PopulateSuggestions(IEnumerable<string> suggestions)
        {
            listBox.Items.Clear();
            foreach (string suggestion in suggestions.Distinct())
            {
                listBox.Items.Add(suggestion);
            }
            if (listBox.Items.Count > 0)
            {
                listBox.SelectedIndex = 0;
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
                return listBox.Items[listBox.SelectedIndex].ToString();
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
            PopulateSuggestions(suggestions.Where(x => x.Contains(textLine)));
        }
    }
}
