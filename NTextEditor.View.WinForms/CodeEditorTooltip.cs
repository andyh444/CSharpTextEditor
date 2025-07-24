using NTextEditor.View.ToolTips;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NTextEditor.View.Winforms
{
    public partial class CodeEditorTooltip : Form
    {
        private SyntaxPalette? _palette;
        private IToolTipContents? _contents;

        public CodeEditorTooltip()
        {
            InitializeComponent();

            // TODO: Sort this out. Double buffering messes up the drawing when the size is changed
            doubleBufferedPanel1.ToggleDoubleBuffer(false);
        }

        public IToolTipContents? GetContents() => _contents;

        public void Update(SyntaxPalette palette, IToolTipContents contents)
        {
            _palette = palette;
            bool contentsChanged = _contents == null || !_contents.Equals(contents);
            _contents = contents;
            if (contentsChanged)
            {
                Refresh();
            }
        }

        public bool IncrementActiveSuggestion()
        {
            if (_contents == null)
            {
                return false;
            }
            if (_contents.Cycle(1))
            {
                Refresh();
                return true;
            }
            return false;
        }

        public bool DecrementActiveSuggestion()
        {
            if (_contents == null)
            {
                return false;
            }
            if (_contents.Cycle(-1))
            {
                Refresh();
                return true;
            }
            return false;
        }

        private void doubleBufferedPanel1_Paint(object sender, PaintEventArgs e)
        {
            if (_palette == null || _contents == null)
            {
                return;
            }
            e.Graphics.ConfigureHighQuality();

            e.Graphics.Clear(_palette.ToolTipBackColour);
            e.Graphics.DrawRectangle(Pens.Black, 0, 0, Width - 1, Height - 1);
            
            Size newSize = _contents.Draw(new WinformsCanvas(e.Graphics, Size, Font), IconCache.Instance, _palette);
            if (Size != newSize)
            {
                Size = newSize;
                Refresh();
            }
        }
    }
}
