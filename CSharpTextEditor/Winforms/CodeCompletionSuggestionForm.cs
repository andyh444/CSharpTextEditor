using CSharpTextEditor.Winforms;
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
        private CodeEditorBox editorBox;
        private SourceCodePosition? position;
        private CodeCompletionSuggestion[] suggestions;
        private readonly Bitmap spannerIcon;
        private readonly Bitmap methodIcon;
        private readonly Bitmap bracketsIcon;
        private readonly Bitmap classIcon;
        private readonly Bitmap interfaceIcon;
        private readonly Bitmap fieldIcon;
        private readonly Bitmap localIcon;
        private readonly Bitmap structIcon;
        private readonly Bitmap enumMemberIcon;
        private readonly Bitmap constantIcon;
        private SyntaxPalette _palette;

        protected override bool ShowWithoutActivation => false;

        public CodeCompletionSuggestionForm()
        {
            InitializeComponent();
            spannerIcon = Properties.Resources.spanner;
            methodIcon = Properties.Resources.box;
            bracketsIcon = Properties.Resources.brackets;
            classIcon = Properties.Resources._class;
            interfaceIcon = Properties.Resources._interface;
            fieldIcon = Properties.Resources.field;
            localIcon = Properties.Resources.local;
            structIcon = Properties.Resources._struct;
            enumMemberIcon = Properties.Resources.enumMember;
            constantIcon = Properties.Resources.constant;
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            if (!Visible)
            {
                toolTip1.Hide(editorBox);
            }
            base.OnVisibleChanged(e);
        }

        public SourceCodePosition? GetPosition() => position;

        public void SetEditorBox(CodeEditorBox editorBox)
        {
            this.editorBox = editorBox;
        }

        public void Show(IWin32Window owner, SourceCodePosition startPosition, IEnumerable<CodeCompletionSuggestion> suggestions, SyntaxPalette syntaxPalette)
        {
            position = startPosition;
            this.suggestions = suggestions.ToArray();
            PopulateSuggestions(suggestions);
            toolTip1.BackColor = syntaxPalette.ToolTipBackColour;
            listBox.BackColor = syntaxPalette.ToolTipBackColour;
            listBox.ForeColor = syntaxPalette.DefaultTextColour;

            Show(owner);
            _palette = syntaxPalette;
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
            textLine = textLine.Substring(position.Value.ColumnNumber, columnNumber - position.Value.ColumnNumber);
            string lowerTextLine = textLine.ToLower();
            IEnumerable<CodeCompletionSuggestion> filteredSuggestions = suggestions.Where(x => x.Name.ToLower().Contains(lowerTextLine));
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
                Bitmap icon = GetIconFromSymbolType(selected.First().SymbolType);
                if (icon != null)
                {
                    e.Graphics.DrawImage(icon, e.Bounds.Location);
                }
                using (Brush b = new SolidBrush(_palette?.DefaultTextColour ?? ForeColor))
                {
                    e.Graphics.DrawString(selected.Key, e.Font, b, new Point(e.Bounds.Location.X + 16, e.Bounds.Location.Y));
                }
            }
        }

        private Bitmap GetIconFromSymbolType(SymbolType symbolType)
        {
            Bitmap icon = null;
            if (symbolType == SymbolType.Property)
            {
                icon = spannerIcon;
            }
            else if (symbolType == SymbolType.Method)
            {
                icon = methodIcon;
            }
            else if (symbolType == SymbolType.Namespace)
            {
                icon = bracketsIcon;
            }
            else if (symbolType == SymbolType.Class)
            {
                icon = classIcon;
            }
            else if (symbolType == SymbolType.Interface)
            {
                icon = interfaceIcon;
            }
            else if (symbolType == SymbolType.Field)
            {
                icon = fieldIcon;
            }
            else if (symbolType == SymbolType.Local)
            {
                icon = localIcon;
            }
            else if (symbolType == SymbolType.Struct)
            {
                icon = structIcon;
            }
            else if (symbolType == SymbolType.EnumMember)
            {
                icon = enumMemberIcon;
            }
            else if (symbolType == SymbolType.Constant)
            {
                icon = constantIcon;
            }
            return icon;
        }

        private void toolTip1_Draw(object sender, DrawToolTipEventArgs e)
        {
            e.DrawBackground();
            e.DrawBorder();
            var selected = GetItemAtSelectedIndex();
            CodeCompletionSuggestion suggestion = selected.First();
            (_, List<SyntaxHighlighting> highlightings) = suggestion.ToolTipSource.GetToolTip(); 
            Func<int, int> getXCoordinate = characterIndex => e.Bounds.X + 3 + DrawingHelper.GetStringSize(e.ToolTipText.Substring(0, characterIndex), e.Font, e.Graphics).Width;
            DrawingHelper.DrawLine(e.Graphics, 0, e.ToolTipText, e.Bounds.Y + 1, e.Font, highlightings, getXCoordinate, _palette ?? SyntaxPalette.GetLightModePalette());
        }
    }
}
