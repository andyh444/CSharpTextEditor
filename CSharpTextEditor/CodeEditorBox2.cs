using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSharpTextEditor
{
    public partial class CodeEditorBox2 : UserControl
    {
        private SourceCode _sourceCode;

        public CodeEditorBox2()
        {
            InitializeComponent();
            _sourceCode = new SourceCode();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);
            int line = 0;
            int characterWidth = TextRenderer.MeasureText("A", Font).Width / 2;
            foreach (string s in _sourceCode.Lines)
            {
                e.Graphics.DrawString(s, Font, Brushes.Black, new PointF(0, line * 16));
                line++;
            }
            if (Focused)
            {
                e.Graphics.DrawLine(Pens.Black, new Point(2 + _sourceCode.CurrentColumnNumber * characterWidth, _sourceCode.CurrentLineNumber * 16), new Point(2 + _sourceCode.CurrentColumnNumber * characterWidth, _sourceCode.CurrentLineNumber * 16 + 16));
            }
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Refresh();
        }

        private void CodeEditorBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
        }

        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            Focus();
            Refresh();
        }

        private void CodeEditorBox2_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyData == Keys.Right
                || e.KeyData == Keys.Left)
            {
                e.IsInputKey = true;
            }
        }

        private void CodeEditorBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back) // backspace
            {
                _sourceCode.RemoveCharacterBeforePosition();
            }
            else if (e.KeyCode == Keys.Delete)
            {
                _sourceCode.RemoveCharacterAfterPosition();
            }
            else if (e.KeyCode == Keys.Left)
            {
                _sourceCode.ShiftActivePositionToTheLeft();
            }
            else if (e.KeyCode == Keys.Right)
            {
                _sourceCode.ShiftActivePositionToTheRight();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                _sourceCode.InsertLineBreakAtPosition();
            }
            else if (GetCharacterFromKeyCode(e.KeyCode, e.Shift, out char? character)
                && character != null)
            {
                _sourceCode.InsertCharacterAtPosition((char)character);
            }
            Refresh();
        }

        private bool GetCharacterFromKeyCode(Keys key, bool shift, out char? character)
        {
            character = null;
            switch (key)
            {
                // letters
                case Keys.A: character = shift ? 'A' : 'a'; break;
                case Keys.B: character = shift ? 'B' : 'b'; break;
                case Keys.C: character = shift ? 'C' : 'c'; break;
                case Keys.D: character = shift ? 'D' : 'd'; break;
                case Keys.E: character = shift ? 'E' : 'e'; break;
                case Keys.F: character = shift ? 'F' : 'f'; break;
                case Keys.G: character = shift ? 'G' : 'g'; break;
                case Keys.H: character = shift ? 'H' : 'h'; break;
                case Keys.I: character = shift ? 'I' : 'i'; break;
                case Keys.J: character = shift ? 'J' : 'j'; break;
                case Keys.K: character = shift ? 'K' : 'k'; break;
                case Keys.L: character = shift ? 'L' : 'l'; break;
                case Keys.M: character = shift ? 'M' : 'm'; break;
                case Keys.N: character = shift ? 'N' : 'n'; break;
                case Keys.O: character = shift ? 'O' : 'o'; break;
                case Keys.P: character = shift ? 'P' : 'p'; break;
                case Keys.Q: character = shift ? 'Q' : 'q'; break;
                case Keys.R: character = shift ? 'R' : 'r'; break;
                case Keys.S: character = shift ? 'S' : 's'; break;
                case Keys.T: character = shift ? 'T' : 't'; break;
                case Keys.U: character = shift ? 'U' : 'u'; break;
                case Keys.V: character = shift ? 'V' : 'v'; break;
                case Keys.W: character = shift ? 'W' : 'w'; break;
                case Keys.X: character = shift ? 'X' : 'x'; break;
                case Keys.Y: character = shift ? 'Y' : 'y'; break;
                case Keys.Z: character = shift ? 'Z' : 'z'; break;

                case Keys.Space: character = ' '; break;
            }
            return character != null;
        }
    }
}
