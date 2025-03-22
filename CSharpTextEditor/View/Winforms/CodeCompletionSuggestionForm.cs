using CSharpTextEditor.Languages;
using CSharpTextEditor.Source;
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

namespace CSharpTextEditor.View.Winforms
{
    internal partial class CodeCompletionSuggestionForm : Form
    {
        private class State(SourceCodePosition position, CodeCompletionSuggestion[] suggestions, SyntaxPalette palette)
        {
            public SourceCodePosition Position { get; } = position;
            public CodeCompletionSuggestion[] Suggestions { get; } = suggestions;
            public SyntaxPalette Palette { get; } = palette;
        }

        private CodeEditorBox? editorBox;
        private State? state;

        protected override bool ShowWithoutActivation => false;

        public CodeCompletionSuggestionForm()
        {
            InitializeComponent();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            if (editorBox == null)
            {
                throw new CSharpTextEditorException("Should call SetEditorBox first");
            }
            if (!Visible)
            {
                toolTip1.Hide(editorBox);
            }
            base.OnVisibleChanged(e);
        }

        public SourceCodePosition GetPosition()
        {
            if (state == null)
            {
                throw new CSharpTextEditorException("Should call Show first");
            }
            return state.Position;
        }

        public void SetEditorBox(CodeEditorBox editorBox)
        {
            this.editorBox = editorBox;
        }

        public void Show(IWin32Window owner, SourceCodePosition startPosition, IEnumerable<CodeCompletionSuggestion> suggestions, SyntaxPalette syntaxPalette)
        {
            state = new State(startPosition, suggestions.ToArray(), syntaxPalette);
            PopulateSuggestions(suggestions);
            toolTip1.BackColor = syntaxPalette.ToolTipBackColour;
            listBox.BackColor = syntaxPalette.ToolTipBackColour;
            listBox.ForeColor = syntaxPalette.DefaultTextColour;

            Show(owner);
        }

        public void PopulateSuggestions(IEnumerable<CodeCompletionSuggestion> suggestions)
        {
            listBox.Items.Clear();
            foreach (IGrouping<string, CodeCompletionSuggestion> suggestion in suggestions.GroupBy(x => x.Name))
            {
                listBox.Items.Add(suggestion);
            }
            if (listBox.Items.Count > 0)
            {
                listBox.SelectedIndices.Add(0);
            }
        }

        private IGrouping<string, CodeCompletionSuggestion> GetItemAtSelectedIndex() => GetItemAtIndex(listBox.SelectedIndex);

        private IGrouping<string, CodeCompletionSuggestion> GetItemAtIndex(int index)
        {
            return (IGrouping<string, CodeCompletionSuggestion>)listBox.Items[index];
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
                var selected = GetItemAtSelectedIndex();
                return selected.Key;
            }
            throw new Exception("No item selected");
        }

        private void listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (editorBox == null)
            {
                throw new CSharpTextEditorException("Should call SetEditorBox first");
            }
            if (listBox.SelectedIndex != -1)
            {
                var selected = GetItemAtSelectedIndex();
                var point = editorBox.PointToClient(Location);

                CodeCompletionSuggestion suggestion = selected.First();
                (string toolTipText, _) = suggestion.ToolTipSource.GetToolTip();
                int overloadCount = selected.Count() - 1;
                if (overloadCount > 0)
                {
                    if (overloadCount == 1)
                    {
                        toolTipText += $" (+1 overload)";
                    }
                    else
                    {
                        toolTipText += $" (+{overloadCount} overloads)";
                    }
                }
                toolTip1.Show(toolTipText, editorBox, point.X + Width + 16, point.Y + 32);
            }
            editorBox?.Focus();

            //toolTip1.Show("Hello world", listBox);
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
            if (state == null)
            {
                throw new CSharpTextEditorException("Should call Show first");
            }
            textLine = textLine.Substring(state.Position.ColumnNumber, columnNumber - state.Position.ColumnNumber);
            string lowerTextLine = textLine.ToLower();
            IEnumerable<CodeCompletionSuggestion> filteredSuggestions = state.Suggestions.Where(x => x.Name.ToLower().Contains(lowerTextLine));
            if (!string.IsNullOrEmpty(lowerTextLine))
            {
                filteredSuggestions = filteredSuggestions.OrderBy(x => x.Name.ToLower().StartsWith(lowerTextLine) ? 0 : 1)
                                           .ThenBy(x => x.Name);
            }
            PopulateSuggestions(filteredSuggestions);
        }

        private void listBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index != -1)
            {
                var selected = GetItemAtIndex(e.Index);
                e.DrawBackground();
                e.DrawFocusRectangle();
                Bitmap? icon = IconCache.GetIcon(selected.First().SymbolType);
                if (icon != null)
                {
                    e.Graphics.DrawImage(icon, e.Bounds.Location);
                }
                using (Brush b = new SolidBrush(state?.Palette.DefaultTextColour ?? ForeColor))
                {
                    e.Graphics.DrawString(selected.Key, e.Font ?? Font, b, new Point(e.Bounds.Location.X + 16, e.Bounds.Location.Y));
                }
            }
        }

        private void toolTip1_Draw(object sender, DrawToolTipEventArgs e)
        {
            if (e.ToolTipText == null)
            {
                return;
            }
            e.DrawBackground();
            e.DrawBorder();
            ICanvas canvas = new WinformsCanvas(e.Graphics, Size.Empty, e.Font ?? Font);
            var selected = GetItemAtSelectedIndex();
            CodeCompletionSuggestion suggestion = selected.First();
            (_, List<SyntaxHighlighting> highlightings) = suggestion.ToolTipSource.GetToolTip(); 
            DrawingHelper.DrawTextLine(canvas, 0, e.ToolTipText, e.Bounds.X + 3, e.Bounds.Y + 1, highlightings, state?.Palette ?? SyntaxPalette.GetLightModePalette());
        }
    }
}
