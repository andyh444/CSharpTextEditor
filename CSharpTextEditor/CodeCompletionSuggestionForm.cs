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

        public void Show(IWin32Window owner, SourceCodePosition startPosition)
        {
            position = startPosition;
            Show(owner);
        }

        public void PopulateSuggestions(IEnumerable<string> suggestions)
        {
            listBox.Items.Clear();
            foreach (string suggestion in suggestions)
            {
                listBox.Items.Add(suggestion);
            }
            listBox.SelectedIndex = 0;
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
    }
}
