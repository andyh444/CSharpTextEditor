using CSharpTextEditor.Languages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSharpTextEditor.View.Winforms
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
            _contents = contents;
            Refresh();
        }

        public void IncrementActiveSuggestion()
        {
            if (_contents == null)
            {
                return;
            }
            if (_contents.Cycle(1))
            {
                Refresh();
            }
        }

        public void DecrementActiveSuggestion()
        {
            if (_contents == null)
            {
                return;
            }
            if (_contents.Cycle(-1))
            {
                Refresh();
            }
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
            
            Size newSize = _contents.Draw(e.Graphics, Font, _palette);
            if (Size != newSize)
            {
                Size = newSize;
                Refresh();
            }
        }
    }
}
